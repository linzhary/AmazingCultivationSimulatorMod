using HarmonyLib;
using ModLoaderLite.Config;
using ModLoaderLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;

namespace BatterMutation
{
    [HarmonyPatch(typeof(MutationMgr))]
    public class MutationMgr_Partch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnGameStart")]
        public static void On_OnGameStart_Prefix()
        {
            var hasSetting = MLLMain.GetSaveOrDefault<bool>("Linzhary.BatterMutation.HasSetting");
            if (!hasSetting)
            {
                MainManager.Instance.Pause(true);
                Wnd_Message.Show(TFMgr.Get("开启无限重抽功能吗？\n本MOD预设重抽次数为三次，开启该功能后为无限\n后续也可以通过MLL设置修改本设置"), 2, (string s0) =>
                {
                    Panel_MutationChoose_Patch.LimitEnabled = s0 != "1";
                    MainManager.Instance.Play(0, true);
                }, true, "天道异动优化设置", 0, 0, string.Empty);
            }
        }
    }
}
