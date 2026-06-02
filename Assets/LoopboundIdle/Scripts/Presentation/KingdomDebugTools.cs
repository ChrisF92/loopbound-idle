using System;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Presentation
{
    public sealed class KingdomDebugTools
    {
        private readonly KingdomGame game;
        private readonly KingdomBalanceReportFormatter reportFormatter;
        private readonly KingdomBalanceSimulator balanceSimulator;

        public KingdomDebugTools(KingdomGame game)
            : this(game, new KingdomBalanceSimulator(), new KingdomBalanceReportFormatter())
        {
        }

        public KingdomDebugTools(
            KingdomGame game,
            KingdomBalanceSimulator balanceSimulator,
            KingdomBalanceReportFormatter reportFormatter)
        {
            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            this.game = game;
            this.balanceSimulator = balanceSimulator ?? new KingdomBalanceSimulator();
            this.reportFormatter = reportFormatter ?? new KingdomBalanceReportFormatter();
            LastBalanceReportMarkdown = "";
        }

        public string LastBalanceReportMarkdown { get; private set; }

        public bool GrantResource(ResourceId resourceId, double amount)
        {
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
            {
                return false;
            }

            game.State.wallet.Add(resourceId, amount);
            game.RefreshViewModel();
            return true;
        }

        public bool SetResource(ResourceId resourceId, double amount)
        {
            if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < 0d)
            {
                return false;
            }

            game.State.wallet.Set(resourceId, amount);
            game.RefreshViewModel();
            return true;
        }

        public void GrantAllResources(double amount)
        {
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
            {
                return;
            }

            var resources = game.State.wallet.resources;
            for (var i = 0; i < resources.Length; i++)
            {
                game.State.wallet.Add(resources[i].resourceId, amount);
            }

            game.RefreshViewModel();
        }

        public bool GrantActiveChallengeGoals()
        {
            if (game.State.activeChallengeId == ChallengeId.None)
            {
                return false;
            }

            var challenge = game.Catalog.GetChallenge(game.State.activeChallengeId);
            GrantCosts(challenge.completionGoals);
            return true;
        }

        public bool GrantChallengeGoals(ChallengeId challengeId)
        {
            if (challengeId == ChallengeId.None)
            {
                return false;
            }

            var challenge = game.Catalog.GetChallenge(challengeId);
            GrantCosts(challenge.completionGoals);
            return true;
        }

        public bool GrantCollapseTestResources(double amount)
        {
            if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
            {
                return false;
            }

            game.State.wallet.Set(ResourceId.Food, amount);
            game.State.wallet.Set(ResourceId.Wood, amount);
            game.State.wallet.Set(ResourceId.Stone, amount);
            game.State.wallet.Set(ResourceId.Knowledge, amount);
            game.State.wallet.Set(ResourceId.Authority, amount);
            game.RefreshViewModel();
            return true;
        }

        public bool ForceSave()
        {
            return game.Save();
        }

        public bool ForceLoad()
        {
            return game.Load();
        }

        public bool DeleteLocalSave()
        {
            return game.DeleteSave();
        }

        public void ResetState()
        {
            game.NewGame();
        }

        public string GenerateBalanceReportMarkdown()
        {
            return GenerateBalanceReportMarkdown(new BalanceSimulationOptions());
        }

        public string GenerateBalanceReportMarkdown(BalanceSimulationOptions options)
        {
            var report = balanceSimulator.Simulate(game.Catalog, options);
            LastBalanceReportMarkdown = reportFormatter.FormatMarkdown(report, game.Catalog);
            return LastBalanceReportMarkdown;
        }

        private void GrantCosts(ResourceCost[] costs)
        {
            if (costs == null)
            {
                return;
            }

            for (var i = 0; i < costs.Length; i++)
            {
                game.State.wallet.Set(costs[i].resourceId, Math.Max(game.State.wallet.Get(costs[i].resourceId), costs[i].amount));
            }

            game.RefreshViewModel();
        }
    }
}
