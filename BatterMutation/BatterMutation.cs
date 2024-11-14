﻿using FairyGUI;
using ModLoaderLite;
using ModLoaderLite.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;

namespace BatterMutation
{
    public static class BatterMutation
    {
        public static void OnLoad()
        {
            Configuration.AddCheckBox("BatterMutation", "Limit", "限制次数", true);
            Configuration.Subscribe(new EventCallback0(HandleConfig));
        }

        private static void HandleConfig()
        {
            Panel_MutationChoose_Patch.LimitEnabled = Configuration.GetCheckBox("BatterMutation", "Limit");
        }
    }
}
