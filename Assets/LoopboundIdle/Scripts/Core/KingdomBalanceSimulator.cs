using System;

namespace LoopboundIdle.Kingdom.Core
{
    [Serializable]
    public sealed class BalanceSimulationOptions
    {
        public double durationSeconds;
        public double stepSeconds;
        public double[] snapshotSeconds;
        public bool buyBuildings;
        public bool buyUpgrades;
        public int maxPurchasesPerStep;

        public BalanceSimulationOptions()
        {
            durationSeconds = 15d * 60d;
            stepSeconds = 1d;
            snapshotSeconds = new[] { 60d, 5d * 60d, 15d * 60d };
            buyBuildings = true;
            buyUpgrades = true;
            maxPurchasesPerStep = 25;
        }
    }

    [Serializable]
    public sealed class BalanceSimulationReport
    {
        public BalanceSimulationSnapshot[] snapshots;
        public KingdomState finalState;
        public int buildingPurchases;
        public int upgradePurchases;

        public BalanceSimulationReport()
        {
            snapshots = new BalanceSimulationSnapshot[0];
        }
    }

    [Serializable]
    public sealed class BalanceSimulationSnapshot
    {
        public double timeSeconds;
        public ResourceAmount[] resources;
        public BuildingProgress[] buildings;
        public UpgradeProgress[] upgrades;
        public int buildingPurchases;
        public int upgradePurchases;
        public double projectedLegacyReward;

        public BalanceSimulationSnapshot()
        {
            resources = new ResourceAmount[0];
            buildings = new BuildingProgress[0];
            upgrades = new UpgradeProgress[0];
        }
    }

    public sealed class KingdomBalanceSimulator
    {
        private readonly KingdomSimulator simulator;

        public KingdomBalanceSimulator()
            : this(new KingdomSimulator())
        {
        }

        public KingdomBalanceSimulator(KingdomSimulator simulator)
        {
            this.simulator = simulator ?? new KingdomSimulator();
        }

        public BalanceSimulationReport Simulate(KingdomCatalog catalog)
        {
            return Simulate(catalog, KingdomState.CreateNew(catalog), new BalanceSimulationOptions());
        }

        public BalanceSimulationReport Simulate(KingdomCatalog catalog, BalanceSimulationOptions options)
        {
            return Simulate(catalog, KingdomState.CreateNew(catalog), options);
        }

        public BalanceSimulationReport Simulate(KingdomCatalog catalog, KingdomState initialState, BalanceSimulationOptions options)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException("catalog");
            }

            if (initialState == null)
            {
                throw new ArgumentNullException("initialState");
            }

            options = NormalizeOptions(options);
            var state = CloneState(initialState);
            var report = new BalanceSimulationReport
            {
                snapshots = new BalanceSimulationSnapshot[options.snapshotSeconds.Length]
            };

            var elapsed = 0d;
            var nextSnapshotIndex = 0;

            while (nextSnapshotIndex < options.snapshotSeconds.Length && options.snapshotSeconds[nextSnapshotIndex] <= 0d)
            {
                report.snapshots[nextSnapshotIndex] = CreateSnapshot(
                    state,
                    catalog,
                    0d,
                    report.buildingPurchases,
                    report.upgradePurchases);
                nextSnapshotIndex++;
            }

            while (elapsed < options.durationSeconds)
            {
                RunAutobuyer(state, catalog, options, report);

                var step = Math.Min(options.stepSeconds, options.durationSeconds - elapsed);
                simulator.Advance(state, catalog, step);
                elapsed += step;

                RunAutobuyer(state, catalog, options, report);

                while (nextSnapshotIndex < options.snapshotSeconds.Length && elapsed >= options.snapshotSeconds[nextSnapshotIndex])
                {
                    report.snapshots[nextSnapshotIndex] = CreateSnapshot(
                        state,
                        catalog,
                        options.snapshotSeconds[nextSnapshotIndex],
                        report.buildingPurchases,
                        report.upgradePurchases);
                    nextSnapshotIndex++;
                }
            }

            while (nextSnapshotIndex < options.snapshotSeconds.Length)
            {
                report.snapshots[nextSnapshotIndex] = CreateSnapshot(
                    state,
                    catalog,
                    elapsed,
                    report.buildingPurchases,
                    report.upgradePurchases);
                nextSnapshotIndex++;
            }

            report.finalState = CloneState(state);
            return report;
        }

        private void RunAutobuyer(
            KingdomState state,
            KingdomCatalog catalog,
            BalanceSimulationOptions options,
            BalanceSimulationReport report)
        {
            var purchasesRemaining = Math.Max(1, options.maxPurchasesPerStep);
            var purchased = true;

            while (purchased && purchasesRemaining > 0)
            {
                purchased = false;

                if (options.buyBuildings)
                {
                    for (var i = 0; i < catalog.buildings.Length && purchasesRemaining > 0; i++)
                    {
                        if (simulator.TryBuyBuilding(state, catalog, catalog.buildings[i].buildingId))
                        {
                            report.buildingPurchases++;
                            purchasesRemaining--;
                            purchased = true;
                        }
                    }
                }

                if (options.buyUpgrades)
                {
                    for (var i = 0; i < catalog.upgrades.Length && purchasesRemaining > 0; i++)
                    {
                        if (simulator.TryBuyUpgrade(state, catalog, catalog.upgrades[i].upgradeId))
                        {
                            report.upgradePurchases++;
                            purchasesRemaining--;
                            purchased = true;
                        }
                    }
                }
            }
        }

        private BalanceSimulationSnapshot CreateSnapshot(
            KingdomState state,
            KingdomCatalog catalog,
            double timeSeconds,
            int buildingPurchases,
            int upgradePurchases)
        {
            return new BalanceSimulationSnapshot
            {
                timeSeconds = timeSeconds,
                resources = CloneResources(state.wallet.resources),
                buildings = CloneBuildings(state.buildings),
                upgrades = CloneUpgrades(state.upgrades),
                buildingPurchases = buildingPurchases,
                upgradePurchases = upgradePurchases,
                projectedLegacyReward = simulator.CalculateLegacyReward(state, catalog)
            };
        }

        private static BalanceSimulationOptions NormalizeOptions(BalanceSimulationOptions options)
        {
            options = options ?? new BalanceSimulationOptions();

            var normalized = new BalanceSimulationOptions
            {
                durationSeconds = Math.Max(0d, options.durationSeconds),
                stepSeconds = options.stepSeconds > 0d ? options.stepSeconds : 1d,
                snapshotSeconds = CloneSnapshotSeconds(options.snapshotSeconds, Math.Max(0d, options.durationSeconds)),
                buyBuildings = options.buyBuildings,
                buyUpgrades = options.buyUpgrades,
                maxPurchasesPerStep = Math.Max(1, options.maxPurchasesPerStep)
            };

            Array.Sort(normalized.snapshotSeconds);
            return normalized;
        }

        private static double[] CloneSnapshotSeconds(double[] snapshotSeconds, double durationSeconds)
        {
            if (snapshotSeconds == null || snapshotSeconds.Length == 0)
            {
                return new[] { durationSeconds };
            }

            var cloned = new double[snapshotSeconds.Length];
            for (var i = 0; i < snapshotSeconds.Length; i++)
            {
                cloned[i] = Math.Max(0d, snapshotSeconds[i]);
            }

            return cloned;
        }

        private static KingdomState CloneState(KingdomState state)
        {
            var wallet = state.wallet ?? new ResourceWallet();

            return new KingdomState
            {
                saveVersion = state.saveVersion,
                loopIndex = state.loopIndex,
                elapsedSeconds = state.elapsedSeconds,
                collapsePressure = state.collapsePressure,
                lastSavedUnixTimeSeconds = state.lastSavedUnixTimeSeconds,
                activeChallengeId = state.activeChallengeId,
                wallet = new ResourceWallet
                {
                    resources = CloneResources(wallet.resources)
                },
                buildings = CloneBuildings(state.buildings),
                upgrades = CloneUpgrades(state.upgrades),
                challenges = CloneChallenges(state.challenges)
            };
        }

        private static ResourceAmount[] CloneResources(ResourceAmount[] resources)
        {
            if (resources == null)
            {
                return new ResourceAmount[0];
            }

            var cloned = new ResourceAmount[resources.Length];
            for (var i = 0; i < resources.Length; i++)
            {
                cloned[i] = resources[i];
            }

            return cloned;
        }

        private static BuildingProgress[] CloneBuildings(BuildingProgress[] buildings)
        {
            if (buildings == null)
            {
                return new BuildingProgress[0];
            }

            var cloned = new BuildingProgress[buildings.Length];
            for (var i = 0; i < buildings.Length; i++)
            {
                cloned[i] = new BuildingProgress(buildings[i].buildingId, buildings[i].level);
            }

            return cloned;
        }

        private static UpgradeProgress[] CloneUpgrades(UpgradeProgress[] upgrades)
        {
            if (upgrades == null)
            {
                return new UpgradeProgress[0];
            }

            var cloned = new UpgradeProgress[upgrades.Length];
            for (var i = 0; i < upgrades.Length; i++)
            {
                cloned[i] = new UpgradeProgress(upgrades[i].upgradeId, upgrades[i].purchased);
            }

            return cloned;
        }

        private static ChallengeProgress[] CloneChallenges(ChallengeProgress[] challenges)
        {
            if (challenges == null)
            {
                return new ChallengeProgress[0];
            }

            var cloned = new ChallengeProgress[challenges.Length];
            for (var i = 0; i < challenges.Length; i++)
            {
                cloned[i] = new ChallengeProgress(challenges[i].challengeId, challenges[i].completions);
            }

            return cloned;
        }
    }
}
