using System.Collections.Generic;
using NUnit.Framework;
using Signal.Core;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class NpcAgendaInteractionDirectorTests
    {
        private readonly List<NpcAgendaDefinition> _created = new List<NpcAgendaDefinition>();

        [TearDown]
        public void TearDown()
        {
            foreach (NpcAgendaDefinition definition in _created)
                if (definition != null) Object.DestroyImmediate(definition);
            _created.Clear();
        }

        [Test]
        public void RheaAgendaResourceValidatesAndKeepsItsThreeThreadsIndependent()
        {
            NpcAgendaDefinition agenda = Resources.Load<NpcAgendaDefinition>("Agendas/RheaAgenda");
            Assert.That(agenda, Is.Not.Null);
            Assert.That(agenda.TryValidate(out string error), Is.True, error);
            Assert.That(agenda.NpcId, Is.EqualTo("rhea"));
            Assert.That(agenda.Threads.Length, Is.EqualTo(3));

            NpcAgendaStageDefinition rumour = agenda.Threads[0].Stages[0];
            Assert.That(rumour.RequireSocializingActivity, Is.True);
            Assert.That(rumour.RequiredPlace, Is.EqualTo("Street"));
            Assert.That(rumour.CompleteIfLegacyDiscoveryExists, Is.True);
            Assert.That(rumour.LegacyDiscoverySubjectId, Is.EqualTo("milo"));
            Assert.That(rumour.LegacyDiscoverySourceId, Is.EqualTo("rhea"));
            Assert.That(rumour.LegacyDiscoveryType, Is.EqualTo(DiscoverySignalType.Rumour));
            Assert.That(rumour.Choices.Length, Is.EqualTo(3));
            foreach (NpcAgendaChoiceDefinition choice in rumour.Choices)
            {
                Assert.That(choice.Action, Is.EqualTo(NpcAgendaChoiceAction.RecordDiscovery));
                Assert.That(choice.DiscoverySubjectId, Is.EqualTo("milo"));
                Assert.That(choice.DiscoveryType, Is.EqualTo(DiscoverySignalType.Rumour));
                Assert.That(choice.DiscoveryConfidence, Is.EqualTo(0.4f));
            }

            NpcAgendaStageDefinition machine = agenda.Threads[1].Stages[0];
            Assert.That(machine.RequiredActivity, Is.EqualTo(NpcAgendaActivityRequirement.Work));
            Assert.That(machine.RequiredPlace, Is.EqualTo("CornerShop"));
            Assert.That(machine.RequiredContract, Is.Not.Null);
            Assert.That(machine.RequiredUpgrade, Is.Not.Null);
            Assert.That(machine.RequireUpgradeNotAcquired, Is.True);

            NpcAgendaStageDefinition mending = agenda.Threads[2].Stages[0];
            Assert.That(mending.RequiredUpgrade, Is.Not.Null);
            Assert.That(mending.RequireUpgradeNotAcquired, Is.False);
        }

        [Test]
        public void PreviouslyHeardRheaRumourMigratesToCompletedAgendaStage()
        {
            NpcAgendaDefinition agenda = CreateAgenda("rhea", "milo-parcel-rumour", "first-tip",
                Choice("ask-source", NpcAgendaChoiceAction.RecordDiscovery));
            NpcAgendaStageDefinition stage = agenda.Threads[0].Stages[0];
            stage.CompleteIfLegacyDiscoveryExists = true;
            stage.LegacyDiscoverySubjectId = "milo";
            stage.LegacyDiscoverySourceId = "rhea";
            stage.LegacyDiscoveryType = DiscoverySignalType.Rumour;

            var discoveries = new PlayerDiscoveryLedger();
            discoveries.Record(new PlayerDiscoverySignal("milo-rumour-rhea-4", "milo",
                DiscoverySignalType.Rumour, "rhea",
                "Rhea heard that Milo has been carrying parcels after closing, but does not know why.",
                0.4f, 4, 300));
            var progress = new NpcAgendaProgressBook();
            var director = CreateDirector(new[] { agenda }, progress, discoveries,
                new PlayerRelationshipBook());

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street",
                out _, out _), Is.False);
            Assert.That(progress.IsStageCompleted("rhea", "milo-parcel-rumour", "first-tip"), Is.True);
            Assert.That(progress.TryGetChoice("rhea", "milo-parcel-rumour", "first-tip",
                out string migratedChoice), Is.True);
            Assert.That(migratedChoice, Is.EqualTo("legacy-discovery"));
        }

        [Test]
        public void DifferentResidentsUseTheSameDirectorAndKeepIndependentProgress()
        {
            NpcAgendaDefinition rhea = CreateAgenda("rhea", "neighbour-tip", "first-talk",
                Choice("ask-source", NpcAgendaChoiceAction.RecordDiscovery));
            NpcAgendaDefinition milo = CreateAgenda("milo", "bakery-chat", "first-talk",
                Choice("talk", NpcAgendaChoiceAction.DialogueOnly));
            var progress = new NpcAgendaProgressBook();
            var discoveries = new PlayerDiscoveryLedger();
            var relationships = new PlayerRelationshipBook();
            var director = CreateDirector(new[] { rhea, milo }, progress, discoveries, relationships);

            Assert.That(director.HasAgenda("rhea"), Is.True);
            Assert.That(director.HasAgenda("unlisted"), Is.False);
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street",
                out NpcAgendaThreadDefinition rheaThread, out NpcAgendaStageDefinition rheaStage), Is.True);
            Assert.That(rheaThread.ThreadId, Is.EqualTo("neighbour-tip"));
            Assert.That(rheaStage.StageId, Is.EqualTo("first-talk"));

            Assert.That(director.TryChoose("rhea", "neighbour-tip", "first-talk", "ask-source",
                out _, out bool completed), Is.True);
            Assert.That(completed, Is.True);
            Assert.That(progress.IsStageCompleted("rhea", "neighbour-tip", "first-talk"), Is.True);
            Assert.That(progress.IsStageCompleted("milo", "bakery-chat", "first-talk"), Is.False);
            Assert.That(discoveries.TryGet("signal-rhea", out PlayerDiscoverySignal signal), Is.True);
            Assert.That(signal.SourceId, Is.EqualTo("rhea"));
            Assert.That(signal.Type, Is.EqualTo(DiscoverySignalType.Rumour));
            Assert.That(signal.Confidence, Is.EqualTo(0.4f));
            Assert.That(relationships.Get("rhea").Familiarity, Is.EqualTo(0.05f));

            Assert.That(director.TryChoose("rhea", "neighbour-tip", "first-talk", "ask-source",
                out _, out _), Is.False);
            Assert.That(director.TryGetNextStage("milo", NpcActivity.Work, "CornerShop",
                out NpcAgendaThreadDefinition miloThread, out _), Is.True);
            Assert.That(miloThread.ThreadId, Is.EqualTo("bakery-chat"));
        }

        [Test]
        public void StageEligibilityCombinesActivityPlaceContractAndUpgradeState()
        {
            bool contractCompleted = false;
            bool upgradeAcquired = false;
            NpcAgendaDefinition agenda = CreateAgenda("rhea", "machine", "offer",
                Choice("open", NpcAgendaChoiceAction.DialogueOnly));
            NpcAgendaStageDefinition stage = agenda.Threads[0].Stages[0];
            stage.RequiredActivity = NpcAgendaActivityRequirement.Work;
            stage.RequiredPlace = "CornerShop";
            stage.RequiredCompletedContractId = "contract";
            stage.RequiredUpgradeId = "upgrade";
            stage.RequireUpgradeNotAcquired = true;

            var director = CreateDirector(new[] { agenda }, new NpcAgendaProgressBook(),
                new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                id => id == "contract" && contractCompleted,
                id => id == "upgrade" && upgradeAcquired);

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, "CornerShop", out _, out _), Is.False);
            contractCompleted = true;
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "CornerShop", out _, out _), Is.False);
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, "Street", out _, out _), Is.False);
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, "CornerShop", out _, out _), Is.True);
            upgradeAcquired = true;
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, "CornerShop", out _, out _), Is.False);
        }

        [Test]
        public void NonCompletingDialogueChoiceCanBeRevisitedWithoutCreatingProgress()
        {
            NpcAgendaDefinition agenda = CreateAgenda("rhea", "machine", "offer",
                Choice("inspect", NpcAgendaChoiceAction.DialogueOnly, complete: false));
            var progress = new NpcAgendaProgressBook();
            var director = new NpcAgendaInteractionDirector(new[] { agenda },
                progress, new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                _ => true, _ => false, (_, __) => false, _ => true, () => 2, () => 600);

            Assert.That(director.TryChoose("rhea", "machine", "offer", "inspect",
                out string response, out bool completed), Is.True);
            Assert.That(response, Is.EqualTo("Response"));
            Assert.That(completed, Is.False);
            Assert.That(director.TryChoose("rhea", "machine", "offer", "inspect",
                out _, out _), Is.True);
            Assert.That(progress.IsStageCompleted("rhea", "machine", "offer"), Is.False);
        }

        [Test]
        public void StageCanRequireRelationshipThresholdsAndAnOpportunityPhase()
        {
            NpcAgendaDefinition agenda = CreateAgenda("rhea", "follow-up", "trusted-stage",
                Choice("continue", NpcAgendaChoiceAction.DialogueOnly));
            NpcAgendaStageDefinition stage = agenda.Threads[0].Stages[0];
            stage.RequireRelationshipThresholds = true;
            stage.MinimumFamiliarity = 0.2f;
            stage.MinimumTrust = 0.6f;
            stage.MinimumAffinity = 0.1f;
            stage.RequireOpportunityPhase = true;
            stage.RequiredOpportunityId = "milo-parcel";
            stage.RequiredOpportunityPhase = OpportunityPhase.Accepted;

            var relationships = new PlayerRelationshipBook();
            relationships.Set("rhea", 0.25f, 0.7f, 0.2f);
            bool parcelAccepted = false;
            var director = new NpcAgendaInteractionDirector(new[] { agenda },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), relationships,
                _ => false, _ => false,
                (id, phase) => id == "milo-parcel" && phase == OpportunityPhase.Accepted && parcelAccepted,
                _ => true, () => 2, () => 600);

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street", out _, out _), Is.False);
            parcelAccepted = true;
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street", out _, out _), Is.True);

            relationships.Set("rhea", 0.25f, 0.4f, 0.2f);
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street", out _, out _), Is.False);
        }

        [Test]
        public void AgendaRelationshipGateCanReadNpcOpinionOfPlayer()
        {
            NpcAgendaDefinition agenda = CreateAgenda("resident", "opinion-gate", "trusted",
                Choice("continue", NpcAgendaChoiceAction.DialogueOnly));
            NpcAgendaStageDefinition stage = agenda.Threads[0].Stages[0];
            stage.RequireRelationshipThresholds = true;
            stage.RelationshipPerspective = NpcAgendaRelationshipPerspective.NpcToPlayer;
            stage.MinimumFamiliarity = 0.3f;
            stage.MinimumTrust = 0.6f;
            stage.MinimumAffinity = -0.2f;

            var socialWorld = new NpcSocialWorld();
            socialWorld.RegisterNpc("resident", "Street", 0f, 0f, 8f);
            var director = new NpcAgendaInteractionDirector(new[] { agenda },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                _ => false, _ => false, (_, __) => false, _ => true, () => 1, () => 600, socialWorld);

            Assert.That(director.TryGetNextStage("resident", NpcActivity.Socialize, "Street", out _, out _), Is.False);
            socialWorld.SetPlayerRelationship("resident", new NpcRelationship(0.4f, 0.7f, 0f));
            Assert.That(director.TryGetNextStage("resident", NpcActivity.Socialize, "Street", out _, out _), Is.True);
        }

        [Test]
        public void AgendaChoiceCanChangeNpcOpinionOfPlayerWithoutChangingPlayerOpinion()
        {
            NpcAgendaChoiceDefinition choice = Choice("help", NpcAgendaChoiceAction.DialogueOnly);
            choice.RelationshipOutcomePerspective = NpcAgendaRelationshipPerspective.NpcToPlayer;
            choice.FamiliarityDelta = 0.2f;
            choice.TrustDelta = 0.1f;
            choice.AffinityDelta = 0.3f;
            NpcAgendaDefinition agenda = CreateAgenda("resident", "opinion-outcome", "help", choice);

            var socialWorld = new NpcSocialWorld();
            socialWorld.RegisterNpc("resident", "Street", 0f, 0f, 8f);
            var playerRelationships = new PlayerRelationshipBook();
            var director = new NpcAgendaInteractionDirector(new[] { agenda },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), playerRelationships,
                _ => false, _ => false, (_, __) => false, _ => true, () => 1, () => 600, socialWorld);

            Assert.That(director.TryChoose("resident", "opinion-outcome", "help", "help",
                out _, out bool completed), Is.True);
            Assert.That(completed, Is.True);
            Assert.That(socialWorld.GetPlayerRelationship("resident").Familiarity,
                Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(socialWorld.GetPlayerRelationship("resident").Trust,
                Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(socialWorld.GetPlayerRelationship("resident").Affinity,
                Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(playerRelationships.Get("resident").Familiarity, Is.Zero);
        }

        [Test]
        public void NpcOpinionAgendaRulesFailClosedWithoutRegisteredSocialWorldActor()
        {
            NpcAgendaDefinition agenda = CreateAgenda("resident", "opinion-gate", "trusted",
                Choice("continue", NpcAgendaChoiceAction.DialogueOnly));
            NpcAgendaStageDefinition stage = agenda.Threads[0].Stages[0];
            stage.RequireRelationshipThresholds = true;
            stage.RelationshipPerspective = NpcAgendaRelationshipPerspective.NpcToPlayer;
            var director = CreateDirector(new[] { agenda }, new NpcAgendaProgressBook(),
                new PlayerDiscoveryLedger(), new PlayerRelationshipBook());

            Assert.That(director.TryGetNextStage("resident", NpcActivity.Socialize, "Street", out _, out _), Is.False);
        }

        [Test]
        public void AgendaValidationRejectsDuplicateIdsAndIncompleteTypedOutcomes()
        {
            NpcAgendaDefinition agenda = CreateAgenda("rhea", "duplicate", "stage",
                Choice("choice", NpcAgendaChoiceAction.DialogueOnly));
            agenda.Threads = new[]
            {
                Thread("duplicate", 0, Stage("stage-a", NpcAgendaActivityRequirement.Any,
                    Choice("choice-a", NpcAgendaChoiceAction.DialogueOnly))),
                Thread("duplicate", 1, Stage("stage-b", NpcAgendaActivityRequirement.Any,
                    Choice("choice-b", NpcAgendaChoiceAction.DialogueOnly)))
            };
            Assert.That(agenda.TryValidate(out string duplicateError), Is.False);
            Assert.That(duplicateError, Does.Contain("unique"));

            NpcAgendaChoiceDefinition invalidDiscovery = Choice("rumour", NpcAgendaChoiceAction.RecordDiscovery);
            invalidDiscovery.DiscoveryId = string.Empty;
            agenda.Threads = new[]
            {
                Thread("valid-thread", 0, Stage("valid-stage",
                    NpcAgendaActivityRequirement.Any, invalidDiscovery))
            };
            Assert.That(agenda.TryValidate(out string outcomeError), Is.False);
            Assert.That(outcomeError, Does.Contain("Discovery"));
        }

        [Test]
        public void ThreadPriorityIsDeterministicAndDoesNotBlockIndependentThreads()
        {
            NpcAgendaDefinition agenda = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            _created.Add(agenda);
            agenda.NpcId = "rhea";
            agenda.Threads = new[]
            {
                Thread("low", 1, Stage("low-stage", NpcAgendaActivityRequirement.Any,
                    Choice("low-choice", NpcAgendaChoiceAction.DialogueOnly))),
                Thread("high", 10, Stage("high-stage", NpcAgendaActivityRequirement.Work,
                    Choice("high-choice", NpcAgendaChoiceAction.DialogueOnly))),
                Thread("same-priority", 10, Stage("same-stage", NpcAgendaActivityRequirement.Work,
                    Choice("same-choice", NpcAgendaChoiceAction.DialogueOnly))),
                Thread("middle", 5, Stage("middle-stage", NpcAgendaActivityRequirement.Any,
                    Choice("middle-choice", NpcAgendaChoiceAction.DialogueOnly)))
            };
            var director = CreateDirector(new[] { agenda }, new NpcAgendaProgressBook(),
                new PlayerDiscoveryLedger(), new PlayerRelationshipBook());

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, "CornerShop",
                out NpcAgendaThreadDefinition workThread, out _), Is.True);
            Assert.That(workThread.ThreadId, Is.EqualTo("high"));
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Rest, "Street",
                out NpcAgendaThreadDefinition restThread, out _), Is.True);
            Assert.That(restThread.ThreadId, Is.EqualTo("middle"));
        }


        [Test]
        public void RheaAgendaResourceLoadsWithThreeValidatedThreadsAndTypedOfferReference()
        {
            NpcAgendaDefinition definition = Resources.Load<NpcAgendaDefinition>("Agendas/RheaAgenda");
            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.TryValidate(out string error), Is.True, error);
            Assert.That(definition.NpcId, Is.EqualTo("rhea"));
            Assert.That(definition.Threads.Length, Is.EqualTo(3));
            Assert.That(definition.Threads[0].Stages[0].Choices[0].DiscoverySubjectId, Is.EqualTo("milo"));
            Assert.That(definition.Threads[0].Stages[0].Choices[0].DiscoveryType, Is.EqualTo(DiscoverySignalType.Rumour));
            Assert.That(definition.Threads[0].Stages[0].Choices[0].DiscoveryConfidence, Is.EqualTo(0.4f));
            Assert.That(definition.Threads[1].Stages[0].RequiredContract, Is.Not.Null);
            Assert.That(definition.Threads[1].Stages[0].RequiredUpgrade, Is.Not.Null);
            Assert.That(definition.Threads[1].Stages[0].Choices[0].WorkshopUpgradeOffer, Is.Not.Null);

            WorkshopUpgradeDefinition routedOffer = null;
            var typedDirector = new NpcAgendaInteractionDirector(new[] { definition },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                _ => true, _ => false, (_, __) => false,
                upgrade => { routedOffer = upgrade; return true; }, () => 1, () => 0);
            Assert.That(typedDirector.TryChoose("rhea", "compact-sewing-machine", "offer-machine",
                "see-machine", out _, out bool stageCompleted), Is.True);
            Assert.That(stageCompleted, Is.False);
            Assert.That(routedOffer, Is.SameAs(definition.Threads[1].Stages[0].Choices[0].WorkshopUpgradeOffer));
        }

        [Test]
        public void RheaRumourPreservesMiloSubjectAndExistingParcelOfferRequirements()
        {
            var discoveries = new PlayerDiscoveryLedger();
            var relationships = new PlayerRelationshipBook();
            var agenda = CreateAgenda("rhea", "milo-tip", "opening",
                new NpcAgendaChoiceDefinition
                {
                    ChoiceId = "ask-source",
                    Label = "Ask source",
                    Response = "A neighbour told me.",
                    Action = NpcAgendaChoiceAction.RecordDiscovery,
                    DiscoveryId = "rhea-milo-parcel-rumour",
                    DiscoverySubjectId = "milo",
                    DiscoveryType = DiscoverySignalType.Rumour,
                    DiscoveryDescription = "Rhea heard from a neighbour that Milo carried a parcel after closing.",
                    DiscoveryConfidence = 0.4f
                });
            var director = CreateDirector(new[] { agenda }, new NpcAgendaProgressBook(),
                discoveries, relationships);

            Assert.That(director.TryChoose("rhea", "milo-tip", "opening", "ask-source", out _, out _), Is.True);
            discoveries.Record(new PlayerDiscoverySignal("milo-routine-test", "milo",
                DiscoverySignalType.RoutineObservation, "milo",
                "Milo stepped outside after closing with a wrapped parcel.", 1f, 2, 600));
            relationships.Adjust("milo", familiarityDelta: 0.2f);
            var parcelOffer = new OpportunityDefinition("milo-parcel", "milo",
                new InteractionConditionSet(NpcActivity.Socialize, minimumFamiliarity: 0.2f,
                    minimumDistinctSignalTypes: 2));
            var context = new InteractionContext(NpcActivity.Socialize, relationships.Get("milo"), discoveries);
            Assert.That(parcelOffer.OfferConditions.IsSatisfiedBy(context, "milo"), Is.True);
        }

        [Test]
        public void AuthoredAgendaRejectsDuplicateThreadIdsAndMissingWorkshopOfferReference()
        {
            var duplicate = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            _created.Add(duplicate);
            duplicate.NpcId = "resident";
            NpcAgendaStageDefinition first = Stage("first", NpcAgendaActivityRequirement.Any,
                Choice("okay", NpcAgendaChoiceAction.DialogueOnly));
            NpcAgendaStageDefinition second = Stage("second", NpcAgendaActivityRequirement.Any,
                Choice("okay", NpcAgendaChoiceAction.DialogueOnly));
            duplicate.Threads = new[] { Thread("same", 0, first), Thread("same", 0, second) };
            Assert.That(duplicate.TryValidate(out _), Is.False);

            var missingOffer = CreateAgenda("resident", "offer", "show",
                new NpcAgendaChoiceDefinition
                {
                    ChoiceId = "show",
                    Label = "Show me",
                    Response = "Here it is.",
                    Action = NpcAgendaChoiceAction.OpenWorkshopUpgradeOffer
                });
            Assert.That(missingOffer.TryValidate(out _), Is.False);
        }

        private NpcAgendaDefinition CreateAgenda(string npcId, string threadId, string stageId,
            NpcAgendaChoiceDefinition choice)
        {
            NpcAgendaDefinition agenda = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            _created.Add(agenda);
            agenda.NpcId = npcId;
            agenda.Threads = new[] { Thread(threadId, 0, Stage(stageId,
                NpcAgendaActivityRequirement.Any, choice)) };
            return agenda;
        }

        private static NpcAgendaThreadDefinition Thread(string id, int priority,
            NpcAgendaStageDefinition stage)
        {
            return new NpcAgendaThreadDefinition
            {
                ThreadId = id,
                DisplayName = id,
                Priority = priority,
                Stages = new[] { stage }
            };
        }

        private static NpcAgendaStageDefinition Stage(string id,
            NpcAgendaActivityRequirement activity, params NpcAgendaChoiceDefinition[] choices)
        {
            return new NpcAgendaStageDefinition
            {
                StageId = id,
                Title = id,
                Dialogue = "Dialogue",
                RequiredActivity = activity,
                Choices = choices
            };
        }

        private static NpcAgendaChoiceDefinition Choice(string id, NpcAgendaChoiceAction action,
            bool complete = true)
        {
            return new NpcAgendaChoiceDefinition
            {
                ChoiceId = id,
                Label = id,
                Response = "Response",
                Action = action,
                CompleteStageOnChoice = complete,
                DiscoveryId = action == NpcAgendaChoiceAction.RecordDiscovery ? "signal-rhea" : null,
                DiscoverySubjectId = action == NpcAgendaChoiceAction.RecordDiscovery ? "milo" : null,
                DiscoveryType = DiscoverySignalType.Rumour,
                DiscoveryDescription = action == NpcAgendaChoiceAction.RecordDiscovery ? "Rhea heard something." : null,
                DiscoveryConfidence = 0.4f,
                FamiliarityDelta = action == NpcAgendaChoiceAction.RecordDiscovery ? 0.05f : 0f
            };
        }

        private static NpcAgendaInteractionDirector CreateDirector(IEnumerable<NpcAgendaDefinition> agendas,
            NpcAgendaProgressBook progress, PlayerDiscoveryLedger discoveries,
            PlayerRelationshipBook relationships, System.Func<string, bool> contract = null,
            System.Func<string, bool> upgrade = null)
        {
            return new NpcAgendaInteractionDirector(agendas, progress, discoveries, relationships,
                contract ?? (_ => false), upgrade ?? (_ => false), (_, __) => false, _ => true,
                () => 2, () => 600);
        }
    }
}
