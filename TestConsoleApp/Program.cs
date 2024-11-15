using BatterMutation;
using HarmonyLib;
using LingWall;
using System;
using System.Reflection;

namespace TestConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Assembly.LoadFrom("Assembly-CSharp.dll");
            Assembly.LoadFrom("Assembly-CSharp-firstpass.dll");
            var harmony = new Harmony("linzhary");
            harmony.PatchAll(typeof(BatterMutation.BatterMutation).Assembly);
            Console.ReadKey();
        }
    }
}
