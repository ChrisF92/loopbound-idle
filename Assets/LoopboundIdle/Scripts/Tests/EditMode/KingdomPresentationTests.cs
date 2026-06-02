using LoopboundIdle.Kingdom.Core;
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
    }
}
