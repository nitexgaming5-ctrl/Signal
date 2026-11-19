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
                UnityEngine.Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void ValidAuthoredCataloguePassesValidationAndDuplicateItemsAreRejected()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            ItemDefinition bread = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                SetPrivate(business, "businessId", "test-bakery");
                SetPrivate(business, "displayName", "Test Bakery");
                SetPrivate(business, "businessType", "Bakery");
                SetPrivate(business, "ownerNpcId", "milo");
                SetPrivate(business, "locationId", "CornerShop");
                SetPrivate(bread, "id", Guid.NewGuid().ToString());
                SetPrivate(bread, "displayName", "Bread");

                var breadOffer = new BusinessDefinition.MerchandiseEntry
                {
                    Item = bread,
                    UnitPrice = 5
                };

                SetPrivate(business, "merchandise", new[] { breadOffer });
                Assert.That(business.TryValidate(out string error), Is.True, error);

                SetPrivate(business, "merchandise", new[]
                {
                    breadOffer,
                    new BusinessDefinition.MerchandiseEntry { Item = bread, UnitPrice = 8 }
                });

                Assert.That(business.TryValidate(out error), Is.False);
                Assert.That(error, Does.Contain("same item"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bread);
                UnityEngine.Object.DestroyImmediate(business);
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
                UnityEngine.Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void MissingOwnerAndNonPositiveMerchandisePriceAreRejected()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            ItemDefinition bread = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                SetPrivate(business, "businessId", "test-bakery");
                SetPrivate(business, "displayName", "Test Bakery");
                SetPrivate(business, "businessType", "Bakery");
                SetPrivate(business, "ownerNpcId", string.Empty);
                SetPrivate(business, "locationId", "CornerShop");
                SetPrivate(bread, "id", Guid.NewGuid().ToString());
                SetPrivate(bread, "displayName", "Bread");
                SetPrivate(business, "merchandise", new[]
                {
                    new BusinessDefinition.MerchandiseEntry { Item = bread, UnitPrice = 0 }
                });

                Assert.That(business.TryValidate(out string error), Is.False);
                Assert.That(error, Does.Contain("owner"));

                SetPrivate(business, "ownerNpcId", "milo");
                Assert.That(business.TryValidate(out error), Is.False);
                Assert.That(error, Does.Contain("positive fixed price"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bread);
                UnityEngine.Object.DestroyImmediate(business);
            }
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Expected serialized field " + fieldName);
            field.SetValue(target, value);
        }
    }
}
