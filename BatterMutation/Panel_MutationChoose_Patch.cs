using FairyGUI;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace BatterMutation
{
    [HarmonyPatch(typeof(Panel_MutationChoose))]
    public class Panel_MutationChoose_Patch
    {
        protected static readonly FieldInfo _field_Result = typeof(Panel_MutationChoose).GetField("Result", BindingFlags.NonPublic | BindingFlags.Instance);
        protected static UI_MutationConfirmBtn _cardRedrawBtn;
        protected static UI_MutationConfirmBtn _groupRedrawBtn;

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(UI_MutationChoosePanel), typeof(Action<MutationSelectResult>) })]
        public static void On_Constructor_Prefix(
            Panel_MutationChoose __instance,
            UI_MutationChoosePanel uiInfo,
            Action<MutationSelectResult> onFillResult
            )
        {
            InitCardRedrawBtn(__instance, uiInfo, onFillResult);
            InitGroupRedrawBtn(__instance, uiInfo, onFillResult);
        }

        private static void InitCardRedrawBtn(Panel_MutationChoose instance,  UI_MutationChoosePanel uiInfo, Action<MutationSelectResult> onFillResult)
        {
            _cardRedrawBtn = UI_MutationConfirmBtn.CreateInstance();
            uiInfo.AddChild(_cardRedrawBtn);
            _cardRedrawBtn.SetScale(uiInfo.m_Confirm.scaleX, uiInfo.m_Confirm.scaleY);
            _cardRedrawBtn.SetPivot(uiInfo.m_Confirm.pivotX, uiInfo.m_Confirm.pivotY, uiInfo.m_Confirm.pivotAsAnchor);
            _cardRedrawBtn.SetSize(uiInfo.m_Confirm.width, uiInfo.m_Confirm.height);
            _cardRedrawBtn.onClick.Set(new EventCallback0(() =>
            {
                var result = new MutationRedrawResult();
                _field_Result.SetValue(instance, result);
                onFillResult?.Invoke(result);
            }));
            _cardRedrawBtn.visible = false;
            _cardRedrawBtn.alpha = 0f;
            _cardRedrawBtn.grayed = false;
            _cardRedrawBtn.text = "重抽";
        }
        private static void InitGroupRedrawBtn(Panel_MutationChoose instance, UI_MutationChoosePanel uiInfo, Action<MutationSelectResult> onFillResult)
        {
            _groupRedrawBtn = UI_MutationConfirmBtn.CreateInstance();
            uiInfo.AddChild(_groupRedrawBtn);
            _groupRedrawBtn.SetScale(uiInfo.m_ConfirmGroup.scaleX, uiInfo.m_ConfirmGroup.scaleY);
            _groupRedrawBtn.SetPivot(uiInfo.m_ConfirmGroup.pivotX, uiInfo.m_ConfirmGroup.pivotY, uiInfo.m_ConfirmGroup.pivotAsAnchor);
            _groupRedrawBtn.SetSize(uiInfo.m_ConfirmGroup.width, uiInfo.m_ConfirmGroup.height);
            _groupRedrawBtn.onClick.Set(new EventCallback0(() =>
            {
                var result = new MutationRedrawResult();
                _field_Result.SetValue(instance, result);
                onFillResult?.Invoke(result);
            }));
            _groupRedrawBtn.visible = false;
            _groupRedrawBtn.alpha = 0f;
            _groupRedrawBtn.grayed = false;
            _groupRedrawBtn.text = "重抽";
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
        [HarmonyPatch("OnClickRandom")]
        public static void On_OnClickRandom_Prefix()
        {
            if (_groupRedrawBtn != null)
            {
                _groupRedrawBtn.visible = false;
                _groupRedrawBtn.alpha = 0f;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch("WaitCardSelect")]
        public static IEnumerable<CodeInstruction> On_WaitCardSelect_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var constructorInfo = instructions.ElementAt(0).operand as ConstructorInfo;
            HarmonyUtils.Instance.Patch(
                constructorInfo.DeclaringType.GetMethod("MoveNext"),
                transpiler: new HarmonyMethod(typeof(Panel_MutationChoose_Patch), nameof(Apply_WaitCardSelect_Patch))
                );
            return instructions;
        }


        [HarmonyTranspiler]
        [HarmonyPatch("WaitGroupSelect")]
        public static IEnumerable<CodeInstruction> On_WaitGroupSelect_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var constructorInfo = instructions.ElementAt(0).operand as ConstructorInfo;
            HarmonyUtils.Instance.Patch(
                constructorInfo.DeclaringType.GetMethod("MoveNext"),
                transpiler: new HarmonyMethod(typeof(Panel_MutationChoose_Patch), nameof(Apply_WaitGroupSelect_Patch))
                );
            return instructions;
        }

        public static IEnumerable<CodeInstruction> Apply_WaitCardSelect_Patch(IEnumerable<CodeInstruction> instructions)
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

        public static IEnumerable<CodeInstruction> Apply_WaitGroupSelect_Patch(IEnumerable<CodeInstruction> instructions)
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
                    new CodeInstruction(OpCodes.Call, typeof(Panel_MutationChoose_Patch).GetMethod("ShowGroupRedrawBtn", BindingFlags.Static | BindingFlags.Public)),
                });
            }

            return codes;
        }

        public static void ShowCardRedrawBtn()
        {
            var UIInfo = Wnd_MutationMain.Instance.UIInfo.m_n8;
            if (_cardRedrawBtn != null)
            {
                _cardRedrawBtn.SetXY(UIInfo.m_Confirm.x, UIInfo.m_Confirm.y + UIInfo.m_Confirm.height / 1.5f);
                _cardRedrawBtn.visible = true;
                _cardRedrawBtn.alpha = 1f;
            }
        }
        public static void ShowGroupRedrawBtn()
        {
            var UIInfo = Wnd_MutationMain.Instance.UIInfo.m_n8;
            if (_groupRedrawBtn != null)
            {
                _groupRedrawBtn.SetXY(UIInfo.m_ConfirmGroup.x, UIInfo.m_ConfirmGroup.y + UIInfo.m_ConfirmGroup.height);
                _groupRedrawBtn.visible = true;
                _groupRedrawBtn.alpha = 1f;
            }
        }
    }
}
