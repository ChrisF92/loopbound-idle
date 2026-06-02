using System.IO;
using LoopboundIdle.Kingdom.Core;
using LoopboundIdle.Kingdom.Persistence;
using LoopboundIdle.Kingdom.Presentation;
using NUnit.Framework;

namespace LoopboundIdle.Kingdom.Tests.EditMode
{
    public sealed class KingdomPresentationTests
    {
        [Test]
        public void NumberFormatterAbbreviatesLargeValuesAndRates()
        {
            var formatter = new KingdomNumberFormatter();

            Assert.AreEqual("999", formatter.FormatNumber(999d));
            Assert.AreEqual("1.23K", formatter.FormatNumber(1234d));
            Assert.AreEqual("0.0017/s", formatter.FormatRate(1d / 600d));
            Assert.AreEqual("9.88M/s", formatter.FormatRate(9876543d));
            Assert.AreEqual("1h 1m", formatter.FormatDuration(3661d));
        }

        [Test]
        public void BalanceReportFormatterCreatesMarkdownSummary()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var report = new KingdomBalanceSimulator().Simulate(catalog, new BalanceSimulationOptions
            {
                durationSeconds = 60d,
                stepSeconds = 1d,
                snapshotSeconds = new[] { 0d, 60d },
                buyBuildings = true,
                buyUpgrades = true,
                maxPurchasesPerStep = 25
            });
            var markdown = new KingdomBalanceReportFormatter().FormatMarkdown(report, catalog);

            Assert.IsTrue(markdown.Contains("# Balance Simulation Report"));
            Assert.IsTrue(markdown.Contains("| Time | Building Buys | Upgrade Buys |"));
            Assert.IsTrue(markdown.Contains("Farms"));
            Assert.IsTrue(markdown.Contains("Food:"));
        }

        [Test]
        public void FacadeUpdatesViewModelAfterPurchase()
        {
            var game = new KingdomGame();

            Assert.IsTrue(game.BuyBuilding(BuildingId.Farm));

            Assert.AreEqual(1, game.State.GetBuildingProgress(BuildingId.Farm).level);
            Assert.AreEqual(1, game.ViewModel.buildings[0].level);
            Assert.AreEqual("Level 1", game.ViewModel.buildings[0].levelLabel);
            Assert.AreEqual(ResourceId.Food, game.ViewModel.resources[1].resourceId);
            Assert.Greater(game.ViewModel.resources[1].amountPerSecond, 0d);
        }

        [Test]
        public void FacadeExportsAndImportsSaveText()
        {
            var game = new KingdomGame();
            game.State.wallet.Set(ResourceId.Food, 500d);
            Assert.IsTrue(game.BuyBuilding(BuildingId.Farm));

            var exported = game.ExportSave(1000L);
            var importedGame = new KingdomGame();

            Assert.IsTrue(importedGame.ImportSave(exported, 1000L));
            Assert.AreEqual(1, importedGame.State.GetBuildingProgress(BuildingId.Farm).level);
            Assert.AreEqual(game.State.wallet.Get(ResourceId.Food), importedGame.State.wallet.Get(ResourceId.Food));
            Assert.AreEqual(1000L, importedGame.State.lastSavedUnixTimeSeconds);
        }

        [Test]
        public void FacadeSavesLoadsAndAppliesOfflineProgress()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var path = Path.Combine(Path.GetTempPath(), "loopbound-kingdom-facade-save.json");
            var store = new KingdomFileSaveStore(path);

            try
            {
                var game = new KingdomGame(catalog, null, store);
                Assert.IsTrue(game.BuyBuilding(BuildingId.Farm));
                var foodBeforeSave = game.State.wallet.Get(ResourceId.Food);

                Assert.IsTrue(game.Save(10L));

                var loadedGame = new KingdomGame(catalog, null, store);
                Assert.IsTrue(loadedGame.Load(20L));

                Assert.AreEqual(10d, loadedGame.LastOfflineSeconds);
                Assert.AreEqual(20L, loadedGame.State.lastSavedUnixTimeSeconds);
                Assert.Greater(loadedGame.State.wallet.Get(ResourceId.Food), foodBeforeSave);
                Assert.AreEqual("10s", loadedGame.ViewModel.lastOfflineLabel);
            }
            finally
            {
                store.Delete();
            }
        }

        [Test]
        public void FailedImportKeepsCurrentState()
        {
            var game = new KingdomGame();
            Assert.IsTrue(game.BuyBuilding(BuildingId.Farm));

            Assert.IsFalse(game.ImportSave("not a save", 100L));

            Assert.AreEqual(1, game.State.GetBuildingProgress(BuildingId.Farm).level);
            Assert.IsNotEmpty(game.LastError);
        }

        [Test]
        public void ChallengeViewModelReflectsActiveChallenge()
        {
            var game = new KingdomGame();

            Assert.IsTrue(game.StartChallenge(ChallengeId.FamineAge));

            Assert.AreEqual(ChallengeId.FamineAge, game.ViewModel.activeChallengeId);
            Assert.AreEqual("Famine Age", game.ViewModel.activeChallengeLabel);
            Assert.IsTrue(game.ViewModel.challenges[0].active);
            Assert.IsFalse(game.ViewModel.challenges[0].canComplete);
        }
    }
}
