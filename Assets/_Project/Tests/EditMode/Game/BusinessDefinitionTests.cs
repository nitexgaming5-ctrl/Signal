using NUnit.Framework;
using UnityEngine;

namespace Signal.Game.Tests
{
    public sealed class BusinessDefinitionTests
    {
        [Test]
        public void DefaultBusinessScheduleUsesAnInclusiveOpenAndExclusiveClose()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            try
            {
                Assert.That(business.IsOpen(389), Is.False);
                Assert.That(business.IsOpen(390), Is.True);
                Assert.That(business.IsOpen(1079), Is.True);
                Assert.That(business.IsOpen(1080), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void DefaultOwnerUsesTheResidentEconomicActorConvention()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            try
            {
                Assert.That(business.OwnerActor.Value, Is.EqualTo("resident:milo"));
                Assert.That(business.LocationId, Is.EqualTo("CornerShop"));
            }
            finally
            {
                Object.DestroyImmediate(business);
            }
        }
    }
}
