using GSQ;
using HarmonyLib;
using System;
using System.IO;
using XiaWorld;
using static GMathUtl;

namespace BatterMutation
{
    [HarmonyPatch(typeof(GMathUtl))]
    public class GMathUtl_Patch
    {
        public static bool Enabled = false;

        public static uint Seed = 0;

        [HarmonyPrefix]
        [HarmonyPatch("SetRander")]
        public static void On_SetRander_Prefix(RandomType t, ref GRandom r)
        {
            if (Enabled && t is RandomType.emMutation && r != null)
            {
                r = new GRandom(Seed + r.Seed);
            }
        }
    }
}
