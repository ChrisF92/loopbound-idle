using LoopboundIdle.Kingdom.Core;
using NUnit.Framework;

namespace LoopboundIdle.Kingdom.Tests.EditMode
{
    public sealed class KingdomSimulatorTests
    {
        [Test]
        public void FarmsGenerateFood()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var state = KingdomState.CreateNew(catalog);
            var simulator = new KingdomSimulator();

            Assert.IsTrue(simulator.TryBuyBuilding(state, catalog, BuildingId.Farm));

            var foodBefore = state.wallet.Get(ResourceId.Food);
            simulator.Advance(state, catalog, 10d);

            Assert.Greater(state.wallet.Get(ResourceId.Food), foodBefore);
        }

        [Test]
        public void CollapseAwardsLegacyAndResetsLoopProgress()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var state = KingdomState.CreateNew(catalog);
            var simulator = new KingdomSimulator();

            state.wallet.Set(ResourceId.Food, 10000d);
            state.wallet.Set(ResourceId.Wood, 10000d);
            state.wallet.Set(ResourceId.Stone, 10000d);
            state.wallet.Set(ResourceId.Knowledge, 10000d);
            state.wallet.Set(ResourceId.Authority, 10000d);
            simulator.TryBuyBuilding(state, catalog, BuildingId.Archive);

            var reward = simulator.CollapseAge(state, catalog);

            Assert.Greater(reward, 0d);
            Assert.AreEqual(reward, state.wallet.Get(ResourceId.Legacy));
            Assert.AreEqual(0, state.GetBuildingProgress(BuildingId.Archive).level);
            Assert.AreEqual(ChallengeId.None, state.activeChallengeId);
        }

        [Test]
        public void ChallengeCompletionIsRecorded()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var state = KingdomState.CreateNew(catalog);
            var simulator = new KingdomSimulator();

            Assert.IsTrue(simulator.StartChallenge(state, catalog, ChallengeId.FamineAge));
            state.wallet.Set(ResourceId.Food, 600d);
            state.wallet.Set(ResourceId.Knowledge, 45d);

            Assert.IsTrue(simulator.TryCompleteActiveChallenge(state, catalog));

            Assert.AreEqual(1, state.GetChallengeProgress(ChallengeId.FamineAge).completions);
            Assert.AreEqual(ChallengeId.None, state.activeChallengeId);
        }
    }
}
