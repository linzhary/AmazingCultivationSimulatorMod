using System;
using System.Collections.Generic;
using HarmonyLib;
using XiaWorld;

namespace SuperJianZhen
{
	[HarmonyPatch(typeof(BuildingHelperZhen))]
	internal static class JianZhenHook
	{
		[HarmonyPostfix]
		[HarmonyPatch("Open")]
		public static void OpenPostfix(BuildingHelperZhen __instance)
		{
			if (JianZhenHook.Enabled)
			{
				Wnd_SelectNpc.Instance.Select(delegate(List<int> npcs)
				{
					Npc npc = ThingMgr.Instance.FindThingByID(npcs[0]) as Npc;
					if (npc != null)
					{
						SuperJianZhen.JianzhenCore = npc.ID;
						SuperJianZhen.JianzhenID = __instance.building.ID;
					}
				}, g_emNpcRank.Disciple, 1, 1, null, null, null, null, null, false, null, false, g_emNpcRaceType.Wisdom, null, null, g_emNpcSex.None, false);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("OnStep")]
		public static bool OnStepPrefix(BuildingHelperZhen __instance, float dt)
		{
			return !JianZhenHook.Enabled;
		}

		// Token: 0x04000001 RID: 1
		public static bool Enabled = true;
	}
}
