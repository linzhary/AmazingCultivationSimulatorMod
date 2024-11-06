using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using XiaWorld;

namespace LingLock
{
	public class Agent
	{
		public void Enter()
		{
			EventMgr.Instance.RegisterEvent(g_emEvent.SelectBuilding, new EventMgr.EventHandler(this.SelectBuilding));
		}

		public void SelectBuilding(Thing thing, object[] objs)
		{
			if (thing == null)
			{
				return;
			}
			if (Agent._additionalBuildings.Contains(thing.def.Name))
			{
				this.SwitchLockButton(thing, this._lockArray.Any((LockItem r) => r.Key == thing.Key && r.State == LockState.Lock));
			}
		}

		public void SwitchLockButton(Thing thing, bool locked = true)
		{
			thing.RemoveBtnData("锁灵");
			thing.RemoveBtnData("解锁");
			if (locked)
			{
				thing.AddBtnData("解锁", "", "GameMain:GetMod('LingLock'):UnLock(bind)", "关闭锁灵效果", null);
				return;
			}
			thing.AddBtnData("锁灵", "", "GameMain:GetMod('LingLock'):Lock(bind)", "打开锁灵效果", null);
		}

		public void SwitchLockState(Thing thing, bool locked = true)
		{
			LockItem lockItem = this._lockArray.FirstOrDefault((LockItem r) => r.Key == thing.Key);
			if (lockItem == null)
			{
				this._lockArray.Add(new LockItem(thing.Key, locked ? LockState.Lock : LockState.PreUnLock));
			}
			else
			{
				lockItem.State = (locked ? LockState.Lock : LockState.PreUnLock);
			}
			this.SwitchLockButton(thing, locked);
		}

		public void Step()
		{
			World instance = World.Instance;
			Map map = instance?.map;
			if (map == null)
			{
				return;
			}
			foreach (LockItem lockItem in this._lockArray)
			{
				try
				{
					List<int> neighbor = GridMgr.Inst.GetNeighbor(lockItem.Key);
					float num = map.Effect.GetEffect(lockItem.Key, g_emMapEffectKind.LingAddion, 0f, true);
					GridManagerEx<LingMapData.LQData> gdm = map.GDM;
					LingMapData.LQData lqdata = gdm?.GetGridData(lockItem.Key, null);
					if (lockItem.State == LockState.Lock)
					{
						if (map.Things.GetThingAtGrid(lockItem.Key, g_emThingType.Building) != null)
						{
							float num2 = Math.Max((num - 2000f) / 200f, 0f);
							lqdata.Value = num * Math.Max(3f - num2, 2f) - 1f;
							using (List<int>.Enumerator enumerator2 = neighbor.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									int key = enumerator2.Current;
									GridManagerEx<LingMapData.LQData> gdm2 = map.GDM;
									LingMapData.LQData lqdata2 = gdm2?.GetGridData(key, null);
									if (lqdata2 != null)
									{
										lqdata2.Value = map.Effect.GetEffect(key, g_emMapEffectKind.LingAddion, 0f, true) + lqdata.Value * 2f / 3f;
									}
								}
								continue;
							}
						}
						lockItem.State = LockState.PreUnLock;
					}
					else if (lockItem.State == LockState.PreUnLock)
					{
						lqdata.Value = num - 1f;
						foreach (int key2 in neighbor)
						{
							GridManagerEx<LingMapData.LQData> gdm3 = map.GDM;
							LingMapData.LQData lqdata3 = gdm3?.GetGridData(key2, null);
							if (lqdata3 != null)
							{
								lqdata3.Value = map.Effect.GetEffect(key2, g_emMapEffectKind.LingAddion, 0f, true);
							}
						}
						lockItem.State = LockState.UnLock;
					}
				}
				catch
				{
				}
			}
		}

		public string Save()
		{
			KLog.Dbg("锁灵法座数据存档", new object[0]);
			return JsonConvert.SerializeObject(from r in this._lockArray
											   where r.State > LockState.UnLock
											   select r);
		}

		public void Load(string json)
		{
			KLog.Dbg("锁灵法座数据读档", new object[0]);
			if (!string.IsNullOrEmpty(json))
			{
				List<LockItem> collection = JsonConvert.DeserializeObject<List<LockItem>>(json);
				this._lockArray.AddRange(collection);
			}
		}

		private static readonly string[] _additionalBuildings = new string[]
		{
			"Building_MagicCushion",
			"Building_LianFaTan"
		};

		private readonly List<LockItem> _lockArray = new List<LockItem>();
	}
}
