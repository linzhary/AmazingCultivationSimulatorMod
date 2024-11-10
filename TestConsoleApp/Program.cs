using HarmonyLib;
using System;
using System.Reflection;

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
