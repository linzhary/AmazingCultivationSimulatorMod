using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using XiaWorld;

namespace BatterMutation
{
    [HarmonyPatch(typeof(Panel_MutationResult))]
    public class Panel_MutationResult_Patch
    {
        internal static readonly MethodInfo Method_OpenPanel = typeof(Wnd_MutationMain).GetMethod("OpenPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static readonly FieldInfo Field_SelectResults = typeof(Wnd_MutationMain).GetField("SelectResults", BindingFlags.NonPublic | BindingFlags.Instance);
        
        [HarmonyPrefix]
        [HarmonyPatch("Run")]
        public static bool On_Run_Prefix(MutationData data, object[] objs)
        {
            if (data == null) return true;
            if (objs.SafeGet(0) as MutationRedrawResult != null)
            {
                Method_OpenPanel.Invoke(Wnd_MutationMain.Instance, new object[] { g_emMutationPanel.Choose });
                var selectResults = Field_SelectResults.GetValue(Wnd_MutationMain.Instance) as List<MutationSelectResult>;
                selectResults.RemoveAt(selectResults.Count - 1);
                Panel_MutationChoose_Patch.Instance.Run(data);
                return false;
            }
            return true;
        }
    }
}
