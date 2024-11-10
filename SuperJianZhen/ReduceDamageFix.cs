using System;
using HarmonyLib;
using XiaWorld;
using XiaWorld.Fight;

namespace SuperJianZhen
{
	[HarmonyPatch(typeof(Thing), "ReduceDamage", new Type[]
	{
		typeof(FightSkillDef),
		typeof(Npc),
		typeof(float),
		typeof(string),
		typeof(float),
		typeof(float)
	})]
	internal static class ReduceDamageFix
	{
		[HarmonyPriority(800)]
		private static void Prefix(FightSkillDef skill, ref float addv)
		{
			if (skill.NormalAtk > 0)
			{
				addv += 0.01f;
			}
		}
	}
}
