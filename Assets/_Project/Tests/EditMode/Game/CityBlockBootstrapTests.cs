using NUnit.Framework;
using Signal.Core;
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
        public void NeighbourBakerHasARoutineAndCanBeTalkedTo()
        {
            var host = new GameObject("CityHost");
            var player = new GameObject("Player");
            var cameraObject = new GameObject("MainCamera");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            Camera camera = cameraObject.AddComponent<Camera>();
            var clock = new GameClock(1, 8 * 60);
            CityBlockBootstrap city = host.AddComponent<CityBlockBootstrap>();

            try
            {
                city.Build(player.transform, body, camera, clock);
                Assert.That(city.SocialWorld, Is.Not.Null);
                Assert.That(city.SocialWorld.NpcCount, Is.EqualTo(2));
                Assert.That(city.ResidentCount, Is.EqualTo(2));
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState registeredMilo), Is.True);
                Assert.That(registeredMilo.LocationId, Is.EqualTo("CornerShop"));
                Assert.That(city.SocialWorld.TryGetNpc("rhea", out NpcAgentState registeredRhea), Is.True);
                Assert.That(registeredRhea.LocationId, Is.EqualTo("Street"));
                Assert.That(city.GetResidentActivity("rhea"), Is.EqualTo(NpcActivity.Socialize));
                Vector2 rheaPosition = city.GetResidentPosition("rhea");
                Assert.That(city.GetPrompt(rheaPosition), Is.EqualTo("E  Talk to Rhea"));
                Assert.That(city.TryInteractNpc(rheaPosition, out string rheaDialogue), Is.True);
                StringAssert.Contains("Rhea:", rheaDialogue);
                Assert.That(city.SocialWorld.BuildPerceptionFor("rhea", 8f).KnowsEvent("rhea-conversation-1"), Is.True);
                Assert.That(city.TryRestorePlace("CornerShop", new Vector2(1000f, 999f)), Is.True);
                Assert.That(city.NeighbourActivity, Is.EqualTo(NpcActivity.Work));

                Vector2 npcPosition = city.NeighbourPosition;
                Assert.That(city.GetPrompt(npcPosition), Is.EqualTo("E  Talk to Milo"));
                Assert.That(city.TryInteractNpc(npcPosition, out string workDialogue), Is.True);
                StringAssert.Contains("baker", workDialogue);
                Assert.That(city.SocialWorld.EventCount, Is.EqualTo(1));
                Assert.That(city.SocialWorld.BuildPerceptionFor("milo", 8f).KnowsEvent("milo-conversation-1"), Is.True);

                clock.Advance(11 * 60);
                Assert.That(city.NeighbourActivity, Is.EqualTo(NpcActivity.Socialize));
                Assert.That(city.TryRestorePlace("Street", city.NeighbourPosition), Is.True);
                Vector2 eveningNpcPosition = city.NeighbourPosition;
                Assert.That(city.TryInteractNpc(eveningNpcPosition, out string eveningDialogue), Is.True);
                StringAssert.Contains("After closing", eveningDialogue);

                clock.Advance(3 * 60);
                Assert.That(city.NeighbourActivity, Is.EqualTo(NpcActivity.Home));
                Assert.That(city.TryRestorePlace("Apartment", city.NeighbourPosition), Is.True);
                Vector2 homeNpcPosition = city.NeighbourPosition;
                Assert.That(city.TryInteractNpc(homeNpcPosition, out string homeDialogue), Is.True);
                StringAssert.Contains("Shop's closed", homeDialogue);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void OffscreenWorldEventsReachNpcsInTheirOwnLocation()
        {
            var host = new GameObject("CityHost");
            var player = new GameObject("Player");
            var cameraObject = new GameObject("MainCamera");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            Camera camera = cameraObject.AddComponent<Camera>();
            var clock = new GameClock(1, 8 * 60);
            CityBlockBootstrap city = host.AddComponent<CityBlockBootstrap>();

            try
            {
                city.Build(player.transform, body, camera, clock);
                Assert.That(city.CurrentPlace, Is.EqualTo("Hideout"));
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState milo), Is.True);
                Assert.That(milo.LocationId, Is.EqualTo("CornerShop"));

                city.PublishWorldEventAt("CornerShop", "delivery-outside", "delivery",
                    "A delivery van stopped outside the bakery.", new Vector2(1000f, 1000.4f), 3f);

                NpcPerception miloView = city.SocialWorld.BuildPerceptionFor("milo", 8f);
                Assert.That(miloView.KnowsEvent("delivery-outside"), Is.True);
                Assert.That(miloView.FindEvent("delivery-outside").Origin,
                    Is.EqualTo(NpcKnowledgeOrigin.DirectObservation));
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