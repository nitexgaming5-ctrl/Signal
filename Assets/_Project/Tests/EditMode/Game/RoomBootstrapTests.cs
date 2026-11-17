using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Signal.Game.Tests
{
    public sealed class RoomBootstrapTests
    {
        private GameObject _host;
        private RoomBootstrap _room;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("RoomHost");
            _room = _host.AddComponent<RoomBootstrap>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void BuildCreatesTheFloorTheWallsAndAPlayerInTheMiddle()
        {
            _room.Build(null);

            Tilemap floor = _host.transform.Find("RoomGrid/Floor").GetComponent<Tilemap>();
            Tilemap walls = _host.transform.Find("RoomGrid/Walls").GetComponent<Tilemap>();

            Assert.That(floor.GetUsedTilesCount(), Is.EqualTo(20 * 12));
            Assert.That(walls.GetUsedTilesCount(), Is.EqualTo(2 * 22 + 2 * 12));
            Assert.That(_room.Player, Is.Not.Null);
            Assert.That(_room.Player.position.x, Is.EqualTo(10f).Within(0.01f));
            Assert.That(_room.Player.position.y, Is.EqualTo(6f).Within(0.01f));
            Assert.That(_room.Bounds.width, Is.EqualTo(22f));
            Assert.That(_room.Bounds.height, Is.EqualTo(14f));
        }

        [Test]
        public void TheDoorIsADifferentTileFromTheWall()
        {
            _room.Build(null);

            Tilemap walls = _host.transform.Find("RoomGrid/Walls").GetComponent<Tilemap>();

            Assert.That(walls.GetTile(new Vector3Int(10, -1, 0)), Is.Not.Null);
            Assert.That(walls.GetTile(new Vector3Int(10, -1, 0)), Is.Not.SameAs(walls.GetTile(new Vector3Int(0, -1, 0))));
        }

        [Test]
        public void BuildingTwiceDoesNothingTheSecondTime()
        {
            _room.Build(null);
            Transform firstPlayer = _room.Player;
            int children = _host.transform.childCount;

            _room.Build(null);

            Assert.That(_room.Player, Is.SameAs(firstPlayer));
            Assert.That(_host.transform.childCount, Is.EqualTo(children));
        }

        [Test]
        public void ThePlayerCanMoveAndIsHeldInByColliders()
        {
            _room.Build(null);

            Assert.That(_room.PlayerBody.bodyType, Is.EqualTo(RigidbodyType2D.Dynamic));
            Assert.That(_room.PlayerBody.gravityScale, Is.EqualTo(0f));
            Assert.That(_room.Player.GetComponent<BoxCollider2D>(), Is.Not.Null);
            Assert.That(_host.transform.Find("RoomGrid/Walls").GetComponent<TilemapCollider2D>(), Is.Not.Null);
        }
    }
}
