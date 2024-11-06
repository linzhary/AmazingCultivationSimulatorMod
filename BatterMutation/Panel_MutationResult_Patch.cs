using HarmonyLib;
using XiaWorld;

namespace BatterMutation
{
    [HarmonyPatch(typeof(Panel_MutationResult))]
    public class Panel_MutationResult_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Run")]
        public static bool On_Run_Prefix(MutationData data, object[] objs)
        {
            if (objs.SafeGet(0) is MutationRedrawResult result)
            {
                result.Data = data;
                return false;
            }
            return true;
        }
    }
}
