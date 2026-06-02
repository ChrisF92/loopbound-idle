using System;

namespace LoopboundIdle.Kingdom.Core
{
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceId resourceId;
        public double amount;

        public ResourceAmount(ResourceId resourceId, double amount)
        {
            this.resourceId = resourceId;
            this.amount = amount;
        }
    }

    [Serializable]
    public struct ResourceCost
    {
        public ResourceId resourceId;
        public double amount;

        public ResourceCost(ResourceId resourceId, double amount)
        {
            this.resourceId = resourceId;
            this.amount = amount;
        }
    }

    [Serializable]
    public sealed class ResourceWallet
    {
        public ResourceAmount[] resources;

        public ResourceWallet()
        {
            resources = CreateDefaultResources();
        }

        public double Get(ResourceId resourceId)
        {
            EnsureInitialized();

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].resourceId == resourceId)
                {
                    return resources[i].amount;
                }
            }

            return 0d;
        }

        public void Set(ResourceId resourceId, double amount)
        {
            EnsureInitialized();

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].resourceId == resourceId)
                {
                    resources[i].amount = SanitizeAmount(amount);
                    return;
                }
            }
        }

        public void Add(ResourceId resourceId, double amount)
        {
            if (amount == 0d)
            {
                return;
            }

            Set(resourceId, Get(resourceId) + amount);
        }

        public bool CanAfford(ResourceCost[] costs)
        {
            if (costs == null)
            {
                return true;
            }

            for (var i = 0; i < costs.Length; i++)
            {
                if (Get(costs[i].resourceId) < costs[i].amount)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Spend(ResourceCost[] costs)
        {
            if (!CanAfford(costs))
            {
                return false;
            }

            if (costs == null)
            {
                return true;
            }

            for (var i = 0; i < costs.Length; i++)
            {
                Add(costs[i].resourceId, -costs[i].amount);
            }

            return true;
        }

        public void ResetLoopResources()
        {
            EnsureInitialized();

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].resourceId != ResourceId.Legacy)
                {
                    resources[i].amount = 0d;
                }
            }
        }

        private void EnsureInitialized()
        {
            if (resources == null || resources.Length == 0)
            {
                resources = CreateDefaultResources();
            }
        }

        private static ResourceAmount[] CreateDefaultResources()
        {
            return new[]
            {
                new ResourceAmount(ResourceId.Population, 12d),
                new ResourceAmount(ResourceId.Food, 25d),
                new ResourceAmount(ResourceId.Wood, 15d),
                new ResourceAmount(ResourceId.Stone, 0d),
                new ResourceAmount(ResourceId.Knowledge, 0d),
                new ResourceAmount(ResourceId.Authority, 0d),
                new ResourceAmount(ResourceId.Legacy, 0d)
            };
        }

        private static double SanitizeAmount(double amount)
        {
            if (double.IsNaN(amount) || double.IsInfinity(amount) || amount < 0d)
            {
                return 0d;
            }

            return amount;
        }
    }
}
