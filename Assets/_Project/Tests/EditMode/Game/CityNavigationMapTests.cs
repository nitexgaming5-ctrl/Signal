using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class CityNavigationMapTests
    {
        [Test]
        public void WalkabilityCombinesRegionBoundsAndSolidObstacles()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Street", new Rect(-5f, -5f, 10f, 10f));
            map.AddObstacle("Street", "Bench", new Rect(1f, 1f, 2f, 1f));

            Assert.That(map.IsWalkable("Street", Vector2.zero), Is.True);
            Assert.That(map.IsWalkable("Street", new Vector2(1.5f, 1.5f)), Is.False);
            Assert.That(map.IsWalkable("Street", new Vector2(8f, 0f)), Is.False);
            Assert.That(map.IsBlocked("Other", Vector2.zero), Is.False);
        }

        [Test]
        public void DoorwayConnectionsCreateAConnectedLocationGraph()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Hideout", new Rect(0f, 0f, 10f, 10f));
            map.AddRegion("Street", new Rect(0f, 0f, 20f, 20f));
            map.AddRegion("Shop", new Rect(0f, 0f, 10f, 10f));
            map.AddConnection("home-door", "Hideout", new Vector2(5f, 0f),
                "Street", new Vector2(5f, 1f));
            map.AddConnection("shop-door", "Street", new Vector2(15f, 1f),
                "Shop", new Vector2(5f, 0f));

            Assert.That(map.AreLocationsConnected("Hideout", "Shop"), Is.True);
            Assert.That(map.AreLocationsConnected("Shop", "Hideout"), Is.True);
            Assert.That(map.GetConnectionsFor("Street").Count, Is.EqualTo(2));
            Assert.That(map.AreLocationsConnected("Shop", "Missing"), Is.False);
        }

        [Test]
        public void DuplicateRegionsAndConnectionsAreRejected()
        {
            var map = new CityNavigationMap();
            map.AddRegion("Street", new Rect(0f, 0f, 10f, 10f));
            map.AddRegion("Shop", new Rect(0f, 0f, 10f, 10f));

            Assert.Throws<System.InvalidOperationException>(() =>
                map.AddRegion("Street", new Rect(0f, 0f, 10f, 10f)));

            map.AddConnection("door", "Street", Vector2.zero, "Shop", Vector2.zero);
            Assert.Throws<System.InvalidOperationException>(() =>
                map.AddConnection("door", "Street", Vector2.zero, "Shop", Vector2.zero));
        }
    }
}
