using BatterMutation;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using XiaWorld;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Assembly.LoadFrom("Assembly-CSharp.dll");
            var harmony = new Harmony("linzhary");
            //harmony.PatchAll(typeof(HarmonyUtils).Assembly);
            Console.ReadKey();
        }
    }
}
