using BatterMutation;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using XiaWorld;

namespace TestConsoleApp
{
    [HarmonyPatch]
    public class Patch
    {
        public static MethodBase TargetMethod()
        {
            var nestTypes = typeof(Panel_MutationChoose).GetNestedTypes(BindingFlags.NonPublic);
            var type = typeof(Panel_MutationChoose).GetNestedTypes(BindingFlags.NonPublic)
                .Where(x => x.Name.StartsWith("<WaitCardSelect>"))
                .First();
            var method = type.GetMethod("MoveNext");
            return method;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> On_WaitCardSelect_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            var foundCreateFunc = false;
            var insertIndex = -1;
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                if (foundCreateFunc == true)
                {
                    if (code.opcode == OpCodes.Newobj
                        && code.operand is ConstructorInfo constructorInfo
                        && constructorInfo.DeclaringType.Name == typeof(WaitUntil).Name)
                    {
                        insertIndex = i - 3;
                        break;
                    }
                    foundCreateFunc = false;
                }

                if (!foundCreateFunc)
                {
                    if (code.opcode == OpCodes.Newobj
                        && code.operand is ConstructorInfo constructorInfo
                        && constructorInfo.DeclaringType.Name == typeof(Func<>).Name)
                    {
                        foundCreateFunc = true;
                    }
                }
            }

            if (insertIndex > -1)
            {
                codes.InsertRange(insertIndex, new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Call, typeof(Panel_MutationChoose_Patch).GetMethod("ShowCardRedrawBtn", BindingFlags.Static | BindingFlags.Public)),
                });
            }

            return codes;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Assembly.LoadFrom("Assembly-CSharp.dll");
            var harmony = new Harmony("linzhary");
            harmony.PatchAll();
            Console.ReadKey();
        }
    }
}
