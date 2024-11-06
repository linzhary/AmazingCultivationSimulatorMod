using FairyGUI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace BatterMutation
{

    [HarmonyPatch(typeof(Wnd_MutationMain))]
    public class Wnd_MutationMain_Patch
    {
        protected static readonly Action<Wnd_MutationMain, g_emMutationPanel> OpenPanel;
        static Wnd_MutationMain_Patch()
        {
            var instanceExpr = Expression.Parameter(typeof(Wnd_MutationMain), "instance");
            {
                var methodInfo = typeof(Wnd_MutationMain).GetMethod("OpenPanel", BindingFlags.NonPublic | BindingFlags.Instance);
                var typeExpr = Expression.Parameter(typeof(g_emMutationPanel), "type");
                var methodCallExpr = Expression.Call(instanceExpr, methodInfo, typeExpr);
                OpenPanel = Expression.Lambda<Action<Wnd_MutationMain, g_emMutationPanel>>(methodCallExpr, instanceExpr, typeExpr).Compile();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("RunTianDaoTrigger")]
        public static bool OnPrefix(
            Wnd_MutationMain __instance,
            MutationData data,
            ref IEnumerator __result,
            Dictionary<g_emMutationPanel, MutationPanelBase> ___Panels,
            List<MutationSelectResult> ___SelectResults)
        {
            __result = GetEnumerator(__instance, data, ___Panels, ___SelectResults);
            return false;
        }

        public static IEnumerator GetEnumerator(
            Wnd_MutationMain instance,
            MutationData data,
            Dictionary<g_emMutationPanel, MutationPanelBase> Panels,
            List<MutationSelectResult> SelectResults)
        {
            instance.UIInfo.m_ShowLiuDong.selectedIndex = 1;
            OpenPanel(instance, g_emMutationPanel.Trigger);
            yield return Panels[g_emMutationPanel.Trigger].Run(data, new object[0]);
            MutationTypeDef type = MutationMgr.m_MutationTypeDefLoader.GetDef(data.Type);
            if (type.Type != g_emMutationType.Constant)
            {
                instance.UIInfo.m_ShowLiuDong.selectedIndex = 0;
            }
            OpenPanel(instance, g_emMutationPanel.Choose);
            yield return Panels[g_emMutationPanel.Choose].Run(data, new object[0]);
            instance.UIInfo.m_ShowLiuDong.selectedIndex = 1;

            var selectResult = SelectResults[SelectResults.Count - 1];
            if (selectResult is MutationReGenerateResult)
            {
                selectResult.Data = data;
                yield break;
            }
            OpenPanel(instance, g_emMutationPanel.Result);

            yield return Panels[g_emMutationPanel.Result].Run(data, new object[] { selectResult });
            yield break;
        }
    }
}
