using System;
using HarmonyLib;
using ModLoaderLite.AssetBundles;
using UnityEngine;
using XiaWorld;

namespace SuperJianZhen
{
	[HarmonyPatch(typeof(JianzhuViewHelper))]
	internal static class JianzhuViewHelperHook
	{
		[HarmonyPrefix]
		[HarmonyPatch("OpenZhen")]
		private static void OpenZhenPrefix(JianzhuViewHelper __instance)
		{
			if (JianZhenHook.Enabled)
			{
				if (JianzhuViewHelperHook.effect == null)
				{
					BuildingThing buildingThing = __instance.thing as BuildingThing;
					GameObject prefabFromAB = BundleManager.GetPrefabFromAB("superjianzhen", "JianZhenEffect");
					if (prefabFromAB != null)
					{
						JianzhuViewHelperHook.effect = UnityEngine.Object.Instantiate<GameObject>(prefabFromAB, buildingThing.View.transform);
					}
				}
				GameObject gameObject = JianzhuViewHelperHook.effect;
				if (gameObject == null)
				{
					return;
				}
				gameObject.SetActive(true);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("CloseZhen")]
		private static void CloseZhenPrefix()
		{
			GameObject gameObject = JianzhuViewHelperHook.effect;
			if (gameObject == null)
			{
				return;
			}
			gameObject.SetActive(false);
		}

		// Token: 0x04000002 RID: 2
		private static GameObject effect;
	}
}
