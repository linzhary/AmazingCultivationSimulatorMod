using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using UnityEngine;
using XiaWorld;

namespace TestConsoleApp
{
    [HarmonyPatch(typeof(Panel_MutationChoose))]
    public class TestPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("WaitCardSelect", MethodType.Enumerator)]
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
                    new CodeInstruction(OpCodes.Call, typeof(TestPatch).GetMethod("Me", BindingFlags.Static | BindingFlags.Public)),
                });
            }

            return codes;
        }

        public static void Me()
        {

        }

        //public static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions)
        //{
        //    var codes = instructions.ToList();
        //    var stateOprand = codes[1].operand;
        //    var insertIndex = -1;
        //    for (var i = 0; i < codes.Count(); i++)
        //    {
        //        var code = codes.ElementAt(i);
        //        if (code.opcode == OpCodes.Call
        //            && code.operand is MethodInfo methodInfo
        //            && methodInfo.Name == "OpenPanel")
        //        {
        //            var code0 = codes.ElementAtOrDefault(i - 1);
        //            if (code0.opcode == OpCodes.Ldc_I4_3)
        //            {
        //                insertIndex = i - 3;
        //            }
        //        }
        //    }
        //    if (insertIndex > -1)
        //    {
        //        var skipLabel = new Label();
        //        // 插入自定义代码
        //        var newCodes = new List<CodeInstruction>
        //        {
        //            new CodeInstruction(OpCodes.Ldarg_0),
        //            new CodeInstruction(OpCodes.Ldfld, typeof(TestClass).GetField("SelectResults", BindingFlags.NonPublic | BindingFlags.Instance)),  // 访问 SelectResults 字段
        //            new CodeInstruction(OpCodes.Callvirt, typeof(List<int>).GetMethod("get_Count()", new[] { typeof(int) })),  // SelectResults.Count
        //            new CodeInstruction(OpCodes.Ldc_I4_1), // 1
        //            new CodeInstruction(OpCodes.Sub), // SelectResults.Count - 1
        //            new CodeInstruction(OpCodes.Stloc_0),  // 将 Sub的结果存到loc_0
        //            new CodeInstruction(OpCodes.Callvirt, typeof(List<int>).GetMethod("get_Item()", new[] { typeof(int) })),  // SelectResults[SelectResults.Count - 1]
        //            new CodeInstruction(OpCodes.Isinst, typeof(MutationReGenerateResult)),  // 判断是否是 MutationReGenerateResult
        //            new CodeInstruction(OpCodes.Brfalse_S, skipLabel),  // 如果不是，跳到跳过 skipLabel 的部分
        //            new CodeInstruction(OpCodes.Ldloc_0),  // 加载 loc_0
        //            new CodeInstruction(OpCodes.Ldfld, typeof(NewTestResult).GetField("Data")),  // 获取 loc_0.Data
        //            new CodeInstruction(OpCodes.Ldarg_1),  // 加载 data
        //            new CodeInstruction(OpCodes.Stfld, typeof(NewTestResult).GetField("Data")),  // 设置 loc_0.Data
        //            new CodeInstruction(OpCodes.Ldarg_0),
        //            new CodeInstruction(OpCodes.Ldc_I4_M1),
        //            new CodeInstruction(OpCodes.Stfld, stateOprand),
        //            new CodeInstruction(OpCodes.Ldc_I4_0),
        //            new CodeInstruction(OpCodes.Ret),
        //            new CodeInstruction(OpCodes.Ldarg_0)
        //            {
        //                labels = new List<Label>{ skipLabel} 
        //            },
        //        };
        //        codes.InsertRange(insertIndex, newCodes);
        //    }
        //    return codes;
        //}
    }
    public class TestResult
    {
        public int Data;
    }
    public class NewTestResult : TestResult
    {

    }

    public enum PanelType
    {
        Zero,
        One,
        Two
    }

    public class TestClassBase
    {
        private readonly List<TestResult> _vals = new List<TestResult> {
            new TestResult(),
            new TestResult(),
            new NewTestResult()
        };

        public void OpenPanel(int v)
        {
        }

        public IEnumerator Run(PanelType panelType, params TestResult[] objs)
        {
            yield break;
        }

        public IEnumerator GetEnumerator(int data)
        {
            OpenPanel(0);
            yield return Run(PanelType.Zero);
            OpenPanel(1);
            yield return Run(PanelType.One);
            var _val = _vals[_vals.Count - 1];
            if (_val is NewTestResult)
            {
                _val.Data = data;
                yield break;
            }
            OpenPanel(2);
            yield return Run(PanelType.Two, _vals[_vals.Count - 1]);
            yield break;
        }
    }
    public class TestClass : TestClassBase
    {

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Assembly.LoadFrom("Assembly-CSharp.dll");
            var harmony = new Harmony("linzhary");
            harmony.PatchAll();
            var testClass = new TestClass();
            var enumA = testClass.GetEnumerator(0);
            while (enumA.MoveNext())
            {
                Console.WriteLine(enumA.Current);
            }
            Console.ReadKey();
        }
    }
}
