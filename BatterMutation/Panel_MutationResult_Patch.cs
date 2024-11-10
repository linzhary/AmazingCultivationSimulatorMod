using GSQ;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using XiaWorld;

namespace BatterMutation
{
    [HarmonyPatch(typeof(Panel_MutationResult))]
    public class Panel_MutationResult_Patch
    {
        internal static readonly MethodInfo Method_OpenPanel = typeof(Wnd_MutationMain).GetMethod("OpenPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly FieldInfo Field_SelectResults = typeof(Wnd_MutationMain).GetField("SelectResults", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly FieldInfo Field_RecordData = typeof(MutationMgr).GetField("RecordData", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly MethodInfo Method_RandomMutationData = typeof(MutationMgr).GetMethod("RandomMutationData", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPrefix]
        [HarmonyPatch("Run")]
        public static bool On_Run_Prefix(Panel_MutationResult __instance, MutationData data, object[] objs, ref IEnumerator __result)
        {
            if (data == null) return true;
            if (objs.SafeGet(0) as MutationRedrawResult != null)
            {
                __result = GetEnumerator(__instance, data);
                return false;
            }
            return true;
        }

        private static IEnumerator GetEnumerator(Panel_MutationResult instance, MutationData data)
        {
            var selectResults = Field_SelectResults.GetValue(Wnd_MutationMain.Instance) as List<MutationSelectResult>;
            selectResults.RemoveAt(selectResults.Count - 1);
            Method_OpenPanel.Invoke(Wnd_MutationMain.Instance, new object[] { g_emMutationPanel.Choose });
            RedrawTriggerData(data);
            yield return Panel_MutationChoose_Patch.Instance.Run(data);
            Method_OpenPanel.Invoke(Wnd_MutationMain.Instance, new object[] { g_emMutationPanel.Result });
            yield return instance.Run(data, selectResults[selectResults.Count - 1]);
        }

        private static void RedrawTriggerData(MutationData data)
        {
            var recordData = Field_RecordData.GetValue(MutationMgr.Instance) as MutataionRecordData;
            var type = data.TriggerTypes[0];
            var extentDef = MutationMgr.m_MutationExtentDefLoader.GetDef(data.Extent);
            var typeDef = MutationMgr.m_MutationTypeDefLoader.GetDef(data.Type);
            recordData.AddRecord(type, extentDef, typeDef);
            int totalTriggerCount = recordData.GetTriggerRecord(type).TotalTriggerCount;
            int seed = World.Instance.GlobleSeed + (int)type * 3125 + totalTriggerCount;
            World.SetRander(GMathUtl.RandomType.emMutation, new GRandom((uint)seed));
            var newData = Method_RandomMutationData.Invoke(MutationMgr.Instance, new object[]
            {
                    type,
                    data.Extent,
                    data.Type
            }) as MutationData;
            newData.Desc = data.Desc;
            World.SetRander(GMathUtl.RandomType.emMutation, null);
            CopyFieldValues(newData, data);
        }

        private static void CopyFieldValues<T>(T source,T target)
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                var sourceVal = field.GetValue(source);
                field.SetValue(target, sourceVal);
            }
        }
    }
}
