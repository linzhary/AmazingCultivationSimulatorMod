using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MLLFixPatcher
{
    // Token: 0x02000006 RID: 6
    internal static class Util
    {
        // Token: 0x06000013 RID: 19 RVA: 0x00002AEC File Offset: 0x00000CEC
        public static Assembly PreLoadAssembly(string file)
        {
            string fileName = Path.GetFileName(file);
            if (!(fileName.ToLower() == "0harmony") && !fileName.ToLower().Contains("modloaderlite"))
            {
                try
                {
                    KLog.Dbg("Pre-Loading: " + fileName, new object[0]);
                    return Assembly.ReflectionOnlyLoadFrom(file);
                }
                catch (Exception ex)
                {
                    KLog.Dbg("Pre-Loading assembly " + fileName + " failed!", new object[0]);
                    KLog.Dbg(ex.Message, new object[0]);
                    KLog.Dbg(ex.StackTrace, new object[0]);
                }
            }
            return null;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002B98 File Offset: 0x00000D98
        public static Assembly LoadAssembly(Assembly asm)
        {
            if (asm != null)
            {
                try
                {
                    KLog.Dbg("Loading: " + asm.FullName, new object[0]);
                    return Assembly.LoadFrom(asm.Location);
                }
                catch (Exception ex)
                {
                    KLog.Dbg(string.Format("loading assembly {0} failed!", asm.GetName()), new object[0]);
                    KLog.Dbg(ex.Message, new object[0]);
                    KLog.Dbg(ex.StackTrace, new object[0]);
                }
            }
            return null;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002C24 File Offset: 0x00000E24
        public static bool ApplyHarmony(Assembly asm, string name)
        {
            if (asm != null)
            {
                string text = string.IsNullOrEmpty(name) ? asm.FullName : name;
                try
                {
                    KLog.Dbg("Applying harmony patch: " + text, new object[0]);
                    Harmony harmony = new Harmony(text);
                    if (harmony != null)
                    {
                        harmony.PatchAll(asm);
                    }
                    KLog.Dbg("Applying patch " + text + " succeeded!", new object[0]);
                    return true;
                }
                catch (Exception ex)
                {
                    KLog.Dbg("Patching harmony mod " + text + " failed!", new object[0]);
                    KLog.Dbg(ex.Message, new object[0]);
                    KLog.Dbg(ex.StackTrace, new object[0]);
                }
                return false;
            }
            return false;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002CE4 File Offset: 0x00000EE4
        public static void Call(Assembly asm, string method)
        {
            if (asm != null)
            {
                try
                {
                    string name = asm.GetName().Name;
                    KLog.Dbg(string.Concat(new string[]
                    {
                        "[ModLoaderLite] calling the ",
                        method,
                        " method for ",
                        name,
                        "..."
                    }), new object[0]);
                    Type type = asm.GetType(name + "." + name);
                    if (type != null)
                    {
                        MethodInfo method2 = type.GetMethod(method);
                        if (method2 != null)
                        {
                            method2.Invoke(null, null);
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    KLog.Dbg(ex.Message, new object[0]);
                }
                catch (TargetInvocationException ex2)
                {
                    KLog.Dbg(string.Concat(new string[]
                    {
                        "invocation of ",
                        method,
                        " in ",
                        asm.FullName,
                        " failed!"
                    }), new object[0]);
                    Exception innerException = ex2.InnerException;
                    KLog.Dbg(innerException.Message, new object[0]);
                    KLog.Dbg(innerException.StackTrace, new object[0]);
                }
                catch (Exception ex3)
                {
                    KLog.Dbg(ex3.Message, new object[0]);
                    KLog.Dbg(ex3.StackTrace, new object[0]);
                }
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002E34 File Offset: 0x00001034
        public static List<string> GetModFiles(string localpath, string modpath, string pattern)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(modpath))
            {
                using (IEnumerator<string> enumerator = (from pd in ModsMgr.Instance.GetPath(localpath, true, true)
                                                         where pd.mod != null
                                                         select pd.path).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string text = enumerator.Current;
                        try
                        {
                            list.AddRange(Directory.GetFiles(text, pattern, SearchOption.AllDirectories));
                        }
                        catch (Exception ex)
                        {
                            KLog.Dbg("Unable to get files in path " + text + ", ignoring the mod!", new object[0]);
                            KLog.Dbg("the error is: " + ex.Message, new object[0]);
                        }
                    }
                    return list;
                }
            }
            try
            {
                string path = Path.Combine(modpath, localpath);
                list.AddRange(Directory.GetFiles(path, pattern, SearchOption.AllDirectories));
            }
            catch (Exception ex2)
            {
                KLog.Dbg(string.Concat(new string[]
                {
                    "Unable to get files in ",
                    localpath,
                    " of ",
                    modpath,
                    ", check your directory name parameters!"
                }), new object[0]);
                KLog.Dbg("the error is: " + ex2.Message, new object[0]);
            }
            return list;
        }
    }
}
