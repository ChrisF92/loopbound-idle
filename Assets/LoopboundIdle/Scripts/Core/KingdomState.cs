using System;

namespace LoopboundIdle.Kingdom.Core
{
    [Serializable]
    public sealed class BuildingProgress
    {
        public BuildingId buildingId;
        public int level;

        public BuildingProgress(BuildingId buildingId, int level)
        {
            this.buildingId = buildingId;
            this.level = level;
        }
    }

    [Serializable]
    public sealed class UpgradeProgress
    {
        public UpgradeId upgradeId;
        public bool purchased;

        public UpgradeProgress(UpgradeId upgradeId, bool purchased)
        {
            this.upgradeId = upgradeId;
            this.purchased = purchased;
        }
    }

    [Serializable]
    public sealed class ChallengeProgress
    {
        public ChallengeId challengeId;
        public int completions;

        public ChallengeProgress(ChallengeId challengeId, int completions)
        {
            this.challengeId = challengeId;
            this.completions = completions;
        }
    }

    [Serializable]
    public sealed class KingdomState
    {
        public int saveVersion;
        public int loopIndex;
        public double elapsedSeconds;
        public double collapsePressure;
        public long lastSavedUnixTimeSeconds;
        public ChallengeId activeChallengeId;
        public ResourceWallet wallet;
        public BuildingProgress[] buildings;
        public UpgradeProgress[] upgrades;
        public ChallengeProgress[] challenges;

        public KingdomState()
        {
            saveVersion = 1;
            loopIndex = 1;
            elapsedSeconds = 0d;
            collapsePressure = 0d;
            lastSavedUnixTimeSeconds = 0L;
            activeChallengeId = ChallengeId.None;
            wallet = new ResourceWallet();
            buildings = new BuildingProgress[0];
            upgrades = new UpgradeProgress[0];
            challenges = new ChallengeProgress[0];
        }

        public static KingdomState CreateNew(KingdomCatalog catalog)
        {
            var state = new KingdomState
            {
                buildings = new BuildingProgress[catalog.buildings.Length],
                upgrades = new UpgradeProgress[catalog.upgrades.Length],
                challenges = new ChallengeProgress[catalog.challenges.Length]
            };

            for (var i = 0; i < catalog.buildings.Length; i++)
            {
                state.buildings[i] = new BuildingProgress(catalog.buildings[i].buildingId, 0);
            }

            for (var i = 0; i < catalog.upgrades.Length; i++)
            {
                state.upgrades[i] = new UpgradeProgress(catalog.upgrades[i].upgradeId, false);
            }

            for (var i = 0; i < catalog.challenges.Length; i++)
            {
                state.challenges[i] = new ChallengeProgress(catalog.challenges[i].challengeId, 0);
            }

            return state;
        }

        public BuildingProgress GetBuildingProgress(BuildingId buildingId)
        {
            for (var i = 0; i < buildings.Length; i++)
            {
                if (buildings[i].buildingId == buildingId)
                {
                    return buildings[i];
                }
            }

            throw new ArgumentOutOfRangeException("buildingId", buildingId, "Unknown building progress.");
        }

        public UpgradeProgress GetUpgradeProgress(UpgradeId upgradeId)
        {
            for (var i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i].upgradeId == upgradeId)
                {
                    return upgrades[i];
                }
            }

            throw new ArgumentOutOfRangeException("upgradeId", upgradeId, "Unknown upgrade progress.");
        }

        public ChallengeProgress GetChallengeProgress(ChallengeId challengeId)
        {
            for (var i = 0; i < challenges.Length; i++)
            {
                if (challenges[i].challengeId == challengeId)
                {
                    return challenges[i];
                }
            }

            throw new ArgumentOutOfRangeException("challengeId", challengeId, "Unknown challenge progress.");
        }
    }
}
