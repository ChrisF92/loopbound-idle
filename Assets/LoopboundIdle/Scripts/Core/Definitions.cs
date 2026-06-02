using System;

namespace LoopboundIdle.Kingdom.Core
{
    [Serializable]
    public struct ProductionRate
    {
        public ResourceId resourceId;
        public double amountPerSecond;

        public ProductionRate(ResourceId resourceId, double amountPerSecond)
        {
            this.resourceId = resourceId;
            this.amountPerSecond = amountPerSecond;
        }
    }

    [Serializable]
    public sealed class BuildingDefinition
    {
        public BuildingId buildingId;
        public string displayName;
        public string description;
        public ResourceCost[] baseCosts;
        public double costGrowth;
        public ProductionRate[] production;

        public BuildingDefinition(
            BuildingId buildingId,
            string displayName,
            string description,
            ResourceCost[] baseCosts,
            double costGrowth,
            ProductionRate[] production)
        {
            this.buildingId = buildingId;
            this.displayName = displayName;
            this.description = description;
            this.baseCosts = baseCosts;
            this.costGrowth = costGrowth;
            this.production = production;
        }

        public ResourceCost[] CostForNextLevel(int currentLevel)
        {
            var multiplier = Math.Pow(costGrowth, Math.Max(0, currentLevel));
            var costs = new ResourceCost[baseCosts.Length];

            for (var i = 0; i < baseCosts.Length; i++)
            {
                costs[i] = new ResourceCost(baseCosts[i].resourceId, baseCosts[i].amount * multiplier);
            }

            return costs;
        }
    }

    [Serializable]
    public sealed class UpgradeDefinition
    {
        public UpgradeId upgradeId;
        public string displayName;
        public string description;
        public ResourceCost[] costs;
        public ResourceId affectedResource;
        public double productionMultiplier;

        public UpgradeDefinition(
            UpgradeId upgradeId,
            string displayName,
            string description,
            ResourceCost[] costs,
            ResourceId affectedResource,
            double productionMultiplier)
        {
            this.upgradeId = upgradeId;
            this.displayName = displayName;
            this.description = description;
            this.costs = costs;
            this.affectedResource = affectedResource;
            this.productionMultiplier = productionMultiplier;
        }
    }

    [Serializable]
    public struct ChallengeModifier
    {
        public ResourceId affectedResource;
        public double productionMultiplier;

        public ChallengeModifier(ResourceId affectedResource, double productionMultiplier)
        {
            this.affectedResource = affectedResource;
            this.productionMultiplier = productionMultiplier;
        }
    }

    [Serializable]
    public sealed class ChallengeDefinition
    {
        public ChallengeId challengeId;
        public string displayName;
        public string description;
        public ResourceCost[] completionGoals;
        public ChallengeModifier[] modifiers;
        public string rewardDescription;

        public ChallengeDefinition(
            ChallengeId challengeId,
            string displayName,
            string description,
            ResourceCost[] completionGoals,
            ChallengeModifier[] modifiers,
            string rewardDescription)
        {
            this.challengeId = challengeId;
            this.displayName = displayName;
            this.description = description;
            this.completionGoals = completionGoals;
            this.modifiers = modifiers;
            this.rewardDescription = rewardDescription;
        }
    }
}
