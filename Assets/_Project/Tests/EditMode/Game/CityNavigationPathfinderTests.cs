using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class CityNavigationPathfinderTests
    {
        [Test]
        public void FindsClearanceSafeRouteAroundAnObstacle()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Street", new Rect(-5f, -5f, 10f, 10f));
            map.AddObstacle("Street", "Wall", new Rect(-0.5f, -2f, 1f, 4f));
            var pathfinder = new CityNavigationPathfinder(map, 0.25f, 0.3f, 2f);
            Vector2 start = new Vector2(-3f, 0f);
            Vector2 goal = new Vector2(3f, 0f);

            Assert.That(pathfinder.TryFindPath("Street", start, goal, out List<Vector2> path), Is.True);
            Assert.That(path.Count, Is.GreaterThan(2));
            Assert.That(path[0], Is.EqualTo(start));
            Assert.That(path[path.Count - 1], Is.EqualTo(goal));

            for (int segment = 1; segment < path.Count; segment++)
            {
                Vector2 from = path[segment - 1];
                Vector2 to = path[segment];
                int samples = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / 0.05f));
                for (int sample = 0; sample <= samples; sample++)
                    Assert.That(map.IsWalkable("Street",
                        Vector2.Lerp(from, to, sample / (float)samples), 0.3f), Is.True);
            }
        }

        [Test]
        public void ReturnsFalseWhenObstacleSeparatesTheEntireRegion()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Room", new Rect(0f, 0f, 10f, 8f));
            map.AddObstacle("Room", "Full wall", new Rect(4.8f, 0f, 0.4f, 8f));
            var pathfinder = new CityNavigationPathfinder(map, 0.25f, 0.25f, 1f);
            Assert.That(pathfinder.TryFindPath("Room", new Vector2(2f, 4f),
                new Vector2(8f, 4f), out _), Is.False);
        }

        [Test]
        public void SnapsAnAuthoredDestinationAwayFromFurniture()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Street", new Rect(-5f, -5f, 10f, 10f));
            map.AddObstacle("Street", "Bench", new Rect(-0.8f, -0.5f, 1.6f, 1f));
            var pathfinder = new CityNavigationPathfinder(map, 0.25f, 0.3f, 1.75f);
            Assert.That(pathfinder.TryFindPath("Street", new Vector2(-3f, 0f),
                Vector2.zero, out List<Vector2> path), Is.True);
            Assert.That(path[path.Count - 1], Is.Not.EqualTo(Vector2.zero));
            Assert.That(map.IsWalkable("Street", path[path.Count - 1], 0.3f), Is.True);
        }

        [Test]
        public void DoesNotCutDiagonallyBetweenTouchingObstacles()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Room", new Rect(0f, 0f, 2f, 2f));
            map.AddObstacle("Room", "Block A", new Rect(1f, 0f, 1f, 1f));
            map.AddObstacle("Room", "Block B", new Rect(0f, 1f, 1f, 1f));
            var pathfinder = new CityNavigationPathfinder(map, 1f, 0f, 0.1f);
            Assert.That(pathfinder.TryFindPath("Room", new Vector2(0.5f, 0.5f),
                new Vector2(1.5f, 1.5f), out _), Is.False);
        }
    }
}