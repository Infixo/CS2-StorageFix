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

namespace MonitorMod;

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

        //LogWorkplaceStructure();
    }
}
/*
    private void LogWorkplaceStructure()
    {
        int maxWorkers = 1000;
        for (int complexity = 0; complexity < Enum.GetNames(typeof(WorkplaceComplexity)).Length; complexity++)
        {
            Log.Info($"Workplace structure for complexity: {complexity} ({(WorkplaceComplexity)complexity})");
            for (int level = 1; level <= 5; level++)
            {
                Workplaces workplaces = Plugin.CalculateNumberOfWorkplaces(maxWorkers, (WorkplaceComplexity)complexity, level);
                Log.Info($"Level {level}, total {workplaces.count}: {workplaces.m_Uneducated} {workplaces.m_PoorlyEducated} {workplaces.m_Educated} {workplaces.m_WellEducated} {workplaces.m_HighlyEducated}");
            }
        }
    }

    
    public static Workplaces CalculateNumberOfWorkplaces(int totalWorkers, WorkplaceComplexity complexity, int buildingLevel)
    {
        Workplaces result = default(Workplaces);
        int num = 4 * (int)complexity + buildingLevel - 1;
        int num2 = totalWorkers;
        int num3 = 0;
        for (int i = 0; i < 5; i++)
        {
            int num4 = math.max(0, 8 - math.abs(num - 4 * i));
            if (i == 0)
            {
                num4 += math.max(0, 8 - math.abs(num + 4));
            }
            if (i == 4)
            {
                num4 += math.max(0, 8 - math.abs(num - 20));
            }
            int num5 = totalWorkers * num4 / 16;
            int num6 = totalWorkers * num4 % 16;
            if (num2 > num5 && num6 + num3 > 0)
            {
                num5++;
                num3 -= 16;
            }
            num3 += num6;
            num5 = math.min(num5, num2);
            num2 -= num5;
            result[i] = num5;
        }
        return result;
    }
*/

