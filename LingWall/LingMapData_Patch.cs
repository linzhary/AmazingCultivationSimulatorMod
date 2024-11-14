using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using XiaWorld;
using System.IO;
using System;

namespace LingWall
{
    [HarmonyPatch(typeof(LingMapData))]
    public class LingMapData_Patch
    {
        private static readonly string[] _validateDefs = new[] { "WallBase", "DoorBase" };
        private static readonly string[] _validateStuffDefs = new[] { "Item_LingStoneBlock", "Item_LingCrystalBlock" };

        [HarmonyTranspiler]
        [HarmonyPatch("UpdateLingUnit")]
        public static IEnumerable<CodeInstruction> On_UpdateLingUnit_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = instructions.ToList();
            var continueLabel = il.DefineLabel();
            yield return new CodeInstruction(OpCodes.Ldarg, 0);
            yield return new CodeInstruction(OpCodes.Ldarg, 1);
            yield return new CodeInstruction(OpCodes.Ldarg, 2);
            yield return new CodeInstruction(OpCodes.Ldarg, 4);
            yield return new CodeInstruction(OpCodes.Call, typeof(LingMapData_Patch).GetMethod("FilterBuilding", BindingFlags.Static | BindingFlags.Public));
            yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);
            yield return new CodeInstruction(OpCodes.Ret);
            yield return new CodeInstruction(OpCodes.Nop).WithLabels(continueLabel);
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.opcode == OpCodes.Callvirt
                    && code.operand is MethodInfo methodInfo
                    && methodInfo.Name == "GetNeighbor")
                {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Call, typeof(LingMapData_Patch).GetMethod("FilterNeighbor", BindingFlags.Static | BindingFlags.Public));
                }
                else
                {
                    yield return code;
                }

            }
            yield break;
        }
        public static bool FilterBuilding(LingMapData __instance, int dkey, bool justmake, bool justdraw)
        {
            if (justdraw || justmake) return true;
            if (World.Instance.map.Things.GetThingAtGrid(dkey, g_emThingType.Building) is BuildingThing building
                && building.IsValid
                && _validateDefs.Contains(building.def.Parent)
                && _validateStuffDefs.Contains(building.StuffDef.Name))
            {
                var gridData = __instance.m_cLQ.GetGridData(dkey, null);
                if (gridData != null)
                {
                    gridData.Value = 0;
                }
                return false;
            }
            return true;
        }
        public static List<int> FilterNeighbor(List<int> neighbor)
        {
            var result = new List<int>();
            foreach (var nKey in neighbor)
            {
                if (World.Instance.map.Things.GetThingAtGrid(nKey, g_emThingType.Building) is BuildingThing building
                    && _validateDefs.Contains(building.def.Parent)
                    && _validateStuffDefs.Contains(building.StuffDef.Name))
                {
                    continue;
                }
                result.Add(nKey);
            }
            return result;
        }
    }
}
