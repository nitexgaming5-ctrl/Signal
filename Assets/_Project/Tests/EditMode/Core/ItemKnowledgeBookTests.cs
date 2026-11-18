using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ItemKnowledgeBookTests
    {
        [Test]
        public void KnowledgeCanBePartialWithoutRevealingUnrecordedProperties()
        {
            var book = new ItemKnowledgeBook();
            Guid itemId = Guid.NewGuid();

            book.RecordFact(itemId, ItemKnowledgeAspect.Condition, (int)ItemCondition.Worn,
                ItemKnowledgeSource.DirectInspection, 1f, 2, 600);

            Assert.That(book.TryGetFact(itemId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact condition), Is.True);
            Assert.That(condition.Value, Is.EqualTo((int)ItemCondition.Worn));
            Assert.That(book.TryGetFact(itemId, ItemKnowledgeAspect.Authenticity, out _), Is.False);
            Assert.That(book.TryGetKnownState(itemId, out _), Is.False);
        }

        [Test]
        public void FullKnowledgeCanReconstructTheKnownState()
        {
            var book = new ItemKnowledgeBook();
            Guid itemId = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Stolen);

            book.RecordFullState(itemId, state, ItemKnowledgeSource.DirectInspection, 1f, 3, 720);

            Assert.That(book.FactCount, Is.EqualTo(4));
            Assert.That(book.TryGetKnownState(itemId, out ItemInstanceState known), Is.True);
            Assert.That(known, Is.EqualTo(state));
        }

        [Test]
        public void RumourCanRecordAnUncertainBeliefThatDiffersFromTheActualItemState()
        {
            var book = new ItemKnowledgeBook();
            Guid itemId = Guid.NewGuid();
            var actual = new ItemInstanceState(ItemQuality.Standard, ItemCondition.Good,
                ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Stolen);

            book.RecordFact(itemId, ItemKnowledgeAspect.Authenticity, (int)ItemAuthenticity.Genuine,
                ItemKnowledgeSource.Rumour, 0.4f, 1, 900);

            Assert.That(book.TryGetFact(itemId, ItemKnowledgeAspect.Authenticity, out ItemKnowledgeFact belief), Is.True);
            Assert.That(belief.Value, Is.EqualTo((int)ItemAuthenticity.Genuine));
            Assert.That(belief.Confidence, Is.EqualTo(0.4f));
            Assert.That(belief.Source, Is.EqualTo(ItemKnowledgeSource.Rumour));
            Assert.That(belief.Value, Is.Not.EqualTo((int)actual.Authenticity));
        }

        [Test]
        public void NewEvidenceReplacesThePreviousBeliefForThatAspectOnly()
        {
            var book = new ItemKnowledgeBook();
            Guid itemId = Guid.NewGuid();
            book.RecordFact(itemId, ItemKnowledgeAspect.Quality, (int)ItemQuality.Poor,
                ItemKnowledgeSource.Rumour, 0.3f, 1, 100);
            book.RecordFact(itemId, ItemKnowledgeAspect.Condition, (int)ItemCondition.Worn,
                ItemKnowledgeSource.DirectInspection, 1f, 1, 110);

            book.RecordFact(itemId, ItemKnowledgeAspect.Quality, (int)ItemQuality.Fine,
                ItemKnowledgeSource.Document, 0.9f, 1, 120);

            Assert.That(book.FactCount, Is.EqualTo(2));
            Assert.That(book.TryGetFact(itemId, ItemKnowledgeAspect.Quality, out ItemKnowledgeFact quality), Is.True);
            Assert.That(quality.Value, Is.EqualTo((int)ItemQuality.Fine));
            Assert.That(quality.Source, Is.EqualTo(ItemKnowledgeSource.Document));
            Assert.That(book.TryGetFact(itemId, ItemKnowledgeAspect.Condition, out _), Is.True);
        }

        [Test]
        public void ClearRemovesAllPlayerKnowledge()
        {
            var book = new ItemKnowledgeBook();
            book.RecordFact(Guid.NewGuid(), ItemKnowledgeAspect.Quality, (int)ItemQuality.Standard,
                ItemKnowledgeSource.DirectInspection, 1f, 1, 0);

            book.Clear();

            Assert.That(book.FactCount, Is.Zero);
            Assert.That(book.Facts, Is.Empty);
        }

        [Test]
        public void KnowledgeFactRejectsInvalidIdentityValuesAndTime()
        {
            Assert.Throws<ArgumentException>(() => new ItemKnowledgeFact(Guid.Empty,
                ItemKnowledgeAspect.Quality, (int)ItemQuality.Standard,
                ItemKnowledgeSource.DirectInspection, 1f, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemKnowledgeFact(Guid.NewGuid(),
                ItemKnowledgeAspect.Quality, 99, ItemKnowledgeSource.DirectInspection, 1f, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemKnowledgeFact(Guid.NewGuid(),
                ItemKnowledgeAspect.Quality, (int)ItemQuality.Standard,
                ItemKnowledgeSource.Rumour, 1.1f, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemKnowledgeFact(Guid.NewGuid(),
                ItemKnowledgeAspect.Quality, (int)ItemQuality.Standard,
                ItemKnowledgeSource.DirectInspection, 1f, 1, 1440));
        }
    }
}
