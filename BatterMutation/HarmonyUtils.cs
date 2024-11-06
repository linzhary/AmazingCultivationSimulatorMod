using HarmonyLib;

namespace BatterMutation
{
    public class HarmonyUtils
    {
        public static readonly Harmony Instance = new Harmony("BatterMutation.InnerHarmony");
    }
}
