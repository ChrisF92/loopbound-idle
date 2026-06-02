using LoopboundIdle.Kingdom.Core;
using NUnit.Framework;

namespace LoopboundIdle.Kingdom.Tests.EditMode
{
    public sealed class KingdomBalanceSimulatorTests
    {
        [Test]
        public void DefaultSimulationGeneratesEarlyGameSnapshots()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var balanceSimulator = new KingdomBalanceSimulator();

            var report = balanceSimulator.Simulate(catalog);

            Assert.AreEqual(3, report.snapshots.Length);
            Assert.AreEqual(60d, report.snapshots[0].timeSeconds);
            Assert.AreEqual(300d, report.snapshots[1].timeSeconds);
            Assert.AreEqual(900d, report.snapshots[2].timeSeconds);
            Assert.Greater(report.buildingPurchases, 0);
            Assert.GreaterOrEqual(report.snapshots[1].buildingPurchases, report.snapshots[0].buildingPurchases);
            Assert.GreaterOrEqual(report.snapshots[2].buildingPurchases, report.snapshots[1].buildingPurchases);
            Assert.Greater(report.finalState.GetBuildingProgress(BuildingId.Farm).level, 0);
            Assert.Greater(report.finalState.GetBuildingProgress(BuildingId.LumberCamp).level, 0);
            Assert.AreEqual(900d, report.finalState.elapsedSeconds);
        }

        [Test]
        public void SimulationDoesNotMutateInitialState()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var initialState = KingdomState.CreateNew(catalog);
            var balanceSimulator = new KingdomBalanceSimulator();

            balanceSimulator.Simulate(catalog, initialState, new BalanceSimulationOptions
            {
                durationSeconds = 60d,
                stepSeconds = 1d,
                snapshotSeconds = new[] { 60d },
                buyBuildings = true,
                buyUpgrades = true,
                maxPurchasesPerStep = 25
            });

            Assert.AreEqual(0d, initialState.elapsedSeconds);
            Assert.AreEqual(0, initialState.GetBuildingProgress(BuildingId.Farm).level);
            Assert.AreEqual(15d, initialState.wallet.Get(ResourceId.Wood));
        }

        [Test]
        public void SimulationCanDisableAutobuy()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var balanceSimulator = new KingdomBalanceSimulator();

            var report = balanceSimulator.Simulate(catalog, new BalanceSimulationOptions
            {
                durationSeconds = 10d,
                stepSeconds = 1d,
                snapshotSeconds = new[] { 10d },
                buyBuildings = false,
                buyUpgrades = false,
                maxPurchasesPerStep = 25
            });

            Assert.AreEqual(0, report.buildingPurchases);
            Assert.AreEqual(0, report.upgradePurchases);
            Assert.AreEqual(0, report.finalState.GetBuildingProgress(BuildingId.Farm).level);
            Assert.AreEqual(10d, report.finalState.elapsedSeconds);
        }

        [Test]
        public void ZeroSecondSnapshotCapturesInitialStateBeforeAutobuy()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var balanceSimulator = new KingdomBalanceSimulator();

            var report = balanceSimulator.Simulate(catalog, new BalanceSimulationOptions
            {
                durationSeconds = 1d,
                stepSeconds = 1d,
                snapshotSeconds = new[] { 0d, 1d },
                buyBuildings = true,
                buyUpgrades = true,
                maxPurchasesPerStep = 25
            });

            Assert.AreEqual(0d, report.snapshots[0].timeSeconds);
            Assert.AreEqual(0, report.snapshots[0].buildings[0].level);
            Assert.Greater(report.snapshots[1].buildingPurchases, 0);
        }
    }
}
