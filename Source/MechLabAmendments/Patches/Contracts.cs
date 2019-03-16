using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using Harmony;
using HBS.Data;
using UnityEngine;

namespace MechLabAmendments.Patches
{
    [HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    public static class SimGameState__OnAttachUXComplete_ContractGenerator
    {
        public static bool Prepare()
        {
            return MechLabAmendments.EnableContractGenerator;
        }

        public static void Postfix(SimGameState __instance)
        {
            __instance.AddContract("Assassinate_Headhunt_PPP", "HostileMercenaries", "Locals", true);
        }
    }

    // @ToDo: Reset Constants.Story.ContractDifficultyVariance to DEFAULT for CAREER mode (find a good entrypoint)

    // Dynamic ContractDifficultyVariance
    [HarmonyPatch(typeof(SimGameState), "GetDifficultyRangeForContract")]
    public static class SimGameState_GetDifficultyRangeForContract_Patch
    {
        public static bool Prepare()
        {
            return MechLabAmendments.EnableContractDifficultyVariance;
        }

        public static void Prefix(SimGameState __instance, ref int baseDiff)
        {
            try
            {
                Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] SimGameState.Constants.Story.ContractDifficultyVariance: " + __instance.Constants.Story.ContractDifficultyVariance);
                int overrideContractDifficultyVariance = Utilities.GetMaxAllowedContractDifficultyVariance(__instance.SimGameMode, __instance.CompanyTags);
                Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] overrideContractDifficultyVariance: " + overrideContractDifficultyVariance);

                // Set
                __instance.Constants.Story.ContractDifficultyVariance = overrideContractDifficultyVariance;



                // Adjustment depending on current StarSystem?
                /*                 
                StarSystem system = __instance.CurSystem;
                Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] SimGameState.CurSystem: " + __instance.CurSystem.Name);
                Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] SimGameState.CompanyTags: " + __instance.CompanyTags);
                bool isCampaignMode = __instance.SimGameMode == SimGameState.SimGameType.KAMEA_CAMPAIGN;
                Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] isCampaignMode: " + isCampaignMode);
                bool isPostCampaign = __instance.CompanyTags.Contains("story_complete");
                Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] isPostCampaign: " + isPostCampaign);

                if (isCampaignMode && isPostCampaign)
                {
                    SimGameState.SimGameType simGameType = SimGameState.SimGameType.CAREER;
                    // GlobalDifficulty for post KAMEA_CAMPAIGN is too high in comparison to CAREER. Needs to be adjusted if it should ever be used like this
                    int overrideBaseDiff = system.Def.GetDifficulty(simGameType) + Mathf.FloorToInt(__instance.GlobalDifficulty);
                    Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] StarSystemDef.Difficulty: " + system.Def.GetDifficulty(simGameType));
                    Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] SimGameState.GlobalDifficulty: " + Mathf.FloorToInt(__instance.GlobalDifficulty));

                    // Set
                    baseDiff = overrideBaseDiff;
                    Logger.LogLine("[SimGameState_GetDifficultyRangeForContract_PREFIX] baseDiff overridden with: " + overrideBaseDiff);
                }
                */
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    // Forcing difficulty the hard way
    [HarmonyPatch(typeof(SimGameState), "PrepContract")]
    public static class SimGameState_PrepContract_Patch
    {
        public static void Postfix(SimGameState __instance, ref Contract contract)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Name: " + contract.Name);
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Difficulty: " + contract.Difficulty);

                // Why the fuck are these empty? -> Fixed by HBS in 1.5.X
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Override.ID: " + contract.Override.ID);
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Override.finalDifficulty: " + contract.Override.finalDifficulty);



                int overrideDifficulty = -1;
                int overrideReward = -1;

                // Custom ContractOverrides should all be between 9 and 15 (BTG normally uses only up to 8)
                List<Contract_MDD> contractsMDD = MetadataDatabase.Instance.GetContractsByDifficultyRange(9, 15, true);
                foreach (Contract_MDD contractMDD in contractsMDD)
                {
                    Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractMDD.ContractID: " + contractMDD.ContractID);
                    Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractMDD.Name: " + contractMDD.Name);
                    Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractMDD.Difficulty: " + contractMDD.Difficulty);

                    // Double check custom ContractOverride by ID and handle difficulty override by Name
                    // Again: WHY the fuck is contract.Override.ID empty at this point?
                    // Remember to ALWAYS use unique names
                    if (MechLabAmendments.ContractOverrideIDs.Contains(contractMDD.ContractID) && contract.Name == contractMDD.Name)
                    {
                        Logger.LogLine("[SimGameState_PrepContract_POSTFIX] Contract (" + contract.Name + ") is an MLA Contract. Overriding difficulty...");
                        overrideDifficulty = (int)contractMDD.Difficulty;

                        if (overrideDifficulty > 0 && overrideDifficulty != contract.Difficulty)
                        {
                            // Set
                            contract.SetFinalDifficulty(overrideDifficulty);

                            // Adjust rewards accordingly
                            if (contract.Override.contractRewardOverride >= 0)
                            {
                                overrideReward = contract.Override.contractRewardOverride;
                            }
                            else
                            {
                                overrideReward = __instance.CalculateContractValueByContractType(contract.ContractType, overrideDifficulty, (float)__instance.Constants.Finances.ContractPricePerDifficulty, __instance.Constants.Finances.ContractPriceVariance, 0);
                            }
                            overrideReward = SimGameState.RoundTo((float)overrideReward, 1000);
                            contract.SetInitialReward(overrideReward);
                        }

                        // TEST
                        //ContractOverride contractOverride = __instance.DataManager.ContractOverrides.Get(contractMDD.ContractID);
                        //Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractOverride.ID: " + contractOverride.ID);
                        //Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractOverride.contractName: " + contractOverride.contractName);
                        //Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractOverride.difficulty: " + contractOverride.difficulty);                    }
                    }
                }

                //Check again
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] (" + contract.Name + ") contract.Difficulty: " + contract.Difficulty);
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] (" + contract.Name + ") contract.Override.finalDifficulty: " + contract.Override.finalDifficulty);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
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
        public static void Postfix(SimGameState __instance, ref int __result)
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
                int contractDifficultyVariance = __instance.Constants.Story.ContractDifficultyVariance;
                int minDifficulty = Mathf.Max(1, baseDifficulty - contractDifficultyVariance);
                int maxDifficulty = Mathf.Max(1, baseDifficulty + contractDifficultyVariance);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] currentSystemDifficulty: " + currentSystemDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] globalDifficulty: " + globalDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] baseDifficulty: " + baseDifficulty);
                Logger.LogLine("[SimGameState_GetAllCurrentlySelectableContracts_POSTFIX] contractDifficultyVariance: " + contractDifficultyVariance);
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
