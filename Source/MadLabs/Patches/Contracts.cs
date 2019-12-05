using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using Harmony;
using UnityEngine;

namespace MadLabs.Patches
{

    // Dynamic ContractDifficultyVariance
    [HarmonyPatch(typeof(SimGameState), "GetDifficultyRangeForContract")]
    public static class SimGameState_GetDifficultyRangeForContract_Patch
    {
        public static bool Prepare()
        {
            return MadLabs.EnableDynamicContractDifficultyVariance;
        }

        public static bool Prefix(SimGameState __instance, int baseDiff, out int minDiff, out int maxDiff)
        {
            Logger.LogLine("----------------------------------------------------------------------------------------------------");
            Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] SimGameState.Constants.Story.ContractDifficultyVariance: " + __instance.Constants.Story.ContractDifficultyVariance);
            
            //int overrideContractDifficultyVariance = Utilities.GetMaxAllowedContractDifficultyVariance(__instance.SimGameMode, __instance.CompanyTags);
            //Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] overrideContractDifficultyVariance: " + overrideContractDifficultyVariance);
            //__instance.Constants.Story.ContractDifficultyVariance = overrideContractDifficultyVariance;

            int[] overrideContractDifficultyVariances = Utilities.GetMaxAllowedContractDifficultyVariances(__instance.SimGameMode, __instance.CompanyTags);
            Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] overrideContractDifficultyVariances[0]: " + overrideContractDifficultyVariances[0]);
            Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] overrideContractDifficultyVariances[1]: " + overrideContractDifficultyVariances[1]);


            minDiff = Mathf.Max(1, baseDiff - overrideContractDifficultyVariances[0]);
            maxDiff = Mathf.Max(1, baseDiff + overrideContractDifficultyVariances[1]);
            Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] baseDiff: " + baseDiff);
            Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] minDiff: " + minDiff);
            Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] maxDiff: " + maxDiff);

            // Skip original method as it would override minDiff/maxDiff again (of course)
            return false;
        }
    }

    // Info
    [HarmonyPatch(typeof(StarSystem), "GetSystemMaxContracts")]
    public static class StarSystem_GetSystemMaxContracts_Patch
    {
        public static void Postfix(StarSystem __instance, ref int __result)
        {
            Logger.LogLine("----------------------------------------------------------------------------------------------------");
            Logger.LogLine("[StarSystem_GetSystemMaxContracts_POSTFIX] StarSystem.Def.UseMaxContractOverride: " + __instance.Def.UseMaxContractOverride);
            Logger.LogLine("[StarSystem_GetSystemMaxContracts_POSTFIX] StarSystem.Def.MaxContractOverride: " + __instance.Def.MaxContractOverride);
            Logger.LogLine("[StarSystem_GetSystemMaxContracts_POSTFIX] StarSystem.Sim.Constants.Story.MaxContractsPerSystem: " + __instance.Sim.Constants.Story.MaxContractsPerSystem);
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetAllCurrentlySelectableContracts")]
    public static class SimGameState_GetAllCurrentlySelectableContracts_Patch
    {
        public static void Postfix(SimGameState __instance, ref int __result, List<string> ___contractDiscardPile)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");

                // Campaign
                //int currentSystemDifficulty = __instance.CurSystem.Def.GetDifficulty(SimGameState.SimGameType.KAMEA_CAMPAIGN);
                // Career
                //int currentSystemDifficulty = __instance.CurSystem.Def.GetDifficulty(SimGameState.SimGameType.CAREER);

                int currentSystemDifficulty = __instance.CurSystem.Def.GetDifficulty(__instance.SimGameMode);
                int globalDifficulty = Mathf.FloorToInt(__instance.GlobalDifficulty);
                int baseDifficulty = currentSystemDifficulty + globalDifficulty;
                int[] contractDifficultyVariances = Utilities.GetMaxAllowedContractDifficultyVariances(__instance.SimGameMode, __instance.CompanyTags);
                int minDifficulty = Mathf.Max(1, baseDifficulty - contractDifficultyVariances[0]);
                int maxDifficulty = Mathf.Max(1, baseDifficulty + contractDifficultyVariances[1]);

                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] currentSystemDifficulty: " + currentSystemDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] globalDifficulty: " + globalDifficulty);

                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] ---");
                int normalizedDifficulty = Utilities.GetNormalizedGlobalDifficulty(__instance);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] normalizedDifficulty: " + normalizedDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] ---");

                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] baseDifficulty: " + baseDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] contractDifficultyVariances[0]: " + contractDifficultyVariances[0]);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] contractDifficultyVariances[1]: " + contractDifficultyVariances[1]);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] minDifficulty: " + minDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] maxDifficulty: " + maxDifficulty);

                foreach (Contract contract in __instance.GlobalContracts)
                {
                    Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] SimGameState.CurSystem.GlobalContracts: " + contract.Name + "(Difficulty " + contract.Difficulty + ")");
                }
                foreach (Contract contract in __instance.CurSystem.SystemContracts)
                {
                    Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] SimGameState.CurSystem.SystemContracts: " + contract.Name + "(Difficulty " + contract.Difficulty + ")");
                }
                foreach (Contract contract in __instance.CurSystem.SystemBreadcrumbs)
                {
                    Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] SimGameState.CurSystem.SystemBreadcrumbs: " + contract.Name + "(Difficulty " + contract.Difficulty + ")");
                }
                if (__instance.HasTravelContract)
                {
                    Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] SimGameState.ActiveTravelContract: " + __instance.ActiveTravelContract.Name + "(Difficulty " + __instance.ActiveTravelContract.Difficulty + ")");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
}
