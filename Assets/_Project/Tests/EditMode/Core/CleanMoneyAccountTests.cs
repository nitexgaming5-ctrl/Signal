using NUnit.Framework;
namespace Signal.Core.Tests
{
    public sealed class CleanMoneyAccountTests
    {
        [Test] public void StartsAtConfiguredNonNegativeBalance()
        {
            Assert.That(new CleanMoneyAccount(125).Balance, Is.EqualTo(125));
            Assert.That(new CleanMoneyAccount(-10).Balance, Is.Zero);
        }
        [Test] public void CreditIncreasesBalanceAndNotifiesOnce()
        {
            var account = new CleanMoneyAccount(20); int count = 0; account.Changed += _ => count++;
            Assert.That(account.TryCredit(15), Is.True);
            Assert.That(account.Balance, Is.EqualTo(35)); Assert.That(count, Is.EqualTo(1));
        }
        [Test] public void DebitRequiresSufficientFunds()
        {
            var account = new CleanMoneyAccount(30);
            Assert.That(account.TryDebit(12), Is.True); Assert.That(account.Balance, Is.EqualTo(18));
            Assert.That(account.TryDebit(19), Is.False); Assert.That(account.Balance, Is.EqualTo(18));
        }
        [TestCase(0)] [TestCase(-1)] public void NonPositiveCreditsAndDebitsAreRejected(int amount)
        {
            var account = new CleanMoneyAccount(50);
            Assert.That(account.TryCredit(amount), Is.False); Assert.That(account.TryDebit(amount), Is.False);
            Assert.That(account.Balance, Is.EqualTo(50));
        }
        [Test] public void CreditCannotOverflowBalance()
        {
            var account = new CleanMoneyAccount(int.MaxValue - 2);
            Assert.That(account.TryCredit(3), Is.False); Assert.That(account.Balance, Is.EqualTo(int.MaxValue - 2));
        }
        [Test] public void RestoreClampsNegativeValuesAndNotifiesOnlyOnChange()
        {
            var account = new CleanMoneyAccount(10); int count = 0; account.Changed += _ => count++;
            account.Restore(10); account.Restore(-5);
            Assert.That(account.Balance, Is.Zero); Assert.That(count, Is.EqualTo(1));
        }
    }
}
