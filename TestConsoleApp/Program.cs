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
            var harmony = new Harmony("linzhary");
            harmony.PatchAll(typeof(LingMapData_Patch).Assembly);
            Console.ReadKey();
        }
    }
}
