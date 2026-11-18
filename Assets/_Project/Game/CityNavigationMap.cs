using System;
using System.Collections.Generic;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Geometry and doorway connectivity for one coordinate-space location.
    /// This is deliberately a navigation description, not a pathfinder.
    /// </summary>
    public sealed class CityNavigationMap
    {
        public sealed class Region
        {
            public string PlaceId { get; }
            public Rect Bounds { get; }

            internal Region(string placeId, Rect bounds)
            {
                PlaceId = placeId;
                Bounds = bounds;
            }
        }

        public sealed class Obstacle
        {
            public string PlaceId { get; }
            public string Label { get; }
            public Rect Bounds { get; }

            internal Obstacle(string placeId, string label, Rect bounds)
            {
                PlaceId = placeId;
                Label = label;
                Bounds = bounds;
            }

            public bool Contains(Vector2 point)
            {
                return Bounds.Contains(point);
            }
        }

        public sealed class Connection
        {
            public string Id { get; }
            public string FromPlace { get; }
            public Vector2 FromPoint { get; }
            public string ToPlace { get; }
            public Vector2 ToPoint { get; }

            internal Connection(string id, string fromPlace, Vector2 fromPoint, string toPlace, Vector2 toPoint)
            {
                Id = id;
                FromPlace = fromPlace;
                FromPoint = fromPoint;
                ToPlace = toPlace;
                ToPoint = toPoint;
            }

            public bool Connects(string placeId)
            {
                return FromPlace == placeId || ToPlace == placeId;
            }

            public string OtherPlace(string placeId)
            {
                if (FromPlace == placeId) return ToPlace;
                if (ToPlace == placeId) return FromPlace;
                return string.Empty;
            }

            public Vector2 PointFor(string placeId)
            {
                if (FromPlace == placeId) return FromPoint;
                if (ToPlace == placeId) return ToPoint;
                return Vector2.zero;
            }
        }

        private readonly List<Region> _regions = new List<Region>();
        private readonly List<Obstacle> _obstacles = new List<Obstacle>();
        private readonly List<Connection> _connections = new List<Connection>();

        public IReadOnlyList<Region> Regions => _regions;
        public IReadOnlyList<Obstacle> Obstacles => _obstacles;
        public IReadOnlyList<Connection> Connections => _connections;

        public void AddRegion(string placeId, Rect bounds)
        {
            if (string.IsNullOrWhiteSpace(placeId) || bounds.width <= 0f || bounds.height <= 0f)
                throw new ArgumentException("A navigation region needs a place ID and positive bounds.");

            if (FindRegion(placeId) != null)
                throw new InvalidOperationException("A navigation region already exists for " + placeId + ".");

            _regions.Add(new Region(placeId, bounds));
        }

        public void AddObstacle(string placeId, string label, Rect bounds)
        {
            if (FindRegion(placeId) == null)
                throw new InvalidOperationException("Add the navigation region before its obstacles.");
            if (bounds.width <= 0f || bounds.height <= 0f)
                return;

            _obstacles.Add(new Obstacle(placeId, label ?? "Obstacle", bounds));
        }

        public void AddConnection(string id, string fromPlace, Vector2 fromPoint, string toPlace, Vector2 toPoint)
        {
            if (string.IsNullOrWhiteSpace(id) || FindRegion(fromPlace) == null || FindRegion(toPlace) == null)
                throw new ArgumentException("A navigation connection must reference two existing locations.");
            if (fromPlace == toPlace)
                throw new ArgumentException("A doorway connection must join two different locations.");
            if (_connections.Exists(connection => connection.Id == id))
                throw new InvalidOperationException("A navigation connection already exists with ID " + id + ".");

            _connections.Add(new Connection(id, fromPlace, fromPoint, toPlace, toPoint));
        }

        public Region FindRegion(string placeId)
        {
            for (int i = 0; i < _regions.Count; i++)
                if (_regions[i].PlaceId == placeId)
                    return _regions[i];
            return null;
        }

        public bool IsInsideRegion(string placeId, Vector2 point)
        {
            Region region = FindRegion(placeId);
            return region != null && region.Bounds.Contains(point);
        }

        public bool IsBlocked(string placeId, Vector2 point)
        {
            for (int i = 0; i < _obstacles.Count; i++)
                if (_obstacles[i].PlaceId == placeId && _obstacles[i].Contains(point))
                    return true;
            return false;
        }

        /// <summary>Point-level geometry query. Character radius and clearance are future additions.</summary>
        public bool IsWalkable(string placeId, Vector2 point)
        {
            return IsInsideRegion(placeId, point) && !IsBlocked(placeId, point);
        }

        public List<Connection> GetConnectionsFor(string placeId)
        {
            var result = new List<Connection>();
            for (int i = 0; i < _connections.Count; i++)
                if (_connections[i].Connects(placeId))
                    result.Add(_connections[i]);
            return result;
        }

        public bool AreLocationsConnected(string fromPlace, string toPlace)
        {
            if (fromPlace == toPlace && FindRegion(fromPlace) != null)
                return true;

            var visited = new HashSet<string> { fromPlace };
            var pending = new Queue<string>();
            pending.Enqueue(fromPlace);
            while (pending.Count > 0)
            {
                string current = pending.Dequeue();
                for (int i = 0; i < _connections.Count; i++)
                {
                    string next = _connections[i].OtherPlace(current);
                    if (string.IsNullOrEmpty(next) || !visited.Add(next))
                        continue;
                    if (next == toPlace)
                        return true;
                    pending.Enqueue(next);
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Optional runtime overlay for inspecting the authored walkable regions, solid geometry,
    /// doorway links and current resident routes. Toggle from the F1 developer menu.
    /// </summary>
    public sealed class CityNavigationDebugView : MonoBehaviour
    {
        private CityNavigationMap _map;
        private CityBlockBootstrap _city;
        private Transform _player;
        private Camera _camera;
        private bool _visible;

        public bool IsVisible => _visible;

        public void Bind(CityNavigationMap map, CityBlockBootstrap city, Transform player, Camera camera)
        {
            _map = map;
            _city = city;
            _player = player;
            _camera = camera;
        }

        public bool Toggle()
        {
            _visible = !_visible;
            return _visible;
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
        }

        private void OnGUI()
        {
            if (!_visible || _map == null || _city == null)
                return;

            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null)
                return;

            string place = _city.CurrentPlace;
            CityNavigationMap.Region region = _map.FindRegion(place);
            if (region != null)
            {
                Rect area = ToScreenRect(region.Bounds);
                DrawRect(area, new Color(0.15f, 0.9f, 0.35f, 0.10f));
                GUI.color = Color.white;
                GUI.Label(new Rect(area.x + 4f, area.y + 3f, 230f, 22f),
                    "WALKABLE REGION: " + place);
            }

            for (int i = 0; i < _map.Obstacles.Count; i++)
            {
                CityNavigationMap.Obstacle obstacle = _map.Obstacles[i];
                if (obstacle.PlaceId != place)
                    continue;
                DrawRect(ToScreenRect(obstacle.Bounds), new Color(1f, 0.15f, 0.12f, 0.32f));
                GUI.color = Color.white;
                Vector2 labelPoint = ToScreenPoint(obstacle.Bounds.center);
                GUI.Label(new Rect(labelPoint.x + 3f, labelPoint.y - 9f, 150f, 18f), obstacle.Label);
            }

            List<CityNavigationMap.Connection> connections = _map.GetConnectionsFor(place);
            for (int i = 0; i < connections.Count; i++)
                DrawMarker(connections[i].PointFor(place), "DOOR: " + connections[i].OtherPlace(place),
                    new Color(1f, 0.8f, 0.1f, 1f));

            if (_player != null)
                DrawMarker(_player.position, "PLAYER", Color.cyan);

            IReadOnlyList<NpcPresenter> residents = _city.DebugResidents;
            for (int i = 0; i < residents.Count; i++)
            {
                NpcPresenter resident = residents[i];
                if (resident == null || resident.CurrentPlace != place)
                    continue;

                DrawMarker(resident.Position, resident.DisplayName + " / " + resident.Activity,
                    new Color(1f, 0.85f, 0.3f, 1f));

                Vector2 previous = resident.Position;
                IReadOnlyList<NpcTravelWaypoint> route = resident.DebugTravelRoute;
                for (int waypointIndex = resident.DebugNextWaypoint; waypointIndex < route.Count; waypointIndex++)
                {
                    NpcTravelWaypoint waypoint = route[waypointIndex];
                    if (waypoint.LocationId != place)
                        break;
                    DrawLine(previous, waypoint.Position, new Color(0.2f, 0.85f, 1f, 0.95f), 3f);
                    DrawMarker(waypoint.Position, "PATH", new Color(0.2f, 0.85f, 1f, 1f));
                    previous = waypoint.Position;
                }
            }

            GUI.color = Color.white;
            GUI.Box(new Rect(12f, 12f, 300f, 48f),
                "NAVIGATION DEBUG  [F1 to close menu]\nGreen: region  Red: solid  Yellow: doorway  Cyan: route");
        }

        private Rect ToScreenRect(Rect worldBounds)
        {
            Vector3 lower = _camera.WorldToScreenPoint(new Vector3(worldBounds.xMin, worldBounds.yMin, 0f));
            Vector3 upper = _camera.WorldToScreenPoint(new Vector3(worldBounds.xMax, worldBounds.yMax, 0f));
            return new Rect(lower.x, Screen.height - upper.y,
                Mathf.Abs(upper.x - lower.x), Mathf.Abs(upper.y - lower.y));
        }

        private Vector2 ToScreenPoint(Vector2 worldPoint)
        {
            Vector3 point = _camera.WorldToScreenPoint(worldPoint);
            return new Vector2(point.x, Screen.height - point.y);
        }

        private void DrawMarker(Vector2 worldPoint, string label, Color color)
        {
            Vector2 point = ToScreenPoint(worldPoint);
            GUI.color = color;
            GUI.DrawTexture(new Rect(point.x - 5f, point.y - 5f, 10f, 10f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(point.x + 7f, point.y - 10f, 210f, 20f), label);
        }

        private void DrawRect(Rect screenRect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawLine(Vector2 fromWorld, Vector2 toWorld, Color color, float thickness)
        {
            Vector2 from = ToScreenPoint(fromWorld);
            Vector2 to = ToScreenPoint(toWorld);
            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length <= 0.01f)
                return;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg,
                new Vector2(from.x + delta.x * 0.5f, from.y + delta.y * 0.5f));
            GUI.color = color;
            GUI.DrawTexture(new Rect(from.x, from.y - thickness * 0.5f, length, thickness),
                Texture2D.whiteTexture);
            GUI.matrix = oldMatrix;
            GUI.color = Color.white;
        }
    }
}
