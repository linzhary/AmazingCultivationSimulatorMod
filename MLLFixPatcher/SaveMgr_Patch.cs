using HarmonyLib;
using ModLoaderLite.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using XiaWorld;

namespace MLLFixPatcher
{
    [HarmonyPatch(typeof(SaveMgr))]
    public class SaveMgr_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("DoSave")]
        public static IEnumerable<CodeInstruction> On_DoSave_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            for (int i = 0; i < codes.Count - 1; i++)
            {
                yield return codes[i];
            }
            yield return new CodeInstruction(OpCodes.Ldloc_0);
            yield return new CodeInstruction(OpCodes.Call, typeof(SaveMgr_Patch).GetMethod("On_DoSave_Execute", BindingFlags.Public | BindingFlags.Static));
            yield return codes.Last();
        }

        public static void On_DoSave_Execute(string saveName)
        {
            string text = Path.Combine(Directory.GetCurrentDirectory(), "saves\\" + saveName + ".mll");
            KLog.Dbg("[ModLoaderLite] saving mll save file " + text + "...", new object[0]);
            try
            {
                using (FileStream fileStream = File.OpenWrite(text))
                {
                    using (DeflateStream deflateStream = new DeflateStream(fileStream, CompressionMode.Compress))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(deflateStream))
                        {
                            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                            {
                                MLLMain_Patch.serializer.Serialize(jsonTextWriter, MLLMain_Patch.saves);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                KLog.Dbg("[ModLoaderLite] Failed to save. Reason: " + ex.Message, new object[0]);
                KLog.Dbg(ex.StackTrace, new object[0]);
            }
            finally
            {
                MLLMain_Patch.saves.Clear();
            }
            MLLMain_Patch.saves = null;
            MLLMain_Patch.serializer = null;
        }
    }
}
