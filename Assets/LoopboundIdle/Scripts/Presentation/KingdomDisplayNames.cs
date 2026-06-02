using LoopboundIdle.Kingdom.Core;

namespace LoopboundIdle.Kingdom.Presentation
{
    public static class KingdomDisplayNames
    {
        public static string Resource(ResourceId resourceId)
        {
            switch (resourceId)
            {
                case ResourceId.Population:
                    return "Population";
                case ResourceId.Food:
                    return "Food";
                case ResourceId.Wood:
                    return "Wood";
                case ResourceId.Stone:
                    return "Stone";
                case ResourceId.Knowledge:
                    return "Knowledge";
                case ResourceId.Authority:
                    return "Authority";
                case ResourceId.Legacy:
                    return "Legacy";
                default:
                    return resourceId.ToString();
            }
        }
    }
}
