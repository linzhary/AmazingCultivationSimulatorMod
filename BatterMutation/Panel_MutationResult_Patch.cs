using HarmonyLib;
using System.Reflection;
using XiaWorld;

namespace BatterMutation
{
    [HarmonyPatch(typeof(Panel_MutationResult))]
    public class Panel_MutationResult_Patch
    {
        protected static readonly MethodInfo _method_OpenPanel = typeof(Wnd_MutationMain).GetMethod("OpenPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        [HarmonyPrefix]
        [HarmonyPatch("Run")]
        public static bool On_Run_Prefix(MutationData data, object[] objs)
        {
            if (data == null) return true;
            var realResult = objs.SafeGet(0) as MutationRedrawResult;
            if (realResult != null)
            {
                _method_OpenPanel.Invoke(Wnd_MutationMain.Instance, new object[] { g_emMutationPanel.Choose });
                realResult.Data = data;
                return false;
            }
            return true;
        }
    }
}
