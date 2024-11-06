using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using XiaWorld;

namespace BatterMutation
{
    [HarmonyPatch(typeof(MutationMgr))]
    public class MutationMgr_Patch
    {
        private delegate void AddTriggerDataDelegate(MutationMgr mgr, g_emMutationTriggerType triggerType, MutationExtentDef extentDef, MutationTypeDef typeDef, string str);
        private static readonly AddTriggerDataDelegate AddTriggerData;
        static MutationMgr_Patch()
        {
            var instanceExpr = Expression.Parameter(typeof(MutationMgr), "instance");
            {
                var methodInfo = typeof(MutationMgr).GetMethod("AddTriggerData", BindingFlags.NonPublic | BindingFlags.Instance);
                var typeExpr = Expression.Parameter(typeof(g_emMutationTriggerType), "type");
                var extentDefExpr = Expression.Parameter(typeof(MutationExtentDef), "extentDef");
                var typeDefExpr = Expression.Parameter(typeof(MutationTypeDef), "typeDef");
                var descExpr = Expression.Parameter(typeof(string), "desc");
                var methodCallExpr = Expression.Call(instanceExpr, methodInfo, typeExpr, extentDefExpr, typeDefExpr, descExpr);
                AddTriggerData = Expression.Lambda<AddTriggerDataDelegate>(methodCallExpr, instanceExpr, typeExpr, extentDefExpr, typeDefExpr, descExpr).Compile();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MutationMgr.SelectCard))]
        public static void OnPrefix(ref List<MutationSelectResult> selectResult)
        {
            var newSelectResult = new List<MutationSelectResult>();
            KLog.Dbg($"{nameof(selectResult)},Length {selectResult.SafeLength()}");
            foreach (var result in selectResult)
            {
                if (result is MutationReGenerateResult
                    && result.Data.TriggerTypes.SafeLength() > 0)
                {
                    KLog.Dbg($"重新抽卡");
                    MutationMgr.Instance.m_TriggerPreparePhaseDatas.Remove(result.Data);
                    AddTriggerData(
                        MutationMgr.Instance,
                        result.Data.TriggerTypes[0],
                        MutationMgr.m_MutationExtentDefLoader.GetDef(result.Data.Extent),
                        MutationMgr.m_MutationTypeDefLoader.GetDef(result.Data.Type),
                        result.Data.Desc
                        );
                }
                else
                {
                    KLog.Dbg($"不抽卡了");
                    newSelectResult.Add(result);
                }
            }
            selectResult = newSelectResult;
        }
    }
}
