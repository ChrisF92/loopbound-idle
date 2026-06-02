using System.IO;
using LoopboundIdle.Kingdom.Core;
using LoopboundIdle.Kingdom.Persistence;
using NUnit.Framework;

namespace LoopboundIdle.Kingdom.Tests.EditMode
{
    public sealed class KingdomPersistenceTests
    {
        [Test]
        public void ExportImportRoundTripsProgress()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var state = KingdomState.CreateNew(catalog);
            var codec = new KingdomSaveCodec();

            state.wallet.Set(ResourceId.Food, 1234d);
            state.GetBuildingProgress(BuildingId.Farm).level = 7;
            state.GetChallengeProgress(ChallengeId.FamineAge).completions = 2;

            var exported = codec.Export(state, catalog, 100L);
            var imported = codec.Import(exported, catalog);

            Assert.IsTrue(exported.StartsWith(KingdomSaveCodec.ExportPrefix));
            Assert.AreEqual(100L, imported.lastSavedUnixTimeSeconds);
            Assert.AreEqual(1234d, imported.wallet.Get(ResourceId.Food));
            Assert.AreEqual(7, imported.GetBuildingProgress(BuildingId.Farm).level);
            Assert.AreEqual(2, imported.GetChallengeProgress(ChallengeId.FamineAge).completions);
        }

        [Test]
        public void MigratorFillsMissingCatalogProgress()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var partialState = new KingdomState
            {
                loopIndex = 0,
                wallet = new ResourceWallet
                {
                    resources = new[] { new ResourceAmount(ResourceId.Legacy, 5d) }
                },
                buildings = new[] { new BuildingProgress(BuildingId.Farm, 3) },
                upgrades = null,
                challenges = null,
                activeChallengeId = (ChallengeId)999
            };

            var migrated = KingdomSaveMigrator.Migrate(partialState, catalog);

            Assert.AreEqual(KingdomSaveMigrator.CurrentSaveVersion, migrated.saveVersion);
            Assert.AreEqual(1, migrated.loopIndex);
            Assert.AreEqual(catalog.buildings.Length, migrated.buildings.Length);
            Assert.AreEqual(3, migrated.GetBuildingProgress(BuildingId.Farm).level);
            Assert.AreEqual(5d, migrated.wallet.Get(ResourceId.Legacy));
            Assert.AreEqual(ChallengeId.None, migrated.activeChallengeId);
        }

        [Test]
        public void FileSaveStoreSavesAndLoadsLocalJson()
        {
            var catalog = KingdomCatalog.CreateDefault();
            var state = KingdomState.CreateNew(catalog);
            var path = Path.Combine(Path.GetTempPath(), "loopbound-kingdom-test-save.json");
            var store = new KingdomFileSaveStore(path);

            try
            {
                state.wallet.Set(ResourceId.Wood, 321d);
                store.Save(state, catalog, 200L);

                var loaded = store.Load(catalog);

                Assert.AreEqual(321d, loaded.wallet.Get(ResourceId.Wood));
                Assert.AreEqual(200L, loaded.lastSavedUnixTimeSeconds);
            }
            finally
            {
                store.Delete();
            }
        }
    }
}
