using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerRelationshipBookTests
    {
        [Test]
        public void RelationshipsArePlayerDirectedAndDefaultToNeutral()
        {
            var book = new PlayerRelationshipBook();

            Assert.That(book.Get("milo").Familiarity, Is.EqualTo(0f));
            Assert.That(book.Get("milo").Trust, Is.EqualTo(0.5f));
            Assert.That(book.Get("milo").Affinity, Is.EqualTo(0f));
            Assert.That(book.Count, Is.Zero);
        }

        [Test]
        public void AdjustClampsValuesAndKeepsNpcEntriesSeparate()
        {
            var book = new PlayerRelationshipBook();
            book.Adjust("milo", familiarityDelta: 0.2f, trustDelta: 0.1f);
            book.Adjust("rhea", familiarityDelta: 0.1f, trustDelta: -0.2f);

            Assert.That(book.Get("milo").Familiarity, Is.EqualTo(0.2f));
            Assert.That(book.Get("milo").Trust, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(book.Get("rhea").Trust, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(book.Count, Is.EqualTo(2));
        }

        [Test]
        public void RejectsInvalidIdsAndNonFiniteValues()
        {
            var book = new PlayerRelationshipBook();
            Assert.Throws<System.ArgumentException>(() => book.Adjust(""));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => book.Adjust("milo", trustDelta: float.NaN));
        }
    }
}
