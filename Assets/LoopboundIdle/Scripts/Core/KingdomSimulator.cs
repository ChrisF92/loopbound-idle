using System;

namespace LoopboundIdle.Kingdom.Core
{
    public sealed class KingdomSimulator
    {
        public const double MaxOfflineSeconds = 8d * 60d * 60d;

        public void Advance(KingdomState state, KingdomCatalog catalog, double seconds)
        {
            if (state == null || catalog == null || seconds <= 0d)
            {
                return;
            }

            ApplyProduction(state, catalog, seconds);

            state.elapsedSeconds += seconds;
            state.collapsePressure += CalculateCollapsePressurePerSecond(state) * seconds;
        }

        public double AdvanceOffline(KingdomState state, KingdomCatalog catalog, long currentUnixTimeSeconds)
        {
            if (state == null || state.lastSavedUnixTimeSeconds <= 0L)
            {
                if (state != null)
                {
                    state.lastSavedUnixTimeSeconds = currentUnixTimeSeconds;
                }

                return 0d;
            }

            var elapsed = currentUnixTimeSeconds - state.lastSavedUnixTimeSeconds;
            if (elapsed <= 0L)
            {
                state.lastSavedUnixTimeSeconds = currentUnixTimeSeconds;
                return 0d;
            }

            var simulatedSeconds = Math.Min(MaxOfflineSeconds, elapsed);
            Advance(state, catalog, simulatedSeconds);
            state.lastSavedUnixTimeSeconds = currentUnixTimeSeconds;
            return simulatedSeconds;
        }

        public bool TryBuyBuilding(KingdomState state, KingdomCatalog catalog, BuildingId buildingId)
        {
            var definition = catalog.GetBuilding(buildingId);
            var progress = state.GetBuildingProgress(buildingId);
            var costs = definition.CostForNextLevel(progress.level);

            if (!state.wallet.Spend(costs))
            {
                return false;
            }

            progress.level++;
            return true;
        }

        public bool TryBuyUpgrade(KingdomState state, KingdomCatalog catalog, UpgradeId upgradeId)
        {
            var progress = state.GetUpgradeProgress(upgradeId);
            if (progress.purchased)
            {
                return false;
            }

            var definition = catalog.GetUpgrade(upgradeId);
            if (!state.wallet.Spend(definition.costs))
            {
                return false;
            }

            progress.purchased = true;
            return true;
        }

        public bool StartChallenge(KingdomState state, KingdomCatalog catalog, ChallengeId challengeId)
        {
            if (challengeId == ChallengeId.None)
            {
                return false;
            }

            catalog.GetChallenge(challengeId);
            ResetCurrentLoop(state, catalog, challengeId);
            return true;
        }

        public bool CanCompleteActiveChallenge(KingdomState state, KingdomCatalog catalog)
        {
            if (state.activeChallengeId == ChallengeId.None)
            {
                return false;
            }

            var challenge = catalog.GetChallenge(state.activeChallengeId);
            return HasResources(state, challenge.completionGoals);
        }

        public bool TryCompleteActiveChallenge(KingdomState state, KingdomCatalog catalog)
        {
            if (!CanCompleteActiveChallenge(state, catalog))
            {
                return false;
            }

            var progress = state.GetChallengeProgress(state.activeChallengeId);
            progress.completions++;
            CollapseAge(state, catalog);
            return true;
        }

        public double CollapseAge(KingdomState state, KingdomCatalog catalog)
        {
            var legacyReward = CalculateLegacyReward(state, catalog);
            var currentLegacy = state.wallet.Get(ResourceId.Legacy);

            ResetCurrentLoop(state, catalog, ChallengeId.None);
            state.wallet.Set(ResourceId.Legacy, currentLegacy + legacyReward);

            return legacyReward;
        }

        public double CalculateLegacyReward(KingdomState state, KingdomCatalog catalog)
        {
            var kingdomValue = CalculateKingdomValue(state, catalog);
            var archiveLevel = state.GetBuildingProgress(BuildingId.Archive).level;
            var archiveMultiplier = 1d + archiveLevel * 0.03d;
            var pressureMultiplier = 1d + Math.Min(1.5d, state.collapsePressure / 100d);
            var chronicleMultiplier = IsUpgradePurchased(state, UpgradeId.ChronicleVaults) ? 1.25d : 1d;

            return Math.Floor(Math.Sqrt(Math.Max(0d, kingdomValue) / 100d) * archiveMultiplier * pressureMultiplier * chronicleMultiplier);
        }

        private static void ApplyProduction(KingdomState state, KingdomCatalog catalog, double seconds)
        {
            for (var i = 0; i < state.buildings.Length; i++)
            {
                var progress = state.buildings[i];
                if (progress.level <= 0)
                {
                    continue;
                }

                var definition = catalog.GetBuilding(progress.buildingId);
                for (var j = 0; j < definition.production.Length; j++)
                {
                    var rate = definition.production[j];
                    var multiplier = CalculateProductionMultiplier(state, catalog, rate.resourceId);
                    state.wallet.Add(rate.resourceId, rate.amountPerSecond * progress.level * multiplier * seconds);
                }
            }
        }

        private static double CalculateProductionMultiplier(KingdomState state, KingdomCatalog catalog, ResourceId resourceId)
        {
            var multiplier = 1d;

            for (var i = 0; i < catalog.upgrades.Length; i++)
            {
                var upgrade = catalog.upgrades[i];
                if (upgrade.affectedResource == resourceId && IsUpgradePurchased(state, upgrade.upgradeId))
                {
                    multiplier *= upgrade.productionMultiplier;
                }
            }

            multiplier *= CalculateLegacyProductionMultiplier(state);
            multiplier *= CalculateChallengeCompletionMultiplier(state, resourceId);
            multiplier *= CalculateActiveChallengeMultiplier(state, catalog, resourceId);

            return multiplier;
        }

        private static double CalculateLegacyProductionMultiplier(KingdomState state)
        {
            var legacy = state.wallet.Get(ResourceId.Legacy);
            return 1d + Math.Sqrt(Math.Max(0d, legacy)) * 0.02d;
        }

        private static double CalculateChallengeCompletionMultiplier(KingdomState state, ResourceId resourceId)
        {
            var bonus = 1d;

            for (var i = 0; i < state.challenges.Length; i++)
            {
                var progress = state.challenges[i];
                if (progress.completions <= 0)
                {
                    continue;
                }

                if (ChallengeRewardsResource(progress.challengeId, resourceId))
                {
                    bonus += progress.completions * 0.05d;
                }
            }

            return bonus;
        }

        private static bool ChallengeRewardsResource(ChallengeId challengeId, ResourceId resourceId)
        {
            switch (challengeId)
            {
                case ChallengeId.FamineAge:
                    return resourceId == ResourceId.Food;
                case ChallengeId.DarkAge:
                    return resourceId == ResourceId.Knowledge;
                case ChallengeId.Stonebound:
                    return resourceId == ResourceId.Stone;
                case ChallengeId.ShortReign:
                    return resourceId == ResourceId.Food || resourceId == ResourceId.Wood;
                case ChallengeId.SilentCouncil:
                    return resourceId == ResourceId.Authority;
                default:
                    return false;
            }
        }

        private static double CalculateActiveChallengeMultiplier(KingdomState state, KingdomCatalog catalog, ResourceId resourceId)
        {
            if (state.activeChallengeId == ChallengeId.None)
            {
                return 1d;
            }

            var challenge = catalog.GetChallenge(state.activeChallengeId);
            var multiplier = 1d;

            for (var i = 0; i < challenge.modifiers.Length; i++)
            {
                var modifier = challenge.modifiers[i];
                if (modifier.affectedResource == resourceId)
                {
                    multiplier *= modifier.productionMultiplier;
                }
            }

            return multiplier;
        }

        private static double CalculateCollapsePressurePerSecond(KingdomState state)
        {
            var pressure = 1d / 600d;
            if (state.activeChallengeId == ChallengeId.ShortReign)
            {
                pressure *= 3d;
            }

            return pressure;
        }

        private static double CalculateKingdomValue(KingdomState state, KingdomCatalog catalog)
        {
            var value = 0d;

            value += state.wallet.Get(ResourceId.Food) * 0.15d;
            value += state.wallet.Get(ResourceId.Wood) * 0.25d;
            value += state.wallet.Get(ResourceId.Stone) * 0.75d;
            value += state.wallet.Get(ResourceId.Knowledge) * 3d;
            value += state.wallet.Get(ResourceId.Authority) * 8d;

            for (var i = 0; i < state.buildings.Length; i++)
            {
                var progress = state.buildings[i];
                var definition = catalog.GetBuilding(progress.buildingId);
                value += progress.level * AverageBaseCost(definition) * 0.4d;
            }

            return value;
        }

        private static double AverageBaseCost(BuildingDefinition definition)
        {
            if (definition.baseCosts.Length == 0)
            {
                return 1d;
            }

            var total = 0d;
            for (var i = 0; i < definition.baseCosts.Length; i++)
            {
                total += definition.baseCosts[i].amount;
            }

            return total / definition.baseCosts.Length;
        }

        private static bool HasResources(KingdomState state, ResourceCost[] goals)
        {
            if (goals == null)
            {
                return true;
            }

            for (var i = 0; i < goals.Length; i++)
            {
                if (state.wallet.Get(goals[i].resourceId) < goals[i].amount)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsUpgradePurchased(KingdomState state, UpgradeId upgradeId)
        {
            return state.GetUpgradeProgress(upgradeId).purchased;
        }

        private static void ResetCurrentLoop(KingdomState state, KingdomCatalog catalog, ChallengeId nextChallengeId)
        {
            var legacy = state.wallet.Get(ResourceId.Legacy);

            state.loopIndex++;
            state.elapsedSeconds = 0d;
            state.collapsePressure = 0d;
            state.activeChallengeId = nextChallengeId;

            state.wallet.ResetLoopResources();
            state.wallet.Set(ResourceId.Legacy, legacy);
            state.wallet.Set(ResourceId.Population, 12d + Math.Floor(Math.Sqrt(legacy)));
            state.wallet.Set(ResourceId.Food, 25d);
            state.wallet.Set(ResourceId.Wood, 15d);

            for (var i = 0; i < state.buildings.Length; i++)
            {
                state.buildings[i].level = 0;
            }

            for (var i = 0; i < state.upgrades.Length; i++)
            {
                state.upgrades[i].purchased = false;
            }

            // Keep catalog referenced here so future reset migrations can compare state shape.
            if (catalog.buildings.Length != state.buildings.Length)
            {
                throw new InvalidOperationException("State building progress does not match the active catalog.");
            }
        }
    }
}
