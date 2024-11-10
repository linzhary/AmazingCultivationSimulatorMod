using System;
using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using ModLoaderLite;
using ModLoaderLite.Config;
using XiaWorld;
using XiaWorld.Fight;

namespace SuperJianZhen
{
	public static class SuperJianZhen
	{
		public static int JianzhenCore { get; set; }

		public static int JianzhenID { get; set; }

		public static void OnInit()
		{
			List<ThingUIActionBntBase> list = ThingUICommandDefine.sThingUICommands[g_emSelectThingSort.Building];
			bool flag = false;
			using (List<ThingUIActionBntBase>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Name == TFMgr.Get("全部归阵"))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				List<ThingUIActionBntBase> list2 = list;
				ThingUIActionBntBase thingUIActionBntBase = new ThingUIActionBntBase();
				thingUIActionBntBase.Name = TFMgr.Get("全部归阵");
				thingUIActionBntBase.Desc = TFMgr.Get("将地图上所有的12阶以下普通法宝全部归阵。不会影响已经装备好的法宝，也不会影响镇物，圣器和秘宝。");
				thingUIActionBntBase.Icon = "res/Sprs/ui/icon_lianbao01";
				thingUIActionBntBase.Camp = g_emFightCamp.Player;
				thingUIActionBntBase.IsVaild = delegate(Thing t, AreaBase a)
				{
					BuildingThing buildingThing = t as BuildingThing;
					return buildingThing.BuildingState == g_emBuildingState.Working && buildingThing.TagData.CheckTag("Zhen") > 0;
				};
				thingUIActionBntBase.Act = delegate(Thing t, AreaBase a)
				{
					SuperJianZhen.BatchBagFabao((BuildingThing)t);
				};
				list2.Add(thingUIActionBntBase);
				KLog.Log(KLogLevel.Debug, "[SuperJianZhen] added command!", new object[0]);
			}
		}

		public static void OnLoad()
		{
			SuperJianZhen.JianzhenCore = MLLMain.GetSaveOrDefault<int>("jnjly.SuperJianZhen.jianzhenCore");
			SuperJianZhen.JianzhenID = MLLMain.GetSaveOrDefault<int>("jnjly.SuperJianZhen.jianzhenID");
			Configuration.AddCheckBox("SuperJianZhen", "Enabled", "激活模组", true);
			Configuration.Subscribe(new EventCallback0(SuperJianZhen.HandleConfig));
		}

		public static void OnSave()
		{
			MLLMain.AddOrOverWriteSave("jnjly.SuperJianZhen.jianzhenCore", SuperJianZhen.JianzhenCore);
			MLLMain.AddOrOverWriteSave("jnjly.SuperJianZhen.jianzhenID", SuperJianZhen.JianzhenID);
		}

		private static void HandleConfig()
		{
			JianZhenHook.Enabled = Configuration.GetCheckBox("SuperJianZhen", "Enabled");
		}

		private static void BatchBagFabao(BuildingThing zhen)
		{
			foreach (ItemThing itemThing in from t in ThingMgr.Instance.GetThingList(g_emThingType.Item)
			select (ItemThing)t into i
			where i.AtG && i.IsFaBao && !i.IsMiBao && i.FSItemState <= 0 && i.Rate < 12
			select i)
			{
				if (itemThing.CheckCommandSingle("ZhenCarry", false) == null)
				{
					zhen.Bag.AddBegItem(itemThing, 1, "ZhenCarry");
				}
			}
		}
	}
}
