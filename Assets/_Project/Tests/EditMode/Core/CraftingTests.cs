using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CraftingTests
    {
        private static ItemType Item(int width = 1, int height = 1, int maxStack = 5)
        {
            return new ItemType(Guid.NewGuid(), width, height, maxStack);
        }

        [Test]
        public void CraftConsumesInputsAndAddsTheOutput()
        {
            var a = Item();
            var b = Item();
            var grid = new GridInventory();
            grid.Place(a, 0, 0, count: 3);
            var recipe = new Recipe(new[] { new Ingredient(a, 2) }, new Ingredient(b, 1));

            CraftResult result = Crafting.TryCraft(recipe, grid);

            Assert.That(result, Is.EqualTo(CraftResult.Done));
            Assert.That(grid.CountOf(a), Is.EqualTo(1));
            Assert.That(grid.CountOf(b), Is.EqualTo(1));
        }

        [Test]
        public void MissingInputsChangeNothing()
        {
            var a = Item();
            var b = Item();
            var grid = new GridInventory();
            grid.Place(a, 0, 0, count: 1);
            var recipe = new Recipe(new[] { new Ingredient(a, 2) }, new Ingredient(b, 1));
            int fired = 0;
            grid.Changed += _ => fired++;

            CraftResult result = Crafting.TryCraft(recipe, grid);

            Assert.That(result, Is.EqualTo(CraftResult.MissingInputs));
            Assert.That(grid.CountOf(a), Is.EqualTo(1));
            Assert.That(grid.CountOf(b), Is.EqualTo(0));
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void AnOutputThatCannotFitRollsEverythingBack()
        {
            var a = Item();
            var big = Item(2, 2, 1);
            var grid = new GridInventory(1, 1);
            grid.Place(a, 0, 0, count: 2);
            var recipe = new Recipe(new[] { new Ingredient(a, 2) }, new Ingredient(big, 1));

            CraftResult result = Crafting.TryCraft(recipe, grid);

            Assert.That(result, Is.EqualTo(CraftResult.NoSpaceForOutput));
            Assert.That(grid.CountOf(a), Is.EqualTo(2));
            Assert.That(grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void TheOutputCanUseSpaceFreedByTheInputs()
        {
            var a = Item(1, 1, 1);
            var b = Item(1, 1, 1);
            var grid = new GridInventory(1, 1);
            grid.Place(a, 0, 0);
            var recipe = new Recipe(new[] { new Ingredient(a, 1) }, new Ingredient(b, 1));

            CraftResult result = Crafting.TryCraft(recipe, grid);

            Assert.That(result, Is.EqualTo(CraftResult.Done));
            Assert.That(grid.CountOf(a), Is.EqualTo(0));
            Assert.That(grid.CountOf(b), Is.EqualTo(1));
        }

        [Test]
        public void ListingTheSameInputTwiceNeedsBothAmounts()
        {
            var a = Item();
            var b = Item();
            var recipe = new Recipe(new[] { new Ingredient(a, 1), new Ingredient(a, 1) }, new Ingredient(b, 1));

            var short1 = new GridInventory();
            short1.Place(a, 0, 0, count: 1);
            var enough = new GridInventory();
            enough.Place(a, 0, 0, count: 2);

            Assert.That(Crafting.TryCraft(recipe, short1), Is.EqualTo(CraftResult.MissingInputs));
            Assert.That(Crafting.TryCraft(recipe, enough), Is.EqualTo(CraftResult.Done));
        }

        [Test]
        public void ARecipeNeedsAtLeastOneInput()
        {
            Assert.Throws<ArgumentException>(() => new Recipe(new Ingredient[0], new Ingredient(Item(), 1)));
        }

        [Test]
        public void AnIngredientNeedsACountOfAtLeastOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Ingredient(Item(), 0));
        }

        [Test]
        public void AnUninitialisedItemTypeIsNotAValidIngredient()
        {
            Assert.Throws<ArgumentException>(() => new Ingredient(default(ItemType), 1));
        }
    }
}
