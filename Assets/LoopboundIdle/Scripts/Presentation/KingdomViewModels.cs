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
