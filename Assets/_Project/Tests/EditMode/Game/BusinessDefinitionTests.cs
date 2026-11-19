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

        [Test]
        public void BuybackOffersAreIndependentFromMerchandiseAndReturnTheirFixedPrice()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            BusinessDefinition secondBusiness = ScriptableObject.CreateInstance<BusinessDefinition>();
            ItemDefinition bread = ScriptableObject.CreateInstance<ItemDefinition>();
            ItemDefinition water = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                SetPrivate(business, "businessId", "test-shop");
                SetPrivate(business, "displayName", "Test Shop");
                SetPrivate(business, "businessType", "Shop");
                SetPrivate(business, "ownerNpcId", "milo");
                SetPrivate(business, "locationId", "CornerShop");
                SetPrivate(bread, "id", Guid.NewGuid().ToString());
                SetPrivate(bread, "displayName", "Bread");
                SetPrivate(water, "id", Guid.NewGuid().ToString());
                SetPrivate(water, "displayName", "Water");
                SetPrivate(business, "merchandise", new[]
                {
                    new BusinessDefinition.MerchandiseEntry { Item = bread, UnitPrice = 5 }
                });
                SetPrivate(business, "buybackOffers", new[]
                {
                    new BusinessDefinition.BuybackEntry { Item = water, UnitPrice = 3 }
                });

                Assert.That(business.TryValidate(out string error), Is.True, error);
                Assert.That(business.TryGetOffer(bread, out int retailPrice), Is.True);
                Assert.That(retailPrice, Is.EqualTo(5));
                Assert.That(business.TryGetBuybackOffer(water, out int buybackPrice), Is.True);
                Assert.That(buybackPrice, Is.EqualTo(3));
                Assert.That(business.TryGetBuybackOffer(bread, out _), Is.False);
                Assert.That(business.TryGetOffer(water, out _), Is.False);

                SetPrivate(secondBusiness, "businessId", "second-shop");
                SetPrivate(secondBusiness, "displayName", "Second Shop");
                SetPrivate(secondBusiness, "businessType", "Shop");
                SetPrivate(secondBusiness, "ownerNpcId", "rhea");
                SetPrivate(secondBusiness, "locationId", "CornerShop");
                SetPrivate(secondBusiness, "buybackOffers", new[]
                {
                    new BusinessDefinition.BuybackEntry { Item = bread, UnitPrice = 11 }
                });
                Assert.That(secondBusiness.TryValidate(out error), Is.True, error);
                Assert.That(secondBusiness.TryGetBuybackOffer(bread, out int secondPrice), Is.True);
                Assert.That(secondPrice, Is.EqualTo(11));
                Assert.That(secondBusiness.TryGetBuybackOffer(water, out _), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(secondBusiness);
                UnityEngine.Object.DestroyImmediate(water);
                UnityEngine.Object.DestroyImmediate(bread);
                UnityEngine.Object.DestroyImmediate(business);
            }
        }

        [Test]
        public void DuplicateOrNonPositiveBuybackOffersAreRejected()
        {
            BusinessDefinition business = ScriptableObject.CreateInstance<BusinessDefinition>();
            ItemDefinition bread = ScriptableObject.CreateInstance<ItemDefinition>();
            try
            {
                SetPrivate(business, "businessId", "test-shop");
                SetPrivate(business, "displayName", "Test Shop");
                SetPrivate(business, "businessType", "Shop");
                SetPrivate(business, "ownerNpcId", "milo");
                SetPrivate(business, "locationId", "CornerShop");
                SetPrivate(bread, "id", Guid.NewGuid().ToString());
                SetPrivate(bread, "displayName", "Bread");

                SetPrivate(business, "buybackOffers", new[]
                {
                    new BusinessDefinition.BuybackEntry { Item = bread, UnitPrice = 4 },
                    new BusinessDefinition.BuybackEntry { Item = bread, UnitPrice = 8 }
                });
                Assert.That(business.TryValidate(out string duplicateError), Is.False);
                Assert.That(duplicateError, Does.Contain("same item"));

                SetPrivate(business, "buybackOffers", new[]
                {
                    new BusinessDefinition.BuybackEntry { Item = bread, UnitPrice = 0 }
                });
                Assert.That(business.TryValidate(out string priceError), Is.False);
                Assert.That(priceError, Does.Contain("positive fixed price"));

                SetPrivate(business, "buybackOffers", new[]
                {
                    new BusinessDefinition.BuybackEntry { Item = null, UnitPrice = 5 }
                });
                Assert.That(business.TryValidate(out string itemError), Is.False);
                Assert.That(itemError, Does.Contain("valid item"));

                SetPrivate(bread, "id", string.Empty);
                SetPrivate(business, "buybackOffers", new[]
                {
                    new BusinessDefinition.BuybackEntry { Item = bread, UnitPrice = 5 }
                });
                Assert.That(business.TryValidate(out string idError), Is.False);
                Assert.That(idError, Does.Contain("stable item ID"));
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
