using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatterMutation
{
    public class HarmonyUtils
    {
        public static readonly Harmony Instance = new Harmony("BatterMutation.InnerHarmony");
    }
}
