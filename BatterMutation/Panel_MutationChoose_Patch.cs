using FairyGUI;
using HarmonyLib;
using Newtonsoft.Json;
using System;
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
        internal static readonly FieldInfo Field_Result = typeof(Panel_MutationChoose).GetField("Result", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly FieldInfo Field_Data = typeof(Panel_MutationChoose).GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static UI_MutationChoosePanel UIInfo;
        internal static UI_MutationConfirmBtn CardRedrawBtn;
        internal static UI_MutationConfirmBtn GroupRedrawBtn;
        internal static Panel_MutationChoose Instance;
        internal static MutationData Data;
        internal static g_emMutationType MutationType;
        internal static readonly Dictionary<g_emMutationType, int?> RedrawBtnUsedTimeMap = new Dictionary<g_emMutationType, int?>()
        {
            { g_emMutationType.None, 0 },
            { g_emMutationType.Constant, 0 },
            { g_emMutationType.Unstable, 0 },
            { g_emMutationType.YinYang, 0 }
        };

        public static bool LimitEnabled = true;

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(UI_MutationChoosePanel), typeof(Action<MutationSelectResult>) })]
        public static void On_Constructor_Prefix(
            Panel_MutationChoose __instance,
            UI_MutationChoosePanel uiInfo,
            ref Action<MutationSelectResult> onFillResult
            )
        {
            Instance = __instance;
            UIInfo = Wnd_MutationMain.Instance.UIInfo.m_n8;
            var sourceOnFillResult = onFillResult;
            onFillResult = result =>
            {
                if (!(result is MutationRedrawResult))
                {
                    RedrawBtnUsedTimeMap[MutationType] = 0;
                }
                sourceOnFillResult(result);
            };
            InitCardRedrawBtn(__instance, uiInfo, onFillResult);
            InitGroupRedrawBtn(__instance, uiInfo, onFillResult);
        }

        private static void InitCardRedrawBtn(Panel_MutationChoose instance, UI_MutationChoosePanel uiInfo, Action<MutationSelectResult> onFillResult)
        {
            CardRedrawBtn = UI_MutationConfirmBtn.CreateInstance();
            uiInfo.AddChild(CardRedrawBtn);
            CardRedrawBtn.SetScale(uiInfo.m_Confirm.scaleX, uiInfo.m_Confirm.scaleY);
            CardRedrawBtn.SetPivot(uiInfo.m_Confirm.pivotX, uiInfo.m_Confirm.pivotY, uiInfo.m_Confirm.pivotAsAnchor);
            CardRedrawBtn.SetSize(uiInfo.m_Confirm.width, uiInfo.m_Confirm.height);
            CardRedrawBtn.onClick.Set(new EventCallback0(() =>
            {
                if (CardRedrawBtn.grayed)
                {
                    Wnd_PublicTips.PopupToCheterSimple("你太黑啦，重抽也救不了你");
                }
                else
                {
                    if (LimitEnabled)
                    {
                        RedrawBtnUsedTimeMap[MutationType]++;
                    }
                    else
                    {
                        RedrawBtnUsedTimeMap[MutationType] = 0;
                    }
                    var result = new MutationRedrawResult();
                    Field_Result.SetValue(instance, result);
                    onFillResult?.Invoke(result);
                }
            }));
            CardRedrawBtn.visible = false;
            CardRedrawBtn.alpha = 0f;
            CardRedrawBtn.grayed = false;
        }
        private static void InitGroupRedrawBtn(Panel_MutationChoose instance, UI_MutationChoosePanel uiInfo, Action<MutationSelectResult> onFillResult)
        {
            GroupRedrawBtn = UI_MutationConfirmBtn.CreateInstance();
            uiInfo.AddChild(GroupRedrawBtn);
            GroupRedrawBtn.SetScale(uiInfo.m_ConfirmGroup.scaleX, uiInfo.m_ConfirmGroup.scaleY);
            GroupRedrawBtn.SetPivot(uiInfo.m_ConfirmGroup.pivotX, uiInfo.m_ConfirmGroup.pivotY, uiInfo.m_ConfirmGroup.pivotAsAnchor);
            GroupRedrawBtn.SetSize(uiInfo.m_ConfirmGroup.width, uiInfo.m_ConfirmGroup.height);
            GroupRedrawBtn.onClick.Set(new EventCallback0(() =>
            {
                if (GroupRedrawBtn.grayed)
                {
                    Wnd_PublicTips.PopupToCheterSimple("你太黑啦，重抽也救不了你");
                }
                else
                {
                    if (LimitEnabled)
                    {
                        RedrawBtnUsedTimeMap[MutationType]++;
                    }
                    else
                    {
                        RedrawBtnUsedTimeMap[MutationType] = 0;
                    }
                    var result = new MutationRedrawResult();
                    Field_Result.SetValue(instance, result);
                    onFillResult?.Invoke(result);
                }
            }));
            GroupRedrawBtn.visible = false;
            GroupRedrawBtn.alpha = 0f;
            GroupRedrawBtn.grayed = false;
        }


        [HarmonyPostfix]
        [HarmonyPatch("Run")]
        public static void On_Run_Prefix(MutationData data)
        {
            Data = data;
            MutationType = MutationMgr.Instance.GetMutationTypeEnumByName(data.Type);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetAnimUI")]
        public static void On_ResetAnimUI_Postfix()
        {
            if (CardRedrawBtn != null)
            {
                CardRedrawBtn.visible = false;
                CardRedrawBtn.alpha = 0f;
            }
            if (GroupRedrawBtn != null)
            {
                GroupRedrawBtn.visible = false;
                GroupRedrawBtn.alpha = 0f;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnClickRandom")]
        public static void On_OnClickRandom_Prefix()
        {
            if (GroupRedrawBtn != null)
            {
                GroupRedrawBtn.visible = false;
                GroupRedrawBtn.alpha = 0f;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch("WaitCardSelect", MethodType.Enumerator)]
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

        [HarmonyTranspiler]
        [HarmonyPatch("WaitGroupSelect", MethodType.Enumerator)]
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
            var usedTime = RedrawBtnUsedTimeMap[MutationType];
            if (CardRedrawBtn != null)
            {
                CardRedrawBtn.SetXY(UIInfo.m_Confirm.x, UIInfo.m_Confirm.y + UIInfo.m_Confirm.height / 1.5f);
                CardRedrawBtn.text = "重抽";
                if (LimitEnabled)
                {
                    CardRedrawBtn.text = $"重抽({3 - usedTime})";
                    CardRedrawBtn.grayed = usedTime == 3;
                }
                CardRedrawBtn.visible = true;
                CardRedrawBtn.alpha = 1f;
            }
        }
        public static void ShowGroupRedrawBtn()
        {
            var usedTime = RedrawBtnUsedTimeMap[MutationType];
            if (GroupRedrawBtn != null)
            {
                GroupRedrawBtn.SetXY(UIInfo.m_ConfirmGroup.x, UIInfo.m_ConfirmGroup.y + UIInfo.m_ConfirmGroup.height);
                GroupRedrawBtn.text = "重抽";
                if (LimitEnabled)
                {
                    GroupRedrawBtn.text = $"重抽({3 - usedTime})";
                    GroupRedrawBtn.grayed = usedTime == 3;
                }
                GroupRedrawBtn.visible = true;
                GroupRedrawBtn.alpha = 1f;
            }
        }
    }
}
