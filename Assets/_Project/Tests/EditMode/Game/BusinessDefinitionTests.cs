using System;
using System.Reflection;
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

        [Test]
        public void ValidAuthoredCataloguePassesValidationAndRejectsDuplicateItems()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            ItemDefinition bread = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                SetField(business, "businessId", "test-bakery");
                SetField(business, "displayName", "Test Bakery");
                SetField(business, "businessType", "Bakery");
                SetField(business, "ownerNpcId", "milo");
                SetField(business, "locationId", "CornerShop");
                SetField(bread, "id", Guid.NewGuid().ToString());
                SetField(bread, "displayName", "Bread");

                var first = new BusinessDefinition.MerchandiseEntry { Item = bread, UnitPrice = 5 };
                SetField(business, "merchandise", new[] { first });
                Assert.That(business.TryValidate(out string error), Is.True, error);

                SetField(business, "merchandise", new[]
                {
                    first,
                    new BusinessDefinition.MerchandiseEntry { Item = bread, UnitPrice = 8 }
                });
                Assert.That(business.TryValidate(out error), Is.False);
                Assert.That(error, Does.Contain("more than once"));
            }
            finally
            {
                Object.DestroyImmediate(bread);
                Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void MissingOwnerAndNonPositiveMerchandisePriceAreRejected()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            ItemDefinition bread = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                SetField(business, "businessId", "test-bakery");
                SetField(business, "displayName", "Test Bakery");
                SetField(business, "businessType", "Bakery");
                SetField(business, "ownerNpcId", "");
                SetField(business, "locationId", "CornerShop");
                SetField(bread, "id", Guid.NewGuid().ToString());
                SetField(bread, "displayName", "Bread");
                SetField(business, "merchandise", new[]
                {
                    new BusinessDefinition.MerchandiseEntry { Item = bread, UnitPrice = 0 }
                });

                Assert.That(business.TryValidate(out string error), Is.False);
                Assert.That(error, Does.Contain("owner"));

                SetField(business, "ownerNpcId", "milo");
                Assert.That(business.TryValidate(out error), Is.False);
                Assert.That(error, Does.Contain("positive fixed price"));
            }
            finally
            {
                Object.DestroyImmediate(bread);
                Object.DestroyImmediate(business);
            }
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Expected serialized field " + fieldName);
            field.SetValue(target, value);
        }
    }
}
