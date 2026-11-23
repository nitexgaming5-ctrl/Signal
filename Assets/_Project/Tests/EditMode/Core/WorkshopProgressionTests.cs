using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class WorkshopProgressionTests
    {
        [Test]
        public void UpgradeRequiresCompletedContractAndPaysItsAuthoredRecipient()
        {
            Guid contractId = Guid.NewGuid();
            Guid upgradeId = Guid.NewGuid();
            var contracts = new CustomerContractBook();
            contracts.TryRegisterOffer(contractId);
            contracts.TryAccept(contractId, 1, 100);
            var progression = new WorkshopProgressionBook();
            var player = new CleanMoneyAccount(100);
            var recipient = new CleanMoneyAccount(5);
            var recipientActor = new EconomicActorId("resident:workshop-supplier");

            WorkshopUpgradeResult blocked = WorkshopUpgradeService.TryAcquire(upgradeId, contractId, 25,
                recipientActor, player, recipient, contracts, progression);
            Assert.That(blocked.Status, Is.EqualTo(WorkshopUpgradeStatus.ContractPrerequisiteNotMet));
            Assert.That(player.Balance, Is.EqualTo(100));

            contracts.TryComplete(contractId, Guid.NewGuid(), 10, new EconomicActorId("resident:customer"), 1, 200);
            WorkshopUpgradeResult acquired = WorkshopUpgradeService.TryAcquire(upgradeId, contractId, 25,
                recipientActor, player, recipient, contracts, progression);

            Assert.That(acquired.Success, Is.True);
            Assert.That(player.Balance, Is.EqualTo(75));
            Assert.That(recipient.Balance, Is.EqualTo(30));
            Assert.That(progression.HasAcquired(upgradeId), Is.True);
        }

        [Test]
        public void UpgradeCannotBePurchasedTwice()
        {
            Guid contractId = Guid.NewGuid();
            Guid upgradeId = Guid.NewGuid();
            var contracts = new CustomerContractBook();
            contracts.TryRegisterOffer(contractId);
            contracts.TryAccept(contractId, 1, 100);
            contracts.TryComplete(contractId, Guid.NewGuid(), 10, new EconomicActorId("resident:customer"), 1, 200);
            var progression = new WorkshopProgressionBook();
            var player = new CleanMoneyAccount(100);
            var recipient = new CleanMoneyAccount(0);
            var actor = new EconomicActorId("resident:supplier");

            Assert.That(WorkshopUpgradeService.TryAcquire(upgradeId, contractId, 25,
                actor, player, recipient, contracts, progression).Success, Is.True);
            WorkshopUpgradeResult replay = WorkshopUpgradeService.TryAcquire(upgradeId, contractId, 25,
                actor, player, recipient, contracts, progression);

            Assert.That(replay.Status, Is.EqualTo(WorkshopUpgradeStatus.AlreadyAcquired));
            Assert.That(player.Balance, Is.EqualTo(75));
            Assert.That(recipient.Balance, Is.EqualTo(25));
        }

        [Test]
        public void UpgradeCanUseAnExplicitNoContractPrerequisite()
        {
            Guid upgradeId = Guid.NewGuid();
            var progression = new WorkshopProgressionBook();
            var player = new CleanMoneyAccount(50);
            var recipient = new CleanMoneyAccount(0);

            WorkshopUpgradeResult result = WorkshopUpgradeService.TryAcquire(upgradeId, Guid.Empty, 20,
                new EconomicActorId("resident:supplier"), player, recipient, null, progression);

            Assert.That(result.Success, Is.True);
            Assert.That(player.Balance, Is.EqualTo(30));
            Assert.That(recipient.Balance, Is.EqualTo(20));
            Assert.That(progression.HasAcquired(upgradeId), Is.True);
        }

        [Test]
        public void InsufficientFundsDoNotAcquireOrCreditRecipient()
        {
            Guid contractId = Guid.NewGuid();
            Guid upgradeId = Guid.NewGuid();
            var contracts = new CustomerContractBook();
            contracts.TryRegisterOffer(contractId);
            contracts.TryAccept(contractId, 1, 100);
            contracts.TryComplete(contractId, Guid.NewGuid(), 10, new EconomicActorId("resident:customer"), 1, 200);
            var progression = new WorkshopProgressionBook();
            var player = new CleanMoneyAccount(5);
            var recipient = new CleanMoneyAccount(0);

            WorkshopUpgradeResult result = WorkshopUpgradeService.TryAcquire(upgradeId, contractId, 25,
                new EconomicActorId("resident:supplier"), player, recipient, contracts, progression);

            Assert.That(result.Status, Is.EqualTo(WorkshopUpgradeStatus.InsufficientFunds));
            Assert.That(player.Balance, Is.EqualTo(5));
            Assert.That(recipient.Balance, Is.Zero);
            Assert.That(progression.HasAcquired(upgradeId), Is.False);
        }
    }
}
