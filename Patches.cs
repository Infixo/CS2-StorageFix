using System;
using Unity.Entities;
using Game;
using Game.Serialization;
using Game.Economy;
using HarmonyLib;

namespace MonitorMod;

[HarmonyPatch]
class MonitorModPatches
{
    [HarmonyPatch(typeof(Game.Common.SystemOrder), "Initialize")]
    [HarmonyPostfix]
    public static void Initialize_Postfix(UpdateSystem updateSystem)
    {
        //updateSystem.UpdateAt<MonitorMod.Systems.CarStorageTransferRequestSystem>(SystemUpdatePhase.GameSimulation);

        //updateSystem.UpdateAt<MonitorMod.Systems.DeliveryTruckAISystem>(SystemUpdatePhase.LoadSimulation);
        //updateSystem.UpdateAfter<MonitorMod.Systems.DeliveryTruckAISystem.Actions, MonitorMod.Systems.DeliveryTruckAISystem>(SystemUpdatePhase.LoadSimulation);

        updateSystem.UpdateAt<MonitorMod.Systems.StorageCompanySystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAfter<PostDeserialize<MonitorMod.Systems.StorageCompanySystem>>(SystemUpdatePhase.Deserialize); // PostDeserialize

        updateSystem.UpdateAt<MonitorMod.Systems.StorageTransferSystem>(SystemUpdatePhase.GameSimulation);

        updateSystem.UpdateAt<MonitorMod.Systems.TransportTrainAISystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<MonitorMod.Systems.TransportTrainAISystem>(SystemUpdatePhase.LoadSimulation);
    }
    /*
    [HarmonyPatch(typeof(Game.Simulation.CarStorageTransferRequestSystem), "OnUpdate")]
    [HarmonyPrefix]
    static bool CarStorageTransferRequestSystem_OnUpdate()
    {
        return false; // don't execute the original system
    }
    /*
    /*
    [HarmonyPatch(typeof(Game.Simulation.DeliveryTruckAISystem), "OnUpdate")]
    [HarmonyPrefix]
    static bool DeliveryTruckAISystem_OnUpdate()
    {
        return false; // don't execute the original system
    }

    [HarmonyPatch(typeof(Game.Simulation.DeliveryTruckAISystem.Actions), "OnUpdate")]
    [HarmonyPrefix]
    static bool DeliveryTruckAISystemActions_OnUpdate()
    {
        return false; // don't execute the original system
    }
    */
    [HarmonyPatch(typeof(Game.Simulation.StorageTransferSystem), "OnUpdate")]
    [HarmonyPrefix]
    static bool StorageTransferSystem_OnUpdate()
    {
        return false; // don't execute the original system
    }

    [HarmonyPatch(typeof(Game.Simulation.StorageCompanySystem), "OnUpdate")]
    [HarmonyPrefix]
    static bool StorageCompanySystem_OnUpdate()
    {
        return false; // don't execute the original system
    }

    [HarmonyPatch(typeof(Game.Simulation.StorageCompanySystem), "PostDeserialize")]
    [HarmonyPrefix]
    static bool StorageCompanySystem_PostDeserialize(Colossal.Serialization.Entities.Context context)
    {
        return false; // don't execute the original system
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
    public static bool EconomyUtils_AddResources_Prefix(ref int __result, Resource resource, int amount, DynamicBuffer<Resources> resources)
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

/*
    [HarmonyPrefix, HarmonyPatch(typeof(Game.Simulation.AgingSystem), "GetTeenAgeLimitInDays")]
    static bool GetTeenAgeLimitInDays_Prefix(ref int __result)
    {
        RealPop.Debug.Log("GetTeenAgeLimitInDays_Prefix");
        __result = 12; // 21
        return false; // don't execute the original method after this
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Game.Simulation.AgingSystem), "GetAdultAgeLimitInDays")]
    static bool GetAdultAgeLimitInDays_Prefix(ref int __result)
    {
        RealPop.Debug.Log("GetAdultAgeLimitInDays_Prefix");
        __result = 24; // 36
        return false; // don't execute the original method after this
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Game.Simulation.AgingSystem), "GetElderAgeLimitInDays")]
    static bool GetElderAgeLimitInDays_Prefix(ref int __result)
    {
        RealPop.Debug.Log("GetElderAgeLimitInDays_Prefix");
        __result = 77; // 84
        return false; // don't execute the original method after this
    }

}
*/


/*
 * 
// This example patch adds the loading of a custom ECS System after the AudioManager has
// its "OnGameLoadingComplete" method called. We're just using it as a entrypoint, and
// it won't affect anything related to audio.
[HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
class AudioManager_OnGameLoadingComplete
{
    static void Postfix(AudioManager __instance, Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
        if (!mode.IsGameOrEditor())
            return;

        // Here we add our custom ECS System to the game's ECS World, so it's "online" at runtime
        __instance.World.GetOrCreateSystem<RealPopSystem>();
    }
}

// This example patch enables the editor in the main menu
[HarmonyPatch(typeof(MenuUISystem), "IsEditorEnabled")]
class MenuUISystem_IsEditorEnabledPatch
{
    static bool Prefix(ref bool __result)
    {
        __result = true;

        return false; // Ignore original function
    }
}
// Thanks to @89pleasure for the MenuUISystem_IsEditorEnabledPatch snippet above
// https://github.com/89pleasure/cities2-mod-collection/blob/71385c000779c23b85e5cc023fd36022a06e9916/EditorEnabled/Patches/MenuUISystemPatches.cs

*/