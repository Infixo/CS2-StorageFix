using System;
using Unity.Entities;
using Game;
using Game.Economy;
using Game.Simulation;
using HarmonyLib;

namespace StorageFix;

[HarmonyPatch]
class Patches
{
    [HarmonyPatch(typeof(Game.Common.SystemOrder), "Initialize")]
    [HarmonyPostfix]
    static void Initialize_Postfix(UpdateSystem updateSystem)
    {
        updateSystem.UpdateAt<StorageFix.TransportTrainAISystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<StorageFix.TransportTrainAISystem>(SystemUpdatePhase.LoadSimulation);
    }

    [HarmonyPatch(typeof(Game.Simulation.TransportTrainAISystem), "OnUpdate")]
    [HarmonyPrefix]
    static bool TransportTrainAISystem_OnUpdate()
    {
        return false; // don't execute the original system
    }

    [HarmonyPatch(typeof(Game.Economy.EconomyUtils), "AddResources", 
        new Type[] { typeof(Resource), typeof(int), typeof(DynamicBuffer<Resources>) }, 
        new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
    [HarmonyPrefix]
    static bool EconomyUtils_AddResources_Prefix(ref int __result, Resource resource, int amount, DynamicBuffer<Resources> resources)
    {
        if (resource == Resource.Money) return true; // don't track money
        //Plugin.Log($"AddRes {amount} of {resource}");
        for (int i = 0; i < resources.Length; i++)
        {
            Resources value = resources[i];
            if (value.m_Resource == resource)
            {
                value.m_Amount += amount;
                if (value.m_Amount < 0)
                    Plugin.LogStack("$AddRes WARNING: adding caused negative value in the buffer");
                resources[i] = value;
                __result = value.m_Amount;
                return false;
            }
        }
        if (amount < 0)
            Plugin.LogStack($"AddRes WARNING: adding a negative resource to the buffer");
        resources.Add(new Resources
        {
            m_Resource = resource,
            m_Amount = amount
        });
        __result = amount;
        return false;
    }
}
