using System;
using System.Reflection;
using NUnit.Framework;

public sealed class ShopSaleRulesTests
{
    [Test]
    public void MatchingRequestedStockProducesTheQuotedRevenue()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null, "ShopSaleRules domain API is not implemented yet.");
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null, "Expected TryCalculateSale public domain operation.");

        object[] args = { "Purified Water", "Purified Water", 12, 2, 1, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.True);
        Assert.That(args[5], Is.EqualTo(12));
        Assert.That(args[6], Is.EqualTo(string.Empty));
    }

    [Test]
    public void WrongOfferedItemIsRejectedWithoutRevenue()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null);
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { "Purified Water", "Field Patch", 12, 2, 1, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.False);
        Assert.That(args[5], Is.EqualTo(0));
        Assert.That(args[6], Is.Not.EqualTo(string.Empty));
    }

    [Test]
    public void MultiUnitOrderMultipliesTheQuotedUnitPrice()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null);
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { "Purified Water", "Purified Water", 12, 3, 2, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.True);
        Assert.That(args[5], Is.EqualTo(24));
    }

    [Test]
    public void OrderWithoutEnoughStockIsRejectedWithoutRevenue()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null);
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { "Purified Water", "Purified Water", 12, 0, 1, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.False);
        Assert.That(args[5], Is.EqualTo(0));
        Assert.That(args[6], Is.Not.EqualTo(string.Empty));
    }

    [Test]
    public void NonPositiveOrderQuantityIsRejectedWithoutRevenue()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null);
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { "Purified Water", "Purified Water", 12, 2, 0, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.False);
        Assert.That(args[5], Is.EqualTo(0));
        Assert.That(args[6], Is.Not.EqualTo(string.Empty));
    }

    [Test]
    public void NonPositiveUnitPriceIsRejectedWithoutRevenue()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null);
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { "Purified Water", "Purified Water", 0, 2, 1, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.False);
        Assert.That(args[5], Is.EqualTo(0));
        Assert.That(args[6], Is.Not.EqualTo(string.Empty));
    }

    [Test]
    public void ReceiptOverflowIsRejectedWithoutRevenue()
    {
        var type = Type.GetType("ShopSaleRules, Signal.Shop");
        Assert.That(type, Is.Not.Null);
        var method = type.GetMethod("TryCalculateSale", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        object[] args = { "Purified Water", "Purified Water", int.MaxValue, 2, 2, 0, string.Empty };
        bool accepted = (bool)method.Invoke(null, args);

        Assert.That(accepted, Is.False);
        Assert.That(args[5], Is.EqualTo(0));
        Assert.That(args[6], Is.Not.EqualTo(string.Empty));
    }

    [Test]
    public void ShopCounterExposesTheSharedInteractionContract()
    {
        var counterType = Type.GetType("ShopCounterInteractable, Assembly-CSharp");
        var interactableType = Type.GetType("InteractableBase, Assembly-CSharp");

        Assert.That(counterType, Is.Not.Null, "Shop counter component is not implemented yet.");
        Assert.That(interactableType, Is.Not.Null);
        Assert.That(counterType.IsSubclassOf(interactableType), Is.True);
        Assert.That(counterType.GetMethod("Interact", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
    }
}
