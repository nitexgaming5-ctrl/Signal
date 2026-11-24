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
                discoveries, relationships, _ => false, _ => false, (_, __) => false, _ => true, () => 2, () => 600);

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
                            Choices = new[] { new NpcAgendaChoiceDefinition { ChoiceId = "later", Label = "Later", Response = "Maybe another time." } }
                        }
                    }
                }
            };
            var director = new NpcAgendaInteractionDirector(new[] { definition },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                id => id == "contract-one", _ => false, (_, __) => false, _ => true, () => 1, () => 0);

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, out _, out _), Is.True);
            Object.DestroyImmediate(definition);
        }


        [Test]
        public void PriorityActivityAndLocationConditionsAreGenericAcrossResidents()
        {
            var rhea = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            rhea.NpcId = "rhea";
            rhea.Threads = new[]
            {
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "rumour",
                    DisplayName = "Rumour",
                    Priority = 5,
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "street-talk",
                            Dialogue = "A rumour.",
                            RequiredActivity = NpcAgendaActivityRequirement.Socialize,
                            RequiredPlace = "Street",
                            Choices = new[] { new NpcAgendaChoiceDefinition { ChoiceId = "leave", Label = "Leave", Response = "Fine." } }
                        }
                    }
                },
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "offer",
                    DisplayName = "Offer",
                    Priority = 20,
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "machine",
                            Dialogue = "An offer.",
                            RequiredActivity = NpcAgendaActivityRequirement.Socialize,
                            RequiredCompletedContractId = "contract-one",
                            RequiredUpgradeId = "upgrade-one",
                            RequireUpgradeNotAcquired = true,
                            Choices = new[] { new NpcAgendaChoiceDefinition { ChoiceId = "later", Label = "Later", Response = "Later." } }
                        }
                    }
                }
            };
            var milo = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            milo.NpcId = "milo";
            milo.Threads = new[]
            {
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "everyday",
                    DisplayName = "Everyday",
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "hello",
                            Dialogue = "Long day at the bakery.",
                            Choices = new[] { new NpcAgendaChoiceDefinition { ChoiceId = "goodbye", Label = "Goodbye", Response = "See you." } }
                        }
                    }
                }
            };

            bool contractCompleted = true;
            bool upgradeAcquired = false;
            var director = new NpcAgendaInteractionDirector(new[] { rhea, milo },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), new PlayerRelationshipBook(),
                _ => contractCompleted, _ => upgradeAcquired, (_, __) => false, _ => true, () => 1, () => 0);

            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Work, "Street", out _, out _), Is.False);
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street",
                out NpcAgendaThreadDefinition selectedThread, out _), Is.True);
            Assert.That(selectedThread.ThreadId, Is.EqualTo("offer"));
            Assert.That(director.TryGetNextStage("milo", NpcActivity.Work, "CornerShop",
                out NpcAgendaThreadDefinition miloThread, out _), Is.True);
            Assert.That(miloThread.ThreadId, Is.EqualTo("everyday"));

            contractCompleted = false;
            upgradeAcquired = true;
            Assert.That(director.TryGetNextStage("rhea", NpcActivity.Socialize, "Street",
                out selectedThread, out _), Is.True);
            Assert.That(selectedThread.ThreadId, Is.EqualTo("rumour"));

            Object.DestroyImmediate(rhea);
            Object.DestroyImmediate(milo);
        }


        [Test]
        public void RelationshipAndOpportunityConditionsGateAStage()
        {
            var definition = ScriptableObject.CreateInstance<NpcAgendaDefinition>();
            definition.NpcId = "resident";
            definition.Threads = new[]
            {
                new NpcAgendaThreadDefinition
                {
                    ThreadId = "follow-up",
                    DisplayName = "Follow-up",
                    Stages = new[]
                    {
                        new NpcAgendaStageDefinition
                        {
                            StageId = "trusted-opportunity",
                            Dialogue = "A private follow-up.",
                            RequireRelationshipThresholds = true,
                            MinimumFamiliarity = 0.2f,
                            MinimumTrust = 0.4f,
                            RequireOpportunityPhase = true,
                            RequiredOpportunityId = "parcel-opportunity",
                            RequiredOpportunityPhase = OpportunityPhase.Resolved,
                            Choices = new[] { new NpcAgendaChoiceDefinition { ChoiceId = "talk", Label = "Talk", Response = "Let's talk." } }
                        }
                    }
                }
            };

            var relationships = new PlayerRelationshipBook();
            bool opportunityResolved = false;
            var director = new NpcAgendaInteractionDirector(new[] { definition },
                new NpcAgendaProgressBook(), new PlayerDiscoveryLedger(), relationships,
                _ => false, _ => false, (_, __) => opportunityResolved, _ => true,
                () => 1, () => 0);

            Assert.That(director.TryGetNextStage("resident", NpcActivity.Socialize, "Street", out _, out _), Is.False);
            relationships.Set("resident", 0.3f, 0.5f, 0f);
            Assert.That(director.TryGetNextStage("resident", NpcActivity.Socialize, "Street", out _, out _), Is.False);
            opportunityResolved = true;
            Assert.That(director.TryGetNextStage("resident", NpcActivity.Socialize, "Street", out _, out _), Is.True);
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
                                    Response = "Have a look.",
                                    Action = NpcAgendaChoiceAction.DialogueOnly,
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
                new PlayerDiscoveryLedger(), new PlayerRelationshipBook(), _ => false, _ => false, (_, __) => false,
                _ => { opened++; return true; }, () => 1, () => 0);

            Assert.That(director.TryChoose("rhea", "machine", "offer", "see-offer", out _, out bool completed), Is.True);
            Assert.That(completed, Is.False);
            Assert.That(opened, Is.Zero);
            Assert.That(progress.CompletedStageCount, Is.Zero);
            Object.DestroyImmediate(definition);
        }
    }
}
