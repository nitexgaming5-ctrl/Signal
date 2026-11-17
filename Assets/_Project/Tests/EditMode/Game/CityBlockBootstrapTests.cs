using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class CityBlockBootstrapTests
    {
        [Test]
        public void SavedLocationCanRestorePlayerAndActiveInterior()
        {
            var host = new GameObject("CityHost");
            var player = new GameObject("Player");
            var cameraObject = new GameObject("MainCamera");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            Camera camera = cameraObject.AddComponent<Camera>();
            CityBlockBootstrap city = host.AddComponent<CityBlockBootstrap>();

            try
            {
                city.Build(player.transform, body, camera);

                Assert.That(city.TryRestorePlace("CornerShop", new Vector2(1001f, 998f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("CornerShop"));
                Assert.That(body.position, Is.EqualTo(new Vector2(1001f, 998f)));

                Assert.That(city.TryRestorePlace("Apartment", new Vector2(1021f, 997f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("Apartment"));
                Assert.That(body.position, Is.EqualTo(new Vector2(1021f, 997f)));

                Assert.That(city.TryRestorePlace("NotAPlace", Vector2.zero), Is.False);
                Assert.That(city.CurrentPlace, Is.EqualTo("Apartment"));
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void PlayerCanTravelBetweenHideoutStreetAndBothInteriors()
        {
            var host = new GameObject("CityHost");
            var player = new GameObject("Player");
            var cameraObject = new GameObject("MainCamera");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            Camera camera = cameraObject.AddComponent<Camera>();
            CameraFollow2D follow = cameraObject.AddComponent<CameraFollow2D>();
            CityBlockBootstrap city = host.AddComponent<CityBlockBootstrap>();

            try
            {
                city.Build(player.transform, body, camera);
                Assert.That(city.CurrentPlace, Is.EqualTo("Hideout"));

                Assert.That(city.TryInteract(new Vector2(10f, -1.5f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("Street"));
                Assert.That(city.TryInteract(new Vector2(0.5f, -10.8f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("CornerShop"));
                Assert.That(city.TryInteract(new Vector2(1000f, 995.8f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("Street"));

                Assert.That(city.TryInteract(new Vector2(19.5f, -10.8f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("Apartment"));
                Assert.That(city.TryInteract(new Vector2(1020f, 995.8f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("Street"));

                Assert.That(city.TryInteract(new Vector2(10f, -1.5f)), Is.True);
                Assert.That(city.CurrentPlace, Is.EqualTo("Hideout"));
                Assert.That(follow, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}