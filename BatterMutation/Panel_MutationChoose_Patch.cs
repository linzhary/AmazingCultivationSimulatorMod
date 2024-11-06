using FairyGUI;
using HarmonyLib;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace BatterMutation
{
    [HarmonyPatch(typeof(Panel_MutationChoose))]
    public class Panel_MutationChoose_Patch
    {
        protected static readonly FieldInfo Field_Result = typeof(Panel_MutationChoose).GetField("Result", BindingFlags.NonPublic | BindingFlags.Instance);
        protected static readonly Action<Panel_MutationChoose> UpdateConfirmBtn;
        protected static readonly Func<Panel_MutationChoose, Transition, IEnumerator> WaitAnim;

        static Panel_MutationChoose_Patch()
        {
            var instanceExpr = Expression.Parameter(typeof(Panel_MutationChoose), "instance");
            {
                var methodInfo = typeof(Panel_MutationChoose).GetMethod("UpdateConfirmBtn", BindingFlags.NonPublic | BindingFlags.Instance);
                var methodCallExpr = Expression.Call(instanceExpr, methodInfo);
                UpdateConfirmBtn = Expression.Lambda<Action<Panel_MutationChoose>>(methodCallExpr, instanceExpr).Compile();
            }
            {
                var methodInfo = typeof(Panel_MutationChoose).GetMethod("WaitAnim", BindingFlags.NonPublic | BindingFlags.Instance);
                var transition = Expression.Parameter(typeof(Transition), "transition");
                var methodCallExpr = Expression.Call(instanceExpr, methodInfo, transition);
                WaitAnim = Expression.Lambda<Func<Panel_MutationChoose, Transition, IEnumerator>>(methodCallExpr, instanceExpr, transition).Compile();
            }
        }


        private static UI_MutationConfirmBtn _cardRedrawBtn;
        private static UI_MutationConfirmBtn _groupRedrawBtn;

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(UI_MutationChoosePanel), typeof(Action<MutationSelectResult>) })]
        public static void On_Constructor_Prefix(
            Panel_MutationChoose __instance,
            UI_MutationChoosePanel uiInfo,
            Action<MutationSelectResult> onFillResult
            )
        {
            {
                _cardRedrawBtn = UI_MutationConfirmBtn.CreateInstance();
                uiInfo.AddChild(_cardRedrawBtn);
                _cardRedrawBtn.SetScale(uiInfo.m_Confirm.scaleX, uiInfo.m_Confirm.scaleY);
                _cardRedrawBtn.SetPivot(uiInfo.m_Confirm.pivotX, uiInfo.m_Confirm.pivotY, uiInfo.m_Confirm.pivotAsAnchor);
                _cardRedrawBtn.SetSize(uiInfo.m_Confirm.width, uiInfo.m_Confirm.height);
                _cardRedrawBtn.onClick.Set(new EventCallback0(() =>
                {
                    var result = new MutationReGenerateResult();
                    Field_Result.SetValue(__instance, result);
                    onFillResult?.Invoke(result);
                }));
                _cardRedrawBtn.visible = false;
                _cardRedrawBtn.alpha = 0f;
                _cardRedrawBtn.grayed = false;
                _cardRedrawBtn.text = "重抽";
            }
            {
                _groupRedrawBtn = UI_MutationConfirmBtn.CreateInstance();
                uiInfo.AddChild(_groupRedrawBtn);
                _groupRedrawBtn.SetScale(uiInfo.m_ConfirmGroup.scaleX, uiInfo.m_ConfirmGroup.scaleY);
                _groupRedrawBtn.SetPivot(uiInfo.m_ConfirmGroup.pivotX, uiInfo.m_ConfirmGroup.pivotY, uiInfo.m_ConfirmGroup.pivotAsAnchor);
                _groupRedrawBtn.SetSize(uiInfo.m_ConfirmGroup.width, uiInfo.m_ConfirmGroup.height);
                _groupRedrawBtn.onClick.Set(new EventCallback0(() =>
                {
                    var result = new MutationReGenerateResult();
                    Field_Result.SetValue(__instance, result);
                    onFillResult?.Invoke(result);
                }));
                _groupRedrawBtn.visible = false;
                _groupRedrawBtn.alpha = 0f;
                _groupRedrawBtn.grayed = false;
                _groupRedrawBtn.text = "重抽";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetAnimUI")]
        public static void On_ResetAnimUI_Postfix()
        {
            if (_cardRedrawBtn != null)
            {
                _cardRedrawBtn.visible = false;
                _cardRedrawBtn.alpha = 0f;
            }
            if (_groupRedrawBtn != null)
            {
                _groupRedrawBtn.visible = false;
                _groupRedrawBtn.alpha = 0f;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("WaitCardSelect")]
        public static bool On_WaitCardSelect_Prefix(
            Panel_MutationChoose __instance,
            ref IEnumerator __result,
            MutationData ___Data,
            g_emMutationExtent ___Extent,
            UI_MutationChoosePanel ___UIInfo
            )
        {
            __result = WaitCardSelect(
                __instance,
                ___Data,
                ___Extent,
                ___UIInfo);
            return false;
        }

        private static IEnumerator WaitCardSelect(
            Panel_MutationChoose instance,
            MutationData Data,
            g_emMutationExtent Extent,
            UI_MutationChoosePanel UIInfo)
        {
            UIInfo.m_pageController.selectedIndex = 0;
            UIInfo.m_n27.RemoveChildrenToPool();
            var constantCards = Data.ConstantCards;
            for (int i = 0; i < constantCards.Count; i++)
            {
                string text = constantCards[i];
                var def = MutationMgr.m_MutationConstantCardDefLoader.GetDef(text);
                var ui_MutationChooseBtn = UIInfo.m_n27.AddItemFromPool() as UI_MutationChooseBtn;
                ui_MutationChooseBtn.m_n2.text = def.DisplayName;
                ui_MutationChooseBtn.m_n3.text = def.Desc;
                ui_MutationChooseBtn.m_extentColorController.selectedIndex = Mathf.Clamp(Extent - g_emMutationExtent.Shallow, 0, 2);
                ui_MutationChooseBtn.data = text;
            }
            UpdateConfirmBtn(instance);
            yield return WaitAnim(instance, UIInfo.m_t0);
            if (_cardRedrawBtn != null)
            {
                _cardRedrawBtn.SetXY(UIInfo.m_Confirm.x, UIInfo.m_Confirm.y + UIInfo.m_Confirm.height / 1.5f);
                _cardRedrawBtn.visible = true;
                _cardRedrawBtn.alpha = 1f;
            }
            yield return new WaitUntil(() => Field_Result.GetValue(instance) != null);
            yield break;
        }

        [HarmonyPrefix]
        [HarmonyPatch("WaitGroupSelect")]
        public static bool On_WaitGroupSelect_Prefix(
            Panel_MutationChoose __instance,
            ref IEnumerator __result,
            MutationData ___Data,
            UI_MutationChoosePanel ___UIInfo,
            g_emMutationType ___MutationType
            )
        {
            __result = WaitGroupSelect(
                __instance,
                ___Data,
                ___UIInfo,
                ___MutationType);
            return false;
        }

        private static IEnumerator WaitGroupSelect(
            Panel_MutationChoose instance,
            MutationData Data,
            UI_MutationChoosePanel UIInfo,
            g_emMutationType MutationType)
        {

            UIInfo.m_pageController.selectedIndex = 1;
            UIInfo.m_n25.RemoveChildrenToPool();
            UIInfo.m_n26.RemoveChildrenToPool();
            if (MutationType == g_emMutationType.Unstable)
            {
                for (int i = 0; i < Data.Conditions.Count; i++)
                {
                    string text = Data.Conditions[i];
                    MutationConditionDef def = MutationMgr.m_MutationConditionDefLoader.GetDef(text);
                    UI_MutationChooseBtn2 uI_MutationChooseBtn = UIInfo.m_n25.AddItemFromPool() as UI_MutationChooseBtn2;
                    uI_MutationChooseBtn.m_typeController.selectedIndex = 0;
                    uI_MutationChooseBtn.m_ShowAnim.selectedIndex = 0;
                    uI_MutationChooseBtn.title = def.Desc;
                    uI_MutationChooseBtn.data = text;
                }

                for (int j = 0; j < Data.lstEffects[0].Effects.Count; j++)
                {
                    string text2 = Data.lstEffects[0].Effects[j];
                    UI_MutationChooseBtn2 uI_MutationChooseBtn2 = UIInfo.m_n26.AddItemFromPool() as UI_MutationChooseBtn2;
                    uI_MutationChooseBtn2.m_typeController.selectedIndex = 2;
                    uI_MutationChooseBtn2.m_ShowAnim.selectedIndex = 0;
                    MutationEffectDef def2 = MutationMgr.m_MutationEffectDefLoader.GetDef(text2);
                    uI_MutationChooseBtn2.m_n5.text = def2.Desc;
                    uI_MutationChooseBtn2.m_number.text = TFMgr.Get(TFMgr.Get("可生效{0}次"), def2.WorkCount);
                    uI_MutationChooseBtn2.data = text2;
                }
            }
            else
            {
                for (int k = 0; k < Data.lstEffects[0].Effects.Count; k++)
                {
                    string text3 = Data.lstEffects[0].Effects[k];
                    UI_MutationChooseBtn2 uI_MutationChooseBtn3 = UIInfo.m_n25.AddItemFromPool() as UI_MutationChooseBtn2;
                    uI_MutationChooseBtn3.m_typeController.selectedIndex = 1;
                    uI_MutationChooseBtn3.m_ShowAnim.selectedIndex = 0;
                    MutationEffectDef def3 = MutationMgr.m_MutationEffectDefLoader.GetDef(text3);
                    uI_MutationChooseBtn3.title = def3.Desc;
                    uI_MutationChooseBtn3.data = text3;
                }

                for (int l = 0; l < Data.lstEffects[1].Effects.Count; l++)
                {
                    string text4 = Data.lstEffects[1].Effects[l];
                    UI_MutationChooseBtn2 uI_MutationChooseBtn4 = UIInfo.m_n26.AddItemFromPool() as UI_MutationChooseBtn2;
                    uI_MutationChooseBtn4.m_typeController.selectedIndex = 3;
                    uI_MutationChooseBtn4.m_ShowAnim.selectedIndex = 0;
                    MutationEffectDef def4 = MutationMgr.m_MutationEffectDefLoader.GetDef(text4);
                    uI_MutationChooseBtn4.m_n5.text = def4.Desc;
                    uI_MutationChooseBtn4.data = text4;
                }
            }

            UpdateConfirmBtn(instance);
            yield return WaitAnim(instance, UIInfo.m_t0);
            bool hasTaiJi = MutationMgr.Instance.IsTriggerChosen(g_emMutationTriggerType.TaiJi);
            if (MutationMgr.Instance.IsTriggerChosen(g_emMutationTriggerType.TaiXuan))
            {
                string tipTxt = TFMgr.Get("在太玄天道异动的影响下，本次只能随机选择");
                if (hasTaiJi)
                {
                    tipTxt += TFMgr.Get("\n在太极天道的影响下，本次随机的范围变得更广了");
                }

                UIInfo.m_TianDoEffect.text = tipTxt;
                UIInfo.m_n38.m_DoMask.Play();
                yield return WaitAnim(instance, UIInfo.m_EnterRandom);
            }
            else if (hasTaiJi)
            {
                UIInfo.m_TianDoEffect.text = TFMgr.Get("在太极天道的影响下，本次随机的范围变得更广了");
                yield return WaitAnim(instance, UIInfo.m_EnterTaiji);
            }
            if (_groupRedrawBtn != null)
            {
                _groupRedrawBtn.SetXY(UIInfo.m_ConfirmGroup.x, UIInfo.m_ConfirmGroup.y + UIInfo.m_ConfirmGroup.height);
                _groupRedrawBtn.visible = true;
                _groupRedrawBtn.alpha = 1f;
            }
            yield return new WaitUntil(() => Field_Result.GetValue(instance) != null);
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnClickRandom")]
        public static void On_OnClickRandom_Prefix()
        {
            if (_groupRedrawBtn != null)
            {
                _groupRedrawBtn.visible = false;
                _groupRedrawBtn.alpha = 0f;
            }
        }
    }
}
