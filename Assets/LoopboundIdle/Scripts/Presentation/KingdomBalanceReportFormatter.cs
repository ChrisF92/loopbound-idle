using System;
using System.Text;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Presentation
{
    public sealed class KingdomBalanceReportFormatter
    {
        private readonly KingdomNumberFormatter formatter;

        public KingdomBalanceReportFormatter()
            : this(new KingdomNumberFormatter())
        {
        }

        public KingdomBalanceReportFormatter(KingdomNumberFormatter formatter)
        {
            this.formatter = formatter ?? new KingdomNumberFormatter();
        }

        public string FormatMarkdown(BalanceSimulationReport report, KingdomCatalog catalog)
        {
            if (report == null)
            {
                return "# Balance Simulation Report\n\nNo report data.";
            }

            var builder = new StringBuilder();
            builder.AppendLine("# Balance Simulation Report");
            builder.AppendLine();
            builder.AppendLine("- Building purchases: " + report.buildingPurchases);
            builder.AppendLine("- Upgrade purchases: " + report.upgradePurchases);
            builder.AppendLine();
            builder.AppendLine("| Time | Building Buys | Upgrade Buys | Projected Legacy | Resources | Buildings | Upgrades |");
            builder.AppendLine("| --- | ---: | ---: | ---: | --- | --- | --- |");

            if (report.snapshots != null)
            {
                for (var i = 0; i < report.snapshots.Length; i++)
                {
                    AppendSnapshotRow(builder, report.snapshots[i], catalog);
                }
            }

            return builder.ToString();
        }

        private void AppendSnapshotRow(StringBuilder builder, BalanceSimulationSnapshot snapshot, KingdomCatalog catalog)
        {
            if (snapshot == null)
            {
                return;
            }

            builder.Append("| ");
            builder.Append(formatter.FormatDuration(snapshot.timeSeconds));
            builder.Append(" | ");
            builder.Append(snapshot.buildingPurchases);
            builder.Append(" | ");
            builder.Append(snapshot.upgradePurchases);
            builder.Append(" | ");
            builder.Append(formatter.FormatNumber(snapshot.projectedLegacyReward));
            builder.Append(" | ");
            builder.Append(FormatResources(snapshot.resources));
            builder.Append(" | ");
            builder.Append(FormatBuildings(snapshot.buildings, catalog));
            builder.Append(" | ");
            builder.Append(FormatUpgrades(snapshot.upgrades, catalog));
            builder.AppendLine(" |");
        }

        private string FormatResources(ResourceAmount[] resources)
        {
            if (resources == null || resources.Length == 0)
            {
                return "None";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < resources.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append("<br>");
                }

                builder.Append(KingdomDisplayNames.Resource(resources[i].resourceId));
                builder.Append(": ");
                builder.Append(formatter.FormatNumber(resources[i].amount));
            }

            return builder.ToString();
        }

        private string FormatBuildings(BuildingProgress[] buildings, KingdomCatalog catalog)
        {
            if (buildings == null || buildings.Length == 0)
            {
                return "None";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < buildings.Length; i++)
            {
                if (buildings[i] == null || buildings[i].level <= 0)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append("<br>");
                }

                builder.Append(GetBuildingName(catalog, buildings[i].buildingId));
                builder.Append(" L");
                builder.Append(buildings[i].level);
            }

            return builder.Length == 0 ? "None" : builder.ToString();
        }

        private string FormatUpgrades(UpgradeProgress[] upgrades, KingdomCatalog catalog)
        {
            if (upgrades == null || upgrades.Length == 0)
            {
                return "None";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] == null || !upgrades[i].purchased)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append("<br>");
                }

                builder.Append(GetUpgradeName(catalog, upgrades[i].upgradeId));
            }

            return builder.Length == 0 ? "None" : builder.ToString();
        }

        private static string GetBuildingName(KingdomCatalog catalog, BuildingId buildingId)
        {
            if (catalog == null)
            {
                return buildingId.ToString();
            }

            try
            {
                return catalog.GetBuilding(buildingId).displayName;
            }
            catch (ArgumentOutOfRangeException)
            {
                return buildingId.ToString();
            }
        }

        private static string GetUpgradeName(KingdomCatalog catalog, UpgradeId upgradeId)
        {
            if (catalog == null)
            {
                return upgradeId.ToString();
            }

            try
            {
                return catalog.GetUpgrade(upgradeId).displayName;
            }
            catch (ArgumentOutOfRangeException)
            {
                return upgradeId.ToString();
            }
        }
    }
}
