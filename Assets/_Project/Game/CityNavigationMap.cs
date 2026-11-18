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

}
