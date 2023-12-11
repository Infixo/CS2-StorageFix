using System;
using System.Linq;
using System.Reflection;
using Colossal.Logging;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Game.Companies;
using Game.Simulation;
using Game.Prefabs;
using Unity.Mathematics;
using System.Diagnostics;

#if BEPINEX_V6
    using BepInEx.Unity.Mono;
#endif

namespace StorageFix;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger; // BepInEx logging
    private static ILog s_Log; // CO logging

    public static void Log(string text)
    {
        //string msg = GetCallingMethod(2) + ": " + text;
        Logger.LogInfo(text);
        s_Log.Info(text);
    }
    public static void LogStack(string text)
    {
        //string msg = GetCallingMethod(2) + ": " + text + " STACKTRACE";
        Logger.LogInfo(text + " STACKTRACE");
        s_Log.logStackTrace = true;
        s_Log.Info(text + "STACKTRACE");
        s_Log.logStackTrace = false;
    }

    /// <summary>
    /// Gets the method from the specified <paramref name="frame"/>.
    /// </summary>
    public static string GetCallingMethod(int frame)
    {
        StackTrace st = new StackTrace();
        MethodBase mb = st.GetFrame(frame).GetMethod(); // 0 - GetCallingMethod, 1 - Log, 2 - actual function calling a Log method
        return mb.DeclaringType + "." + mb.Name;
    }

    // mod settings
    //public static ConfigEntry<int> TeenAgeLimitInDays;
    //public static ConfigEntry<int> AdultAgeLimitInDays;
    //public static ConfigEntry<int> ElderAgeLimitInDays;
    //public static ConfigEntry<int> Education2InDays; // high school
    //public static ConfigEntry<int> Education3InDays; // college
    //public static ConfigEntry<int> Education4InDays; // university
    //public static ConfigEntry<float> GraduationLevel1; // elementary school
    //public static ConfigEntry<float> GraduationLevel2; // high school
    //public static ConfigEntry<float> GraduationLevel3; // college
    //public static ConfigEntry<float> GraduationLevel4; // university

    private void Awake()
    {
        Logger = base.Logger;

        // CO logging standard as described here https://cs2.paradoxwikis.com/Logging
        s_Log = LogManager.GetLogger(MyPluginInfo.PLUGIN_NAME);

        Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony");
        var patchedMethods = harmony.GetPatchedMethods().ToArray();

        Log($"Plugin {MyPluginInfo.PLUGIN_GUID} made patches! Patched methods: " + patchedMethods.Length);

        foreach (var patchedMethod in patchedMethods)
        {
            Log($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
        }
    }
}
