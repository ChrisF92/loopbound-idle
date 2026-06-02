using System.Text;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Presentation
{
    public sealed class KingdomViewModelBuilder
    {
        private readonly KingdomNumberFormatter formatter;

        public KingdomViewModelBuilder()
            : this(new KingdomNumberFormatter())
        {
        }

        public KingdomViewModelBuilder(KingdomNumberFormatter formatter)
        {
            this.formatter = formatter ?? new KingdomNumberFormatter();
        }

        public KingdomGameViewModel Build(
            KingdomState state,
            KingdomCatalog catalog,
            KingdomSimulator simulator,
            double lastOfflineSeconds)
        {
            var viewModel = new KingdomGameViewModel();
            if (state == null || catalog == null || simulator == null)
            {
                return viewModel;
            }

            viewModel.loopIndex = state.loopIndex;
            viewModel.loopLabel = "Loop " + state.loopIndex;
            viewModel.elapsedLabel = formatter.FormatDuration(state.elapsedSeconds);
            viewModel.collapsePressureLabel = formatter.FormatNumber(state.collapsePressure);
            viewModel.collapsePressureRateLabel = formatter.FormatRate(simulator.CalculateCollapsePressurePerSecond(state));
            viewModel.projectedLegacyRewardLabel = formatter.FormatNumber(simulator.CalculateLegacyReward(state, catalog)) + " Legacy";
            viewModel.activeChallengeId = state.activeChallengeId;
            viewModel.activeChallengeLabel = GetActiveChallengeLabel(state, catalog);
            viewModel.canCompleteActiveChallenge = simulator.CanCompleteActiveChallenge(state, catalog);
            viewModel.lastOfflineSeconds = lastOfflineSeconds;
            viewModel.lastOfflineLabel = formatter.FormatDuration(lastOfflineSeconds);
            viewModel.resources = BuildResources(state, catalog, simulator);
            viewModel.buildings = BuildBuildings(state, catalog, simulator);
            viewModel.upgrades = BuildUpgrades(state, catalog, simulator);
            viewModel.challenges = BuildChallenges(state, catalog, simulator);

            return viewModel;
        }

        private ResourceViewModel[] BuildResources(KingdomState state, KingdomCatalog catalog, KingdomSimulator simulator)
        {
            var resources = state.wallet.resources;
            var viewModels = new ResourceViewModel[resources.Length];
            for (var i = 0; i < resources.Length; i++)
            {
                var resourceId = resources[i].resourceId;
                var perSecond = simulator.CalculateProductionPerSecond(state, catalog, resourceId);
                viewModels[i] = new ResourceViewModel
                {
                    resourceId = resourceId,
                    displayName = KingdomDisplayNames.Resource(resourceId),
                    amount = resources[i].amount,
                    amountPerSecond = perSecond,
                    amountLabel = formatter.FormatNumber(resources[i].amount),
                    rateLabel = formatter.FormatRate(perSecond)
                };
            }

            return viewModels;
        }

        private BuildingViewModel[] BuildBuildings(KingdomState state, KingdomCatalog catalog, KingdomSimulator simulator)
        {
            var viewModels = new BuildingViewModel[catalog.buildings.Length];
            for (var i = 0; i < catalog.buildings.Length; i++)
            {
                var definition = catalog.buildings[i];
                var progress = state.GetBuildingProgress(definition.buildingId);
                viewModels[i] = new BuildingViewModel
                {
                    buildingId = definition.buildingId,
                    displayName = definition.displayName,
                    description = definition.description,
                    level = progress.level,
                    canBuy = simulator.CanBuyBuilding(state, catalog, definition.buildingId),
                    levelLabel = "Level " + progress.level,
                    costLabel = formatter.FormatCosts(definition.CostForNextLevel(progress.level)),
                    productionLabel = FormatProduction(definition.production)
                };
            }

            return viewModels;
        }

        private UpgradeViewModel[] BuildUpgrades(KingdomState state, KingdomCatalog catalog, KingdomSimulator simulator)
        {
            var viewModels = new UpgradeViewModel[catalog.upgrades.Length];
            for (var i = 0; i < catalog.upgrades.Length; i++)
            {
                var definition = catalog.upgrades[i];
                var progress = state.GetUpgradeProgress(definition.upgradeId);
                viewModels[i] = new UpgradeViewModel
                {
                    upgradeId = definition.upgradeId,
                    displayName = definition.displayName,
                    description = definition.description,
                    purchased = progress.purchased,
                    canBuy = simulator.CanBuyUpgrade(state, catalog, definition.upgradeId),
                    costLabel = formatter.FormatCosts(definition.costs),
                    effectLabel = KingdomDisplayNames.Resource(definition.affectedResource) + " x" + formatter.FormatNumber(definition.productionMultiplier),
                    statusLabel = progress.purchased ? "Purchased" : "Available"
                };
            }

            return viewModels;
        }

        private ChallengeViewModel[] BuildChallenges(KingdomState state, KingdomCatalog catalog, KingdomSimulator simulator)
        {
            var viewModels = new ChallengeViewModel[catalog.challenges.Length];
            for (var i = 0; i < catalog.challenges.Length; i++)
            {
                var definition = catalog.challenges[i];
                var progress = state.GetChallengeProgress(definition.challengeId);
                var active = state.activeChallengeId == definition.challengeId;
                viewModels[i] = new ChallengeViewModel
                {
                    challengeId = definition.challengeId,
                    displayName = definition.displayName,
                    description = definition.description,
                    completions = progress.completions,
                    active = active,
                    canStart = !active,
                    canComplete = active && simulator.CanCompleteActiveChallenge(state, catalog),
                    completionGoalLabel = formatter.FormatCosts(definition.completionGoals),
                    completionLabel = progress.completions + " completions",
                    modifierLabel = FormatModifiers(definition.modifiers),
                    rewardLabel = definition.rewardDescription
                };
            }

            return viewModels;
        }

        private string FormatProduction(ProductionRate[] production)
        {
            if (production == null || production.Length == 0)
            {
                return "No direct production";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < production.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(formatter.FormatRate(production[i].amountPerSecond));
                builder.Append(' ');
                builder.Append(KingdomDisplayNames.Resource(production[i].resourceId));
            }

            return builder.ToString();
        }

        private string FormatModifiers(ChallengeModifier[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                return "No production modifier";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < modifiers.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(KingdomDisplayNames.Resource(modifiers[i].affectedResource));
                builder.Append(" production ");
                builder.Append(formatter.FormatPercent(modifiers[i].productionMultiplier));
            }

            return builder.ToString();
        }

        private static string GetActiveChallengeLabel(KingdomState state, KingdomCatalog catalog)
        {
            if (state.activeChallengeId == ChallengeId.None)
            {
                return "No active challenge";
            }

            return catalog.GetChallenge(state.activeChallengeId).displayName;
        }
    }
}
