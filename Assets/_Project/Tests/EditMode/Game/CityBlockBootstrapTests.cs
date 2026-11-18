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
        public void FallbackResidentsUseSharedPresenterAndCanBeTalkedTo()
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
                Assert.That(city.GetResidentActivity("rhea"), Is.EqualTo(NpcActivity.Rest));
                Vector2 rheaPosition = city.GetResidentPosition("rhea");
                Assert.That(city.GetPrompt(rheaPosition), Is.Empty);
                Assert.That(city.TryRestorePlace("Street", rheaPosition), Is.True);
                Assert.That(city.GetPrompt(rheaPosition), Is.EqualTo("E  Talk to Rhea"));
                Assert.That(city.TryInteractNpc(rheaPosition, out string rheaDialogue), Is.True);
                StringAssert.Contains("Rhea:", rheaDialogue);
                Assert.That(city.SocialWorld.BuildPerceptionFor("rhea", 8f).KnowsEvent("rhea-conversation-1"), Is.True);
                Assert.That(city.TryRestorePlace("CornerShop", new Vector2(1000f, 999f)), Is.True);
                Assert.That(city.GetResidentActivity("milo"), Is.EqualTo(NpcActivity.Work));

                Vector2 npcPosition = city.GetResidentPosition("milo");
                Assert.That(city.GetPrompt(npcPosition), Is.EqualTo("E  Talk to Milo"));
                Assert.That(city.TryInteractNpc(npcPosition, out string workDialogue), Is.True);
                StringAssert.Contains("baker", workDialogue);
                Assert.That(city.SocialWorld.EventCount, Is.EqualTo(2));
                Assert.That(city.SocialWorld.BuildPerceptionFor("milo", 8f).KnowsEvent("milo-conversation-1"), Is.True);

                clock.Advance(11 * 60);
                Assert.That(city.GetResidentActivity("milo"), Is.EqualTo(NpcActivity.Socialize));
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState travellingMilo), Is.True);
                Assert.That(travellingMilo.LocationId, Is.EqualTo("CornerShop"));
                city.DebugAdvanceNpcMovement(30f);
                Assert.That(city.TryRestorePlace("Street", city.GetResidentPosition("milo")), Is.True);
                Vector2 eveningNpcPosition = city.GetResidentPosition("milo");
                Assert.That(city.TryInteractNpc(eveningNpcPosition, out string eveningDialogue), Is.True);
                StringAssert.Contains("Evenings are quieter", eveningDialogue);

                clock.Advance(3 * 60);
                Assert.That(city.GetResidentActivity("milo"), Is.EqualTo(NpcActivity.Home));
                city.DebugAdvanceNpcMovement(30f);
                Assert.That(city.TryRestorePlace("Apartment", city.GetResidentPosition("milo")), Is.True);
                Vector2 homeNpcPosition = city.GetResidentPosition("milo");
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
        public void ResidentWalksThroughStreetWhenRoutineChangesLocation()
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
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState initial), Is.True);
                Assert.That(initial.LocationId, Is.EqualTo("CornerShop"));

                clock.Advance(11 * 60);
                Assert.That(city.GetResidentActivity("milo"), Is.EqualTo(NpcActivity.Socialize));
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState leavingShop), Is.True);
                Assert.That(leavingShop.LocationId, Is.EqualTo("CornerShop"),
                    "A schedule change sets a destination; it does not teleport the NPC.");

                city.DebugAdvanceNpcMovement(30f);
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState arrivedStreet), Is.True);
                Assert.That(arrivedStreet.LocationId, Is.EqualTo("Street"));
                Assert.That(city.GetResidentPosition("milo").x, Is.EqualTo(18.2f).Within(0.05f));
                Assert.That(city.GetResidentPosition("milo").y, Is.EqualTo(-11.6f).Within(0.05f));

                clock.Advance(3 * 60);
                Assert.That(city.GetResidentActivity("milo"), Is.EqualTo(NpcActivity.Home));
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState leavingStreet), Is.True);
                Assert.That(leavingStreet.LocationId, Is.EqualTo("Street"));

                city.DebugAdvanceNpcMovement(30f);
                Assert.That(city.SocialWorld.TryGetNpc("milo", out NpcAgentState arrivedApartment), Is.True);
                Assert.That(arrivedApartment.LocationId, Is.EqualTo("Apartment"));
                Assert.That(city.GetResidentPosition("milo").x, Is.EqualTo(1016.5f).Within(0.05f));
                Assert.That(city.GetResidentPosition("milo").y, Is.EqualTo(1000f).Within(0.05f));
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void ResidentsDevelopFamiliarityWhenTheirSchedulesBringThemTogether()
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
                Assert.That(city.SocialEncounterCount, Is.EqualTo(0));

                clock.Advance(10 * 60);

                Assert.That(city.GetResidentActivity("milo"), Is.EqualTo(NpcActivity.Socialize));
                Assert.That(city.GetResidentActivity("rhea"), Is.EqualTo(NpcActivity.Socialize));
                Assert.That(city.SocialEncounterCount, Is.EqualTo(0),
                    "Milo is still travelling from the shop and must not socialize before arriving.");

                city.DebugAdvanceNpcMovement(30f);
                clock.Advance(1);
                Assert.That(city.SocialEncounterCount, Is.EqualTo(1));

                NpcRelationship miloToRhea = city.SocialWorld.GetRelationship("milo", "rhea");
                NpcRelationship rheaToMilo = city.SocialWorld.GetRelationship("rhea", "milo");
                Assert.That(miloToRhea.Familiarity, Is.EqualTo(0.01f).Within(0.0001f));
                Assert.That(rheaToMilo.Familiarity, Is.EqualTo(0.01f).Within(0.0001f));
                Assert.That(miloToRhea.Trust, Is.EqualTo(0.5f));
                Assert.That(rheaToMilo.Trust, Is.EqualTo(0.5f));

                clock.Advance(1);
                Assert.That(city.SocialEncounterCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void BuildUsesSuppliedCharacterDefinitionsWithTheSamePresenter()
        {
            var host = new GameObject("CityHost");
            var player = new GameObject("Player");
            var cameraObject = new GameObject("MainCamera");
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            Camera camera = cameraObject.AddComponent<Camera>();
            var clock = new GameClock(1, 8 * 60);
            NpcCharacterDefinition shopkeeper = ScriptableObject.CreateInstance<NpcCharacterDefinition>();
            NpcCharacterDefinition resident = ScriptableObject.CreateInstance<NpcCharacterDefinition>();
            shopkeeper.NpcId = "test-shopkeeper";
            shopkeeper.DisplayName = "Shopkeeper";
            shopkeeper.PublicRole = "Vendor";
            shopkeeper.UtilityProfile = NpcUtilityProfile.CreateDefault();
            resident.NpcId = "test-resident";
            resident.DisplayName = "Resident";
            resident.PublicRole = "Neighbour";
            resident.UtilityProfile = NpcUtilityProfile.CreateDefault();
            resident.UtilityProfile.WorkStartMinute = 540;
            resident.SocialPosition = new Vector2(4.8f, -17.5f);
            CityBlockBootstrap city = host.AddComponent<CityBlockBootstrap>();

            try
            {
                city.Build(player.transform, body, camera, clock, new[] { shopkeeper, resident });

                Assert.That(city.ResidentCount, Is.EqualTo(2));
                Assert.That(city.SocialWorld.NpcCount, Is.EqualTo(2));
                Assert.That(city.SocialWorld.TryGetNpc("test-shopkeeper", out _), Is.True);
                Assert.That(city.SocialWorld.TryGetNpc("test-resident", out _), Is.True);
                Assert.That(city.GetResidentActivity("test-resident"), Is.EqualTo(NpcActivity.Rest));
                Vector2 residentPosition = city.GetResidentPosition("test-resident");
                Assert.That(city.TryRestorePlace("Street", residentPosition), Is.True);
                Assert.That(city.GetPrompt(residentPosition), Is.EqualTo("E  Talk to Resident"));
                Assert.That(city.TryInteractNpc(residentPosition, out string dialogue), Is.True);
                StringAssert.Contains("Resident:", dialogue);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(shopkeeper);
                Object.DestroyImmediate(resident);
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