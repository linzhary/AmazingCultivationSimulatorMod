using HarmonyLib;
using ModLoaderLite;
using ModLoaderLite.Config;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Reflection;
using FairyGUI;
using Newtonsoft.Json;

namespace MLLFixPatcher
{
    [HarmonyPatch(typeof(MLLMain))]
    public class MLLMain_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Load")]
        public static bool On_Load_Prefix(
            ref Dictionary<string, object> ___saves,
            JsonSerializer ___serializer,
             List<Assembly> ___assemblies
            )
        {
            KLog.Dbg("[ModLoaderLite] adding config menu...", new object[0]);
            var value = Traverse.Create(Wnd_GameMain.Instance).Field<PopupMenu>("MainMenu").Value;
            value?.AddItem("MLL设置", delegate ()
                {
                    Configuration.Show();
                });
            var path = Path.Combine(Directory.GetCurrentDirectory(), "saves\\" + GameWatch.Instance.LoadFile + ".mll");
            KLog.Dbg("[ModLoaderLite] loading mll save file " + path + "...", new object[0]);

            try
            {
                if (File.Exists(path))
                {
                    KLog.Dbg("[ModLoaderLite] deserializing mll save file...", new object[0]);

                    using (var fileStream = File.OpenRead(path))
                    {
                        using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
                        {
                            using (var streamReader = new StreamReader(deflateStream))
                            {
                                using (var jsonTextReader = new JsonTextReader(streamReader))
                                {
                                    ___saves = ___serializer.Deserialize<Dictionary<string, object>>(jsonTextReader);
                                }
                            }
                        }
                    }
                }
                KLog.Dbg("[ModLoaderLite] loading config...", new object[0]);
                Configuration.Load();
                KLog.Dbg("[ModLoaderLite] calling OnLoad methods for each mod...", new object[0]);
                foreach (Assembly asm in ___assemblies)
                {
                    Util.Call(asm, "OnLoad");
                }
            }
            catch (Exception ex)
            {
                KLog.Dbg("[ModLoaderLite] Failed to load. Reason: " + ex.Message, new object[0]);
                KLog.Dbg(ex.StackTrace, new object[0]);
            }
            finally
            {
                ___saves.Clear();
            }
            return false;
        }
    }
}
