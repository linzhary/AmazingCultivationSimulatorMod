using GSQ;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatterMutation
{
    [HarmonyPatch(typeof(GMathUtl))]
    public class GMathUtil_Patch
    {
        public static bool Enabled = false;

        [HarmonyPrefix]
        [HarmonyPatch("SetRander")]
        public static void On_SetRander_Prefix(GMathUtl.RandomType t, ref GRandom random)
        {
            if (Enabled && t is GMathUtl.RandomType.emMutation)
            {
                random = new GRandom((uint)(DateTimeOffset.UtcNow.Ticks + new Random().Next(1000, 9999)));
            }
        }
    }
}
