using System;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Presentation
{
    [Serializable]
    public sealed class KingdomGameViewModel
    {
        public int loopIndex;
        public string loopLabel;
        public string elapsedLabel;
        public string collapsePressureLabel;
        public string collapsePressureRateLabel;
        public string projectedLegacyRewardLabel;
        public ChallengeId activeChallengeId;
        public string activeChallengeLabel;
        public bool canCompleteActiveChallenge;
        public double lastOfflineSeconds;
        public string lastOfflineLabel;
        public ResourceViewModel[] resources;
        public BuildingViewModel[] buildings;
        public UpgradeViewModel[] upgrades;
        public ChallengeViewModel[] challenges;

        public KingdomGameViewModel()
        {
            loopLabel = "";
            elapsedLabel = "";
            collapsePressureLabel = "";
            collapsePressureRateLabel = "";
            projectedLegacyRewardLabel = "";
            activeChallengeLabel = "";
            lastOfflineLabel = "";
            resources = new ResourceViewModel[0];
            buildings = new BuildingViewModel[0];
            upgrades = new UpgradeViewModel[0];
            challenges = new ChallengeViewModel[0];
        }

        public ResourceViewModel FindResource(ResourceId resourceId)
        {
            ResourceViewModel viewModel;
            return TryFindResource(resourceId, out viewModel) ? viewModel : null;
        }

        public bool TryFindResource(ResourceId resourceId, out ResourceViewModel viewModel)
        {
            viewModel = null;
            if (resources == null)
            {
                return false;
            }

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i] != null && resources[i].resourceId == resourceId)
                {
                    viewModel = resources[i];
                    return true;
                }
            }

            return false;
        }

        public BuildingViewModel FindBuilding(BuildingId buildingId)
        {
            BuildingViewModel viewModel;
            return TryFindBuilding(buildingId, out viewModel) ? viewModel : null;
        }

        public bool TryFindBuilding(BuildingId buildingId, out BuildingViewModel viewModel)
        {
            viewModel = null;
            if (buildings == null)
            {
                return false;
            }

            for (var i = 0; i < buildings.Length; i++)
            {
                if (buildings[i] != null && buildings[i].buildingId == buildingId)
                {
                    viewModel = buildings[i];
                    return true;
                }
            }

            return false;
        }

        public UpgradeViewModel FindUpgrade(UpgradeId upgradeId)
        {
            UpgradeViewModel viewModel;
            return TryFindUpgrade(upgradeId, out viewModel) ? viewModel : null;
        }

        public bool TryFindUpgrade(UpgradeId upgradeId, out UpgradeViewModel viewModel)
        {
            viewModel = null;
            if (upgrades == null)
            {
                return false;
            }

            for (var i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] != null && upgrades[i].upgradeId == upgradeId)
                {
                    viewModel = upgrades[i];
                    return true;
                }
            }

            return false;
        }

        public ChallengeViewModel FindChallenge(ChallengeId challengeId)
        {
            ChallengeViewModel viewModel;
            return TryFindChallenge(challengeId, out viewModel) ? viewModel : null;
        }

        public bool TryFindChallenge(ChallengeId challengeId, out ChallengeViewModel viewModel)
        {
            viewModel = null;
            if (challenges == null)
            {
                return false;
            }

            for (var i = 0; i < challenges.Length; i++)
            {
                if (challenges[i] != null && challenges[i].challengeId == challengeId)
                {
                    viewModel = challenges[i];
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class ResourceViewModel
    {
        public ResourceId resourceId;
        public string displayName;
        public double amount;
        public double amountPerSecond;
        public string amountLabel;
        public string rateLabel;
    }

    [Serializable]
    public sealed class BuildingViewModel
    {
        public BuildingId buildingId;
        public string displayName;
        public string description;
        public int level;
        public bool canBuy;
        public string levelLabel;
        public string costLabel;
        public string productionLabel;
    }

    [Serializable]
    public sealed class UpgradeViewModel
    {
        public UpgradeId upgradeId;
        public string displayName;
        public string description;
        public bool purchased;
        public bool canBuy;
        public string costLabel;
        public string effectLabel;
        public string statusLabel;
    }

    [Serializable]
    public sealed class ChallengeViewModel
    {
        public ChallengeId challengeId;
        public string displayName;
        public string description;
        public int completions;
        public bool active;
        public bool canStart;
        public bool canComplete;
        public string completionGoalLabel;
        public string completionLabel;
        public string modifierLabel;
        public string rewardLabel;
    }
}
