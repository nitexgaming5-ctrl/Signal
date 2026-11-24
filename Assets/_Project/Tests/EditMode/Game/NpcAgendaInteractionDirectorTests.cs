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
            Assert.That(signal.Confidence, Is.EqualTo(0.35f));
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
                _ => true, _ => false, _ => true, () => 2, () => 600);

            Assert.That(director.TryChoose("rhea", "machine", "offer", "inspect",
                out string response, out bool completed), Is.True);
            Assert.That(response, Is.EqualTo("Response"));
            Assert.That(completed, Is.False);
            Assert.That(director.TryChoose("rhea", "machine", "offer", "inspect",
                out _, out _), Is.True);
            Assert.That(progress.IsStageCompleted("rhea", "machine", "offer"), Is.False);
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
                DiscoveryConfidence = 0.35f,
                FamiliarityDelta = action == NpcAgendaChoiceAction.RecordDiscovery ? 0.05f : 0f
            };
        }

        private static NpcAgendaInteractionDirector CreateDirector(IEnumerable<NpcAgendaDefinition> agendas,
            NpcAgendaProgressBook progress, PlayerDiscoveryLedger discoveries,
            PlayerRelationshipBook relationships, System.Func<string, bool> contract = null,
            System.Func<string, bool> upgrade = null)
        {
            return new NpcAgendaInteractionDirector(agendas, progress, discoveries, relationships,
                contract ?? (_ => false), upgrade ?? (_ => false), _ => true,
                () => 2, () => 600);
        }
    }
}
