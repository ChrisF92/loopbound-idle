using System;
using System.Globalization;
using System.Text;
using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Presentation
{
    public sealed class KingdomNumberFormatter
    {
        private static readonly string[] Suffixes =
        {
            "",
            "K",
            "M",
            "B",
            "T",
            "Qa",
            "Qi",
            "Sx",
            "Sp",
            "Oc",
            "No",
            "Dc"
        };

        public string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "0";
            }

            var sign = value < 0d ? "-" : "";
            var absolute = Math.Abs(value);
            if (absolute < 1000d)
            {
                return sign + FormatSmallNumber(absolute);
            }

            var tier = 0;
            while (absolute >= 1000d && tier < Suffixes.Length - 1)
            {
                absolute /= 1000d;
                tier++;
            }

            if (tier == Suffixes.Length - 1 && absolute >= 1000d)
            {
                return value.ToString("0.##e+0", CultureInfo.InvariantCulture);
            }

            return sign + FormatAbbreviatedMantissa(absolute) + Suffixes[tier];
        }

        public string FormatRate(double amountPerSecond)
        {
            return FormatNumber(amountPerSecond) + "/s";
        }

        public string FormatPercent(double value)
        {
            return (value * 100d).ToString("0.#", CultureInfo.InvariantCulture) + "%";
        }

        public string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0d)
            {
                return "0s";
            }

            var totalSeconds = (long)Math.Floor(seconds);
            var hours = totalSeconds / 3600L;
            var minutes = (totalSeconds % 3600L) / 60L;
            var remainingSeconds = totalSeconds % 60L;

            if (hours > 0L)
            {
                return hours.ToString(CultureInfo.InvariantCulture) + "h " + minutes.ToString(CultureInfo.InvariantCulture) + "m";
            }

            if (minutes > 0L)
            {
                return minutes.ToString(CultureInfo.InvariantCulture) + "m " + remainingSeconds.ToString(CultureInfo.InvariantCulture) + "s";
            }

            return remainingSeconds.ToString(CultureInfo.InvariantCulture) + "s";
        }

        public string FormatCosts(ResourceCost[] costs)
        {
            if (costs == null || costs.Length == 0)
            {
                return "Free";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < costs.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(FormatNumber(costs[i].amount));
                builder.Append(' ');
                builder.Append(KingdomDisplayNames.Resource(costs[i].resourceId));
            }

            return builder.ToString();
        }

        private static string FormatSmallNumber(double value)
        {
            if (value > 0d && value < 0.01d)
            {
                return value.ToString("0.####", CultureInfo.InvariantCulture);
            }

            if (value >= 100d)
            {
                return Math.Floor(value).ToString("0", CultureInfo.InvariantCulture);
            }

            if (value >= 10d)
            {
                return value.ToString("0.#", CultureInfo.InvariantCulture);
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string FormatAbbreviatedMantissa(double value)
        {
            if (value >= 100d)
            {
                return value.ToString("0", CultureInfo.InvariantCulture);
            }

            if (value >= 10d)
            {
                return value.ToString("0.#", CultureInfo.InvariantCulture);
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
