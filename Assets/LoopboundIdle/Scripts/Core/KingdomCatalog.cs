using System;

namespace LoopboundIdle.Kingdom.Core
{
    [Serializable]
    public sealed class KingdomCatalog
    {
        public BuildingDefinition[] buildings;
        public UpgradeDefinition[] upgrades;
        public ChallengeDefinition[] challenges;

        public KingdomCatalog(
            BuildingDefinition[] buildings,
            UpgradeDefinition[] upgrades,
            ChallengeDefinition[] challenges)
        {
            this.buildings = buildings;
            this.upgrades = upgrades;
            this.challenges = challenges;
        }

        public BuildingDefinition GetBuilding(BuildingId buildingId)
        {
            for (var i = 0; i < buildings.Length; i++)
            {
                if (buildings[i].buildingId == buildingId)
                {
                    return buildings[i];
                }
            }

            throw new ArgumentOutOfRangeException("buildingId", buildingId, "Unknown building id.");
        }

        public UpgradeDefinition GetUpgrade(UpgradeId upgradeId)
        {
            for (var i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i].upgradeId == upgradeId)
                {
                    return upgrades[i];
                }
            }

            throw new ArgumentOutOfRangeException("upgradeId", upgradeId, "Unknown upgrade id.");
        }

        public ChallengeDefinition GetChallenge(ChallengeId challengeId)
        {
            for (var i = 0; i < challenges.Length; i++)
            {
                if (challenges[i].challengeId == challengeId)
                {
                    return challenges[i];
                }
            }

            throw new ArgumentOutOfRangeException("challengeId", challengeId, "Unknown challenge id.");
        }

        public static KingdomCatalog CreateDefault()
        {
            return new KingdomCatalog(
                CreateDefaultBuildings(),
                CreateDefaultUpgrades(),
                CreateDefaultChallenges());
        }

        private static BuildingDefinition[] CreateDefaultBuildings()
        {
            return new[]
            {
                new BuildingDefinition(
                    BuildingId.Farm,
                    "Farms",
                    "Fields that keep the kingdom fed through the early years.",
                    new[] { new ResourceCost(ResourceId.Wood, 8d) },
                    1.14d,
                    new[] { new ProductionRate(ResourceId.Food, 0.45d) }),

                new BuildingDefinition(
                    BuildingId.LumberCamp,
                    "Lumber Camps",
                    "Foresters cut enough timber to expand the settlement.",
                    new[] { new ResourceCost(ResourceId.Food, 12d) },
                    1.16d,
                    new[] { new ProductionRate(ResourceId.Wood, 0.25d) }),

                new BuildingDefinition(
                    BuildingId.Quarry,
                    "Quarries",
                    "Stoneworkers prepare the kingdom for sturdier construction.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Food, 35d),
                        new ResourceCost(ResourceId.Wood, 28d)
                    },
                    1.18d,
                    new[] { new ProductionRate(ResourceId.Stone, 0.12d) }),

                new BuildingDefinition(
                    BuildingId.School,
                    "Schools",
                    "Teachers convert prosperity into lasting knowledge.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Wood, 65d),
                        new ResourceCost(ResourceId.Stone, 20d)
                    },
                    1.2d,
                    new[] { new ProductionRate(ResourceId.Knowledge, 0.055d) }),

                new BuildingDefinition(
                    BuildingId.Council,
                    "Councils",
                    "Administrators turn knowledge into royal authority.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Stone, 90d),
                        new ResourceCost(ResourceId.Knowledge, 35d)
                    },
                    1.23d,
                    new[] { new ProductionRate(ResourceId.Authority, 0.025d) }),

                new BuildingDefinition(
                    BuildingId.Archive,
                    "Archives",
                    "Records that improve the value of every future collapse.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Knowledge, 80d),
                        new ResourceCost(ResourceId.Authority, 20d)
                    },
                    1.25d,
                    new ProductionRate[0])
            };
        }

        private static UpgradeDefinition[] CreateDefaultUpgrades()
        {
            return new[]
            {
                new UpgradeDefinition(
                    UpgradeId.CropRotation,
                    "Crop Rotation",
                    "Farm output is increased by planned planting cycles.",
                    new[] { new ResourceCost(ResourceId.Knowledge, 12d) },
                    ResourceId.Food,
                    1.5d),

                new UpgradeDefinition(
                    UpgradeId.ReinforcedAxes,
                    "Reinforced Axes",
                    "Lumber Camps produce more Wood.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Wood, 80d),
                        new ResourceCost(ResourceId.Knowledge, 18d)
                    },
                    ResourceId.Wood,
                    1.45d),

                new UpgradeDefinition(
                    UpgradeId.StoneRoads,
                    "Stone Roads",
                    "Quarries produce more Stone as transport improves.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Stone, 55d),
                        new ResourceCost(ResourceId.Knowledge, 24d)
                    },
                    ResourceId.Stone,
                    1.4d),

                new UpgradeDefinition(
                    UpgradeId.PublicSchools,
                    "Public Schools",
                    "Schools produce more Knowledge.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Food, 250d),
                        new ResourceCost(ResourceId.Knowledge, 45d)
                    },
                    ResourceId.Knowledge,
                    1.5d),

                new UpgradeDefinition(
                    UpgradeId.RoyalCharter,
                    "Royal Charter",
                    "Councils produce more Authority.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Knowledge, 110d),
                        new ResourceCost(ResourceId.Authority, 35d)
                    },
                    ResourceId.Authority,
                    1.6d),

                new UpgradeDefinition(
                    UpgradeId.ChronicleVaults,
                    "Chronicle Vaults",
                    "Legacy gains from collapse are improved.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Knowledge, 160d),
                        new ResourceCost(ResourceId.Authority, 75d)
                    },
                    ResourceId.Legacy,
                    1.25d)
            };
        }

        private static ChallengeDefinition[] CreateDefaultChallenges()
        {
            return new[]
            {
                new ChallengeDefinition(
                    ChallengeId.FamineAge,
                    "Famine Age",
                    "Food production is heavily reduced during this reign.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Food, 600d),
                        new ResourceCost(ResourceId.Knowledge, 45d)
                    },
                    new[] { new ChallengeModifier(ResourceId.Food, 0.35d) },
                    "Improves Farms after each completion."),

                new ChallengeDefinition(
                    ChallengeId.DarkAge,
                    "Dark Age",
                    "Knowledge production is reduced.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Knowledge, 110d),
                        new ResourceCost(ResourceId.Authority, 20d)
                    },
                    new[] { new ChallengeModifier(ResourceId.Knowledge, 0.4d) },
                    "Improves Schools after each completion."),

                new ChallengeDefinition(
                    ChallengeId.Stonebound,
                    "Stonebound",
                    "Wood production is reduced and Stone goals are increased.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Stone, 260d),
                        new ResourceCost(ResourceId.Authority, 30d)
                    },
                    new[] { new ChallengeModifier(ResourceId.Wood, 0.5d) },
                    "Improves Quarries after each completion."),

                new ChallengeDefinition(
                    ChallengeId.ShortReign,
                    "Short Reign",
                    "Collapse pressure rises faster.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Food, 850d),
                        new ResourceCost(ResourceId.Wood, 500d),
                        new ResourceCost(ResourceId.Authority, 45d)
                    },
                    new ChallengeModifier[0],
                    "Improves early-loop acceleration after each completion."),

                new ChallengeDefinition(
                    ChallengeId.SilentCouncil,
                    "Silent Council",
                    "Authority production is reduced.",
                    new[]
                    {
                        new ResourceCost(ResourceId.Knowledge, 220d),
                        new ResourceCost(ResourceId.Authority, 85d)
                    },
                    new[] { new ChallengeModifier(ResourceId.Authority, 0.45d) },
                    "Improves Councils after each completion.")
            };
        }
    }
}
