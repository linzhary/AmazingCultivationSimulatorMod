using System;

namespace LingLock
{
	// Token: 0x02000004 RID: 4
	[Serializable]
	public class LockItem
	{
		// Token: 0x0600000A RID: 10 RVA: 0x000024D4 File Offset: 0x000006D4
		public LockItem(int key, LockState state = LockState.Lock)
		{
			this.Key = key;
			this.State = state;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000B RID: 11 RVA: 0x000024F1 File Offset: 0x000006F1
		// (set) Token: 0x0600000C RID: 12 RVA: 0x000024F9 File Offset: 0x000006F9
		public int Key { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000D RID: 13 RVA: 0x00002502 File Offset: 0x00000702
		// (set) Token: 0x0600000E RID: 14 RVA: 0x0000250A File Offset: 0x0000070A
		public LockState State { get; set; } = LockState.Lock;
	}
}
