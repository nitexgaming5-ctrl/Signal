using NUnit.Framework;
using Signal.Core;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class NpcAgendaInteractionDirectorTests
    {
        [Test]
        public void ChoiceRecordsSourcedRumourAndRelationshipOnce()
        {
            var definition = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            definition.NpcId = "rhea";
            definition.Threads = new[]
            {
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "milo-tip",
                    DisplayName = "A neighbour who notices things",
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "opening",
                            Dialogue = "Rhea shares an uncertain rumour.",
                            RequireSocializingActivity = true,
                            Choices = new[]
                            {
                                new NpcAgendaChoiceDefinition
                                {
                                    ChoiceId = "ask-source",
                                    Label = "Ask where she heard it",
                                    Response = "Someone from the block told me.",
                                    Action = NpcAgendaChoiceAction.RecordDiscovery,
                                    DiscoveryId = "rhea-milo-rumour",
                                    DiscoverySubjectId = "milo-after-hours",
                                    DiscoveryType = DiscoverySignalType.Rumour,
                                    DiscoveryDescription = "Rhea heard from a neighbour that Milo carried parcels after closing.",
                                    DiscoveryConfidence = 0.4f,
                                    FamiliarityDelta = 0.03f
                                }
                            }
                        }
                    }
                }
            };

            var progress = new NpcAgendaProgressBook();
            var discoveries = new PlayerDiscoveryLedger();
            var relationships = new PlayerRelationshipBook();
            var director = new NpcAgendaInteractionDirector(new[] { definition }, progress,
                discoveries, relationships, _ => false, _ => false, () => true, () => 2, () => 600);

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize,
                out NpcAgendaThreadDefinition thread, out NpcAgendaStageDefinition stage), Is.True);
            Assert.That(thread.ThreadId, Is.EqualTo("milo-tip"));
            Assert.That(director.TryChoose("rhea", thread.ThreadId, stage.StageId, "ask-source",
                out string response, out bool completed), Is.True);
            Assert.That(completed, Is.True);
            Assert.That(response, Is.EqualTo("Someone from the block told me."));
            Assert.That(discoveries.HasSignal("rhea-milo-rumour"), Is.True);
            Assert.That(discoveries.TryGet("rhea-milo-rumour", out PlayerDiscoverySignal signal), Is.True);
            Assert.That(signal.Type, Is.EqualTo(DiscoverySignalType.Rumour));
            Assert.That(signal.SourceId, Is.EqualTo("rhea"));
            Assert.That(signal.Confidence, Is.EqualTo(0.4f));
            Assert.That(relationships.Get("rhea").Familiarity, Is.EqualTo(0.03f).Within(0.0001f));

            Assert.That(director.TryChoose("rhea", thread.ThreadId, stage.StageId, "ask-source",
                out _, out _), Is.False);
            Assert.That(relationships.Get("rhea").Familiarity, Is.EqualTo(0.03f).Within(0.0001f));
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void StagePrerequisitesAreEvaluatedWithoutCompletingUnavailableThreads()
        {
            var definition = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            definition.NpcId = "rhea";
            definition.Threads = new[]
            {
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "machine",
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "offer",
                            Dialogue = "The old machine is available.",
                            RequiredCompletedContractId = "contract-one",
                            Choices = new[] { new NpcAgendaChoiceDefinition { ChoiceId = "later", Label = "Later" } }
                        }
                    }
                }
            };
            var director = new NpcAgendaInteractionDirector(new[] { definition },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                id => id == "contract-one", _ => false, () => true, () => 1, () => 0);

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, out _, out _), Is.True);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void RepeatableOfferChoiceDoesNotConsumeStageProgress()
        {
            var definition = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            definition.NpcId = "rhea";
            definition.Threads = new[]
            {
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "machine",
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "offer",
                            Dialogue = "Would you like to see the machine?",
                            Choices = new[]
                            {
                                new NpcAgendaChoiceDefinition
                                {
                                    ChoiceId = "see-offer",
                                    Label = "Take a look",
                                    Action = NpcAgendaChoiceAction.OpenWorkshopUpgradeOffer,
                                    CompleteStageOnChoice = false
                                }
                            }
                        }
                    }
                }
            };
            int opened = 0;
            var progress = new NpcAgendaProgressBook();
            var director = new NpcAgendaInteractionDirector(new[] { definition }, progress,
                new PlayerDiscoveryLedger(), new PlayerRelationshipBook(), _ => false, _ => false,
                () => { opened++; return true; }, () => 1, () => 0);

            Assert.That(director.TryChoose("rhea", "machine", "offer", "see-offer", out _, out bool completed), Is.True);
            Assert.That(completed, Is.False);
            Assert.That(opened, Is.EqualTo(1));
            Assert.That(progress.CompletedStageCount, Is.Zero);
            Object.DestroyImmediate(definition);
        }
    }
}
