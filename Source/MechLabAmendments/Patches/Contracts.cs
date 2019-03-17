using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using BattleTech.UI;
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
            __instance.AddContract("Assassinate_Headhunt_P", "AuriganPirates", "Locals", true);
            __instance.AddContract("Assassinate_Headhunt_PP", "MajestyMetals", "Locals", true);
            __instance.AddContract("Assassinate_Headhunt_PPP", "Betrayers", "Locals", true);
        }
    }

    // Dynamic ContractDifficultyVariance
    [HarmonyPatch(typeof(SimGameState), "GetDifficultyRangeForContract")]
    public static class SimGameState_GetDifficultyRangeForContract_Patch
    {
        public static bool Prepare()
        {
            return MechLabAmendments.EnableDynamicContractDifficultyVariance;
        }

        public static void Prefix(SimGameState __instance, ref int baseDiff)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
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
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Override.finalDifficulty: " + contract.Override.finalDifficulty);
                // Why the fuck are these empty for "SimGameState.AddContract()"?
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Override.ID: " + contract.Override.ID);
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contract.Override.filename: " + contract.Override.filename);


                //if (MechLabAmendments.ContractOverrideIDs.Contains(contract.Override.ID))
                if (MechLabAmendments.ContractOverrideNames.Contains(contract.Name))
                {
                    Logger.LogLine("[SimGameState_PrepContract_POSTFIX] Contract (" + contract.Name + ") is an MLA Contract. Overriding difficulty...");

                    int overrideDifficulty = -1;
                    int overrideReward = -1;

                    // Selecting by Difficulty as there currently is no selection by ID(s)
                    // Custom ContractOverrides should all be between 10 and 15 (BTG normally uses only up to 9)
                    List<Contract_MDD> contractsMDD = MetadataDatabase.Instance.GetContractsByDifficultyRange(10, 12, true);
                    foreach (Contract_MDD contractMDD in contractsMDD)
                    {
                        Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractMDD.ContractID: " + contractMDD.ContractID);
                        Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractMDD.Name: " + contractMDD.Name);
                        Logger.LogLine("[SimGameState_PrepContract_POSTFIX] contractMDD.Difficulty: " + contractMDD.Difficulty);

                        //if (contractMDD.ContractID == contract.Override.ID)
                        if (contractMDD.Name == contract.Name)
                        {
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
                        }
                    }
                }

                //Check
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] (" + contract.Name + ") CHECK contract.Difficulty: " + contract.Difficulty);
                Logger.LogLine("[SimGameState_PrepContract_POSTFIX] (" + contract.Name + ") CHECK contract.Override.finalDifficulty: " + contract.Override.finalDifficulty);
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
        public static void Postfix(SimGameState __instance, ref int __result, List<string> ___contractDiscardPile)
        {
            try
            {
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
