using System;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Persistence
{
    public static class KingdomSaveMigrator
    {
        public const int CurrentSaveVersion = 1;

        public static KingdomState Migrate(KingdomState state, KingdomCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            if (state == null)
            {
                state = KingdomState.CreateNew(catalog);
            }

            state.saveVersion = CurrentSaveVersion;
            state.loopIndex = Math.Max(1, state.loopIndex);
            state.elapsedSeconds = SanitizeNonNegative(state.elapsedSeconds);
            state.collapsePressure = SanitizeNonNegative(state.collapsePressure);
            state.lastSavedUnixTimeSeconds = SanitizeNonNegative(state.lastSavedUnixTimeSeconds);
            state.wallet = MigrateWallet(state.wallet);
            state.buildings = MigrateBuildings(state.buildings, catalog);
            state.upgrades = MigrateUpgrades(state.upgrades, catalog);
            state.challenges = MigrateChallenges(state.challenges, catalog);

            if (!HasChallenge(catalog, state.activeChallengeId))
            {
                state.activeChallengeId = ChallengeId.None;
            }

            return state;
        }

        private static ResourceWallet MigrateWallet(ResourceWallet wallet)
        {
            var migrated = new ResourceWallet();
            if (wallet == null || wallet.resources == null)
            {
                return migrated;
            }

            for (var i = 0; i < migrated.resources.Length; i++)
            {
                var resourceId = migrated.resources[i].resourceId;
                migrated.resources[i].amount = SanitizeNonNegative(GetResourceAmount(wallet.resources, resourceId, migrated.resources[i].amount));
            }

            return migrated;
        }

        private static double GetResourceAmount(ResourceAmount[] resources, ResourceId resourceId, double fallback)
        {
            if (resources == null)
            {
                return fallback;
            }

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].resourceId == resourceId)
                {
                    return resources[i].amount;
                }
            }

            return fallback;
        }

        private static BuildingProgress[] MigrateBuildings(BuildingProgress[] existing, KingdomCatalog catalog)
        {
            var migrated = new BuildingProgress[catalog.buildings.Length];
            for (var i = 0; i < catalog.buildings.Length; i++)
            {
                var buildingId = catalog.buildings[i].buildingId;
                migrated[i] = new BuildingProgress(buildingId, SanitizeNonNegative(GetBuildingLevel(existing, buildingId)));
            }

            return migrated;
        }

        private static int GetBuildingLevel(BuildingProgress[] buildings, BuildingId buildingId)
        {
            if (buildings == null)
            {
                return 0;
            }

            for (var i = 0; i < buildings.Length; i++)
            {
                if (buildings[i] != null && buildings[i].buildingId == buildingId)
                {
                    return buildings[i].level;
                }
            }

            return 0;
        }

        private static UpgradeProgress[] MigrateUpgrades(UpgradeProgress[] existing, KingdomCatalog catalog)
        {
            var migrated = new UpgradeProgress[catalog.upgrades.Length];
            for (var i = 0; i < catalog.upgrades.Length; i++)
            {
                var upgradeId = catalog.upgrades[i].upgradeId;
                migrated[i] = new UpgradeProgress(upgradeId, IsUpgradePurchased(existing, upgradeId));
            }

            return migrated;
        }

        private static bool IsUpgradePurchased(UpgradeProgress[] upgrades, UpgradeId upgradeId)
        {
            if (upgrades == null)
            {
                return false;
            }

            for (var i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] != null && upgrades[i].upgradeId == upgradeId)
                {
                    return upgrades[i].purchased;
                }
            }

            return false;
        }

        private static ChallengeProgress[] MigrateChallenges(ChallengeProgress[] existing, KingdomCatalog catalog)
        {
            var migrated = new ChallengeProgress[catalog.challenges.Length];
            for (var i = 0; i < catalog.challenges.Length; i++)
            {
                var challengeId = catalog.challenges[i].challengeId;
                migrated[i] = new ChallengeProgress(challengeId, SanitizeNonNegative(GetChallengeCompletions(existing, challengeId)));
            }

            return migrated;
        }

        private static int GetChallengeCompletions(ChallengeProgress[] challenges, ChallengeId challengeId)
        {
            if (challenges == null)
            {
                return 0;
            }

            for (var i = 0; i < challenges.Length; i++)
            {
                if (challenges[i] != null && challenges[i].challengeId == challengeId)
                {
                    return challenges[i].completions;
                }
            }

            return 0;
        }

        private static bool HasChallenge(KingdomCatalog catalog, ChallengeId challengeId)
        {
            if (challengeId == ChallengeId.None)
            {
                return true;
            }

            for (var i = 0; i < catalog.challenges.Length; i++)
            {
                if (catalog.challenges[i].challengeId == challengeId)
                {
                    return true;
                }
            }

            return false;
        }

        private static double SanitizeNonNegative(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
            {
                return 0d;
            }

            return value;
        }

        private static int SanitizeNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }

        private static long SanitizeNonNegative(long value)
        {
            return value < 0L ? 0L : value;
        }
    }
}
