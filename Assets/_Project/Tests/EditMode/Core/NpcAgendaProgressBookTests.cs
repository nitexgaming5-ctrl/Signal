using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class NpcAgendaProgressBookTests
    {
        [Test]
        public void CompletesStageOnceAndRetainsSelectedChoice()
        {
            var book = new NpcAgendaProgressBook();
            int changed = 0;
            book.Changed += _ => changed++;

            Assert.That(book.TryCompleteStage("rhea", "milo-tip", "first-talk", "ask-source"), Is.True);
            Assert.That(book.TryCompleteStage("rhea", "milo-tip", "first-talk", "leave"), Is.False);
            Assert.That(book.IsStageCompleted("rhea", "milo-tip", "first-talk"), Is.True);
            Assert.That(book.TryGetChoice("rhea", "milo-tip", "first-talk", out string choice), Is.True);
            Assert.That(choice, Is.EqualTo("ask-source"));
            Assert.That(changed, Is.EqualTo(1));
        }

        [Test]
        public void IndependentThreadsAndResidentsDoNotShareProgress()
        {
            var book = new NpcAgendaProgressBook();
            book.TryCompleteStage("rhea", "milo-tip", "opening", "ask-source");

            Assert.That(book.IsStageCompleted("rhea", "machine-offer", "opening"), Is.False);
            Assert.That(book.IsStageCompleted("milo", "milo-tip", "opening"), Is.False);
            Assert.That(book.CompletedStageCount, Is.EqualTo(1));
        }

        [Test]
        public void RestoreRejectsDuplicateWithoutOverwritingChoice()
        {
            var book = new NpcAgendaProgressBook();
            Assert.That(book.TryRestore(new NpcAgendaStageProgress("rhea", "thread", "stage", "first")), Is.True);
            Assert.That(book.TryRestore(new NpcAgendaStageProgress("rhea", "thread", "stage", "second")), Is.False);
            Assert.That(book.TryGetChoice("rhea", "thread", "stage", out string choice), Is.True);
            Assert.That(choice, Is.EqualTo("first"));
        }

        [Test]
        public void RejectsMissingStableIdsAndChoiceIds()
        {
            var book = new NpcAgendaProgressBook();
            Assert.Throws<ArgumentException>(() => book.TryCompleteStage("", "thread", "stage", "choice"));
            Assert.Throws<ArgumentException>(() => book.TryCompleteStage("rhea", "thread", "stage", ""));
            Assert.Throws<ArgumentException>(() => new NpcAgendaStageProgress("rhea", "", "stage", "choice"));
        }

        [Test]
        public void RestoreRejectsDefaultUninitializedRecord()
        {
            var book = new NpcAgendaProgressBook();
            Assert.That(book.TryRestore(default), Is.False);
            Assert.That(book.CompletedStageCount, Is.Zero);
        }

        [Test]
        public void ClearEmitsOneChangeOnlyWhenThereWasProgress()
        {
            var book = new NpcAgendaProgressBook();
            int changed = 0;
            book.Changed += _ => changed++;
            book.Clear();
            book.TryCompleteStage("rhea", "thread", "stage", "choice");
            book.Clear();
            book.Clear();
            Assert.That(changed, Is.EqualTo(2));
            Assert.That(book.CompletedStageCount, Is.Zero);
        }
    }
}
