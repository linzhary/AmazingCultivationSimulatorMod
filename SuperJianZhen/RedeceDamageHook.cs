using System;
using HarmonyLib;
using UnityEngine;
using XiaWorld;

namespace SuperJianZhen
{
	[HarmonyPatch(typeof(Thing), "RedeceDamage")]
	internal static class RedeceDamageHook
	{
		private static void Prefix(Thing __instance, DamageInfo damageinfo, ref float damage, ref float v)
		{
			if (JianZhenHook.Enabled && damageinfo.source != g_emDamageSource.Thunder)
			{
				BuildingThing buildingThing = ThingMgr.Instance.FindThingByID(SuperJianZhen.JianzhenID) as BuildingThing;
				if (buildingThing != null)
				{
					BuildingHelperZhen buildingHelperZhen = buildingThing.Helper as BuildingHelperZhen;
					if (buildingHelperZhen != null && buildingHelperZhen.State == BuildingHelperZhen.ZhenState.Open)
					{
						Npc npc = ThingMgr.Instance.FindThingByID(SuperJianZhen.JianzhenCore) as Npc;
						if (npc != null && npc.Camp == __instance.Camp)
						{
							if (__instance.ThingType == g_emThingType.Npc)
							{
								EffectPool.Instance.BindEffect(90009, __instance as Npc, "Root", 0.5f, false);
							}
							else
							{
								GameObject effect = EffectPool.Instance.GetEffect(90009, new Vector3?(GridMgr.Inst.Grid2Pos(__instance.Key)), 0.5f, false);
								if (effect != null)
								{
									effect.AddComponent<EffectHelper>();
								}
							}
							RedeceDamageHook.ReduceDamage(buildingThing, ref v, damageinfo, __instance, npc);
							damage = v;
						}
					}
				}
			}
		}

		private static void Postfix(ref float v, ref bool __result)
		{
			if (v <= 0f)
			{
				__result = false;
			}
		}

		private static void ReduceDamage(BuildingThing zhen, ref float v, DamageInfo info, Thing target, Npc core)
		{
			v /= 1f + (float)zhen.Bag.m_lisItems.Count / 10f;
			core.CostLingFromDamage(v, info, ref v);
		}
	}
}
