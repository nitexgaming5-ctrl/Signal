using System.Collections.Generic;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Grid-based A* path planning inside one navigation region. Geometry is sampled from
    /// CityNavigationMap; clearance expands obstacles so an NPC's centre does not skim walls.
    /// The grid is cached because city geometry is static after CityBlockBootstrap.Build.
    /// </summary>
    public sealed class CityNavigationPathfinder
    {
        private sealed class Grid
        {
            public readonly int Width;
            public readonly int Height;
            public readonly Vector2[] Positions;
            public readonly bool[] Walkable;

            public Grid(CityNavigationMap map, string placeId, float cellSize, float clearance)
            {
                CityNavigationMap.Region region = map.FindRegion(placeId);
                Width = Mathf.CeilToInt(region.Bounds.width / cellSize);
                Height = Mathf.CeilToInt(region.Bounds.height / cellSize);
                Positions = new Vector2[Width * Height];
                Walkable = new bool[Width * Height];
                for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    int index = Index(x, y);
                    Vector2 point = new Vector2(region.Bounds.xMin + (x + 0.5f) * cellSize,
                        region.Bounds.yMin + (y + 0.5f) * cellSize);
                    Positions[index] = point;
                    Walkable[index] = map.IsWalkable(placeId, point, clearance);
                }
            }

            public int Index(int x, int y) => y * Width + x;
            public bool Contains(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        }

        private struct HeapEntry
        {
            public int Index;
            public float Priority;
            public HeapEntry(int index, float priority) { Index = index; Priority = priority; }
        }

        private sealed class MinHeap
        {
            private readonly List<HeapEntry> _entries = new List<HeapEntry>();
            public int Count => _entries.Count;

            public void Push(int index, float priority)
            {
                _entries.Add(new HeapEntry(index, priority));
                int child = _entries.Count - 1;
                while (child > 0)
                {
                    int parent = (child - 1) / 2;
                    if (_entries[parent].Priority <= _entries[child].Priority) break;
                    HeapEntry swap = _entries[parent];
                    _entries[parent] = _entries[child];
                    _entries[child] = swap;
                    child = parent;
                }
            }

            public HeapEntry Pop()
            {
                HeapEntry result = _entries[0];
                HeapEntry last = _entries[_entries.Count - 1];
                _entries.RemoveAt(_entries.Count - 1);
                if (_entries.Count == 0) return result;
                _entries[0] = last;
                int parent = 0;
                while (true)
                {
                    int left = parent * 2 + 1;
                    int right = left + 1;
                    if (left >= _entries.Count) break;
                    int smallest = right < _entries.Count &&
                        _entries[right].Priority < _entries[left].Priority ? right : left;
                    if (_entries[parent].Priority <= _entries[smallest].Priority) break;
                    HeapEntry swap = _entries[parent];
                    _entries[parent] = _entries[smallest];
                    _entries[smallest] = swap;
                    parent = smallest;
                }
                return result;
            }
        }

        private readonly CityNavigationMap _map;
        private readonly float _cellSize;
        private readonly float _clearance;
        private readonly float _maxSnapDistance;
        private readonly Dictionary<string, Grid> _grids = new Dictionary<string, Grid>();
        public float CellSize => _cellSize;
        public float Clearance => _clearance;

        public CityNavigationPathfinder(CityNavigationMap map, float cellSize = 0.35f,
            float clearance = 0.38f, float maxSnapDistance = 1.75f)
        {
            if (map == null) throw new System.ArgumentNullException(nameof(map));
            if (cellSize <= 0f || clearance < 0f || maxSnapDistance < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(cellSize));
            _map = map;
            _cellSize = cellSize;
            _clearance = clearance;
            _maxSnapDistance = maxSnapDistance;
        }

        /// <summary>
        /// Returns a clearance-safe route. An authored destination on furniture is moved
        /// to the nearest safe grid cell within the configured snap distance.
        /// </summary>
        public bool TryFindPath(string placeId, Vector2 start, Vector2 goal, out List<Vector2> path)
        {
            path = new List<Vector2>();
            if (_map.FindRegion(placeId) == null) return false;
            Grid grid = GetGrid(placeId);
            if (!TryFindNearestWalkable(grid, start, out int startIndex) ||
                !TryFindNearestWalkable(grid, goal, out int goalIndex))
                return false;
            if (!TryFindGridPath(grid, startIndex, goalIndex, out List<int> rawPath))
                return false;

            List<int> smoothPath = SmoothPath(placeId, grid, rawPath);
            Vector2 firstGridPoint = grid.Positions[smoothPath[0]];
            if (_map.IsWalkable(placeId, start, _clearance) &&
                HasLineOfSight(placeId, start, firstGridPoint))
                AddDistinct(path, start);
            else
                AddDistinct(path, firstGridPoint);

            for (int i = 1; i < smoothPath.Count; i++)
                AddDistinct(path, grid.Positions[smoothPath[i]]);

            Vector2 lastPoint = path[path.Count - 1];
            if (_map.IsWalkable(placeId, goal, _clearance) &&
                HasLineOfSight(placeId, lastPoint, goal))
                AddDistinct(path, goal);
            if (path.Count == 0)
                path.Add(grid.Positions[smoothPath[smoothPath.Count - 1]]);
            return true;
        }

        private Grid GetGrid(string placeId)
        {
            if (!_grids.TryGetValue(placeId, out Grid grid))
            {
                grid = new Grid(_map, placeId, _cellSize, _clearance);
                _grids.Add(placeId, grid);
            }
            return grid;
        }

        private bool TryFindNearestWalkable(Grid grid, Vector2 point, out int nearestIndex)
        {
            nearestIndex = -1;
            float bestDistance = _maxSnapDistance * _maxSnapDistance;
            for (int i = 0; i < grid.Positions.Length; i++)
            {
                if (!grid.Walkable[i]) continue;
                float distance = (grid.Positions[i] - point).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                nearestIndex = i;
            }
            return nearestIndex >= 0;
        }

        private bool TryFindGridPath(Grid grid, int startIndex, int goalIndex, out List<int> path)
        {
            path = new List<int>();
            if (startIndex == goalIndex) { path.Add(startIndex); return true; }

            int count = grid.Positions.Length;
            var costs = new float[count];
            var parents = new int[count];
            var closed = new bool[count];
            for (int i = 0; i < count; i++)
            {
                costs[i] = float.PositiveInfinity;
                parents[i] = -1;
            }

            var open = new MinHeap();
            costs[startIndex] = 0f;
            open.Push(startIndex, Heuristic(grid, startIndex, goalIndex));
            bool found = false;

            while (open.Count > 0)
            {
                int current = open.Pop().Index;
                if (closed[current]) continue;
                if (current == goalIndex) { found = true; break; }
                closed[current] = true;
                int x = current % grid.Width;
                int y = current / grid.Width;

                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (!grid.Contains(nx, ny)) continue;
                    int next = grid.Index(nx, ny);
                    if (!grid.Walkable[next] || closed[next]) continue;
                    if (dx != 0 && dy != 0 &&
                        (!grid.Walkable[grid.Index(x + dx, y)] ||
                         !grid.Walkable[grid.Index(x, y + dy)]))
                        continue;

                    float stepCost = dx != 0 && dy != 0 ? 1.41421356f : 1f;
                    float candidate = costs[current] + stepCost;
                    if (candidate >= costs[next]) continue;
                    costs[next] = candidate;
                    parents[next] = current;
                    open.Push(next, candidate + Heuristic(grid, next, goalIndex));
                }
            }

            if (!found) return false;
            int cursor = goalIndex;
            while (cursor >= 0)
            {
                path.Add(cursor);
                if (cursor == startIndex) break;
                cursor = parents[cursor];
            }
            if (path[path.Count - 1] != startIndex) { path.Clear(); return false; }
            path.Reverse();
            return true;
        }

        private static float Heuristic(Grid grid, int from, int to)
        {
            int dx = Mathf.Abs(from % grid.Width - to % grid.Width);
            int dy = Mathf.Abs(from / grid.Width - to / grid.Width);
            int diagonal = Mathf.Min(dx, dy);
            return diagonal * 1.41421356f + Mathf.Max(dx, dy) - diagonal;
        }

        private List<int> SmoothPath(string placeId, Grid grid, List<int> rawPath)
        {
            var result = new List<int>();
            if (rawPath.Count == 0) return result;
            int current = 0;
            result.Add(rawPath[0]);
            while (current < rawPath.Count - 1)
            {
                int next = rawPath.Count - 1;
                while (next > current + 1 &&
                    !HasLineOfSight(placeId, grid.Positions[rawPath[current]],
                        grid.Positions[rawPath[next]]))
                    next--;
                result.Add(rawPath[next]);
                current = next;
            }
            return result;
        }

        private bool HasLineOfSight(string placeId, Vector2 from, Vector2 to)
        {
            float distance = Vector2.Distance(from, to);
            int samples = Mathf.Max(1, Mathf.CeilToInt(distance / (_cellSize * 0.35f)));
            for (int i = 0; i <= samples; i++)
            {
                Vector2 point = Vector2.Lerp(from, to, i / (float)samples);
                if (!_map.IsWalkable(placeId, point, _clearance)) return false;
            }
            return true;
        }

        private static void AddDistinct(List<Vector2> points, Vector2 point)
        {
            if (points.Count == 0 || (points[points.Count - 1] - point).sqrMagnitude > 0.0025f)
                points.Add(point);
        }
    }
}