using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using XiaWorld;

namespace BatterMutation
{

    [HarmonyPatch(typeof(Wnd_MutationMain))]
    public class Wnd_MutationMain_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OpenPanel")]
        public static bool On_OpenPanel_Prefix(g_emMutationPanel type, List<MutationSelectResult> ___SelectResults)
        {
            if (type is g_emMutationPanel.Result)
            {
                var selectResult = ___SelectResults.LastOrDefault();
                if (selectResult is MutationRedrawResult)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
