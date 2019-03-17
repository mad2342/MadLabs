using System;
using Harmony;
using BattleTech;
using BattleTech.Data;
using HBS.Collections;
using BattleTech.Framework;

namespace MadLabs.Patches
{
    // Get and Save some Contract/Progression information
    [HarmonyPatch(typeof(Contract), "Begin")]
    public static class Contract_Begin_Patch
    {
        public static void Prefix(Contract __instance)
        {
            try
            {
                Logger.LogLine("[Contract_Begin_PREFIX] Contract.Name: " + __instance.Name);
                Logger.LogLine("[Contract_Begin_PREFIX] Contract.Difficulty: " + __instance.Difficulty);

                SimGameState simGameState = __instance.BattleTechGame.Simulation;
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.CompanyTags: " + simGameState.CompanyTags);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.DaysPassed: " + simGameState.DaysPassed);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.GlobalDifficulty: " + simGameState.GlobalDifficulty);

                // @ToDo: Play around with this. Should also allow multiple +++ Units on 2 skull constracts in late game...
                Fields.CurrentContractTotalThreatLevel = 0;
                Fields.MaxAllowedTotalThreatLevelPerContract = __instance.Difficulty;

                Fields.MaxAllowedExtraThreatLevelByProgression = Utilities.GetMaxAllowedExtraThreatLevelByProgression(simGameState.DaysPassed, simGameState.CompanyTags);
                Fields.MaxAllowedMadlabsUnitsPerLance = Utilities.GetMaxAllowedMadlabsUnitsByProgression(simGameState.GlobalDifficulty);
                Logger.LogLine("[Contract_Begin_PREFIX] Fields.MaxAllowedExtraThreatLevelByProgression: " + Fields.MaxAllowedExtraThreatLevelByProgression);
                Logger.LogLine("[Contract_Begin_PREFIX] Fields.MaxAllowedMadlabsUnitsPerLance: " + Fields.MaxAllowedMadlabsUnitsPerLance);

                /*
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.CompanyTags: " + simGameState.CompanyTags);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.DaysPassed: " + simGameState.DaysPassed);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.GlobalDifficulty: " + simGameState.GlobalDifficulty);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.GetCurrentMechCount(): " + simGameState.GetCurrentMechCount(false));
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.IsCampaign: " + simGameState.IsCampaign);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.isCareerMode(): " + simGameState.IsCareerMode());
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.TargetSystem.Name: " + simGameState.TargetSystem.Name);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.TargetSystem.Stats: " + simGameState.TargetSystem.Stats);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.TargetSystem.Tags: " + simGameState.TargetSystem.Tags);

                Logger.LogLine("[Contract_Begin_PREFIX] contract.Name: " + __instance.Name);
                Logger.LogLine("[Contract_Begin_PREFIX] contract.Difficulty: " + __instance.Difficulty);
                Logger.LogLine("[Contract_Begin_PREFIX] contract.IsStoryContract: " + __instance.IsStoryContract);
                */
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }


    // Mess around with unit spawns
    [HarmonyPatch(typeof(UnitSpawnPointOverride), "GenerateUnit")]
    public static class UnitSpawnPointOverride_GenerateUnit_Patch
    {
        public static void Postfix(UnitSpawnPointOverride __instance, DataManager ___dataManager, string lanceName)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] lanceName: " + lanceName);

                // If we encounter untagged (players lance, empty spawnpoints, or manually defined in the contracts json) units we don't touch them
                if (!__instance.IsUnitDefTagged || !__instance.IsPilotDefTagged)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.IsUnitDefTagged: " + __instance.IsUnitDefTagged);
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.IsPilotDefTagged: " + __instance.IsPilotDefTagged);
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Unit or Pilot was either specified exactly via configuration or excluded from spawning. Aborting.");
                    return;
                }

                if (Fields.CurrentLanceName != lanceName)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] This is a new Lance, resetting Fields.CurrentLanceMadlabsUnitCount");
                    Fields.CurrentLanceName = lanceName;
                    Fields.CurrentLanceMadlabsUnitCount = 0;
                }

                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedUnitDefId: " + __instance.selectedUnitDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedUnitType: " + __instance.selectedUnitType);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedPilotDefId: " + __instance.selectedPilotDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");

                if (__instance.selectedUnitType == UnitType.Mech)
                {
                    string selectedMechDefId = __instance.selectedUnitDefId;
                    MechDef selectedMechDef = null;
                    string replacementMechDefId = "";

                    if (!___dataManager.MechDefs.TryGet(selectedMechDefId, out selectedMechDef))
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") couldn't get fetched. Aborting...");
                        return;
                    }
                    else
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") successfully requested. Continuing...");
                    }

                    if (selectedMechDef.MechTags.Contains("unit_madlabs"))
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") is a custom MadLabs unit.");

                        // Prepare load requests
                        LoadRequest loadRequest = ___dataManager.CreateLoadRequest(null, false);



                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.MaxAllowedMadlabsUnitsPerLance: " + Fields.MaxAllowedMadlabsUnitsPerLance);
                        if (Fields.CurrentLanceMadlabsUnitCount < Fields.MaxAllowedMadlabsUnitsPerLance)
                        {
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] " + Fields.CurrentLanceMadlabsUnitCount + "/" + Fields.MaxAllowedMadlabsUnitsPerLance + " MadLabs Units in this Lance. Will get an Madlabs Pilot.");

                            // Normalize ThreatLevel
                            int selectedMechsExtraThreatLevel = Utilities.GetExtraThreatLevelFromMechDef(selectedMechDef);
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") has extraThreatLevel: " + selectedMechsExtraThreatLevel);

                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.MaxAllowedTotalThreatLevelPerContract: " + Fields.MaxAllowedTotalThreatLevelPerContract);
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.CurrentContractTotalThreatLevel: " + Fields.CurrentContractTotalThreatLevel);
                            int remainingAllowedExtraThreatLevelForContract = Fields.MaxAllowedTotalThreatLevelPerContract - Fields.CurrentContractTotalThreatLevel;

                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] remainingAllowedExtraThreatLevelForContract: " + remainingAllowedExtraThreatLevelForContract);
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.MaxAllowedExtraThreatLevelByProgression: " + Fields.MaxAllowedExtraThreatLevelByProgression);
                            int allowedExtraThreatLevel = Math.Min(Fields.MaxAllowedExtraThreatLevelByProgression, remainingAllowedExtraThreatLevelForContract);
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] allowedExtraThreatLevel: " + allowedExtraThreatLevel);

                            if (selectedMechsExtraThreatLevel > allowedExtraThreatLevel)
                            {
                                // Replace with less powerful version of the same Mech (Fallback to STOCK included)
                                replacementMechDefId = Utilities.GetMechDefIdBasedOnSameChassis(selectedMechDef.ChassisID, allowedExtraThreatLevel, ___dataManager);
                                __instance.selectedUnitDefId = replacementMechDefId;

                                // Track total additional threat
                                Fields.CurrentContractTotalThreatLevel += allowedExtraThreatLevel;

                                // Add to load request
                                loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, __instance.selectedUnitDefId, new bool?(false));
                            }
                            else
                            {
                                Fields.CurrentContractTotalThreatLevel += selectedMechsExtraThreatLevel;
                            }
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.CurrentContractTotalThreatLevel: " + Fields.CurrentContractTotalThreatLevel);


                            // Put another pilot in (even if the replacement did fall back to STOCK)
                            __instance.selectedPilotDefId = Utilities.GetPilotIdForMechDef(selectedMechDef);

                            // Add new pilot to load request
                            loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, __instance.selectedPilotDefId, new bool?(false));

                            // Count
                            Fields.CurrentLanceMadlabsUnitCount++;
                            
                        }
                        else
                        {
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Already " + Fields.MaxAllowedMadlabsUnitsPerLance + " MadLabs Unit(s) in this Lance. Resetting selectedMechDefId(" + selectedMechDefId + ") to its stock variant.");

                            // Replace with stock
                            replacementMechDefId = selectedMechDef.ChassisID.Replace("chassisdef", "mechdef");
                            __instance.selectedUnitDefId = replacementMechDefId;

                            // Add to load request
                            loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, __instance.selectedUnitDefId, new bool?(false));
                        }

                        // Fire load requests
                        loadRequest.ProcessRequests(1000u);
                    }
                    else
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") is no MadLabs Unit. Let it pass unchanged...");
                    }
                }

                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] CHECK UnitSpawnPointOverride.selectedUnitDefId: " + __instance.selectedUnitDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] CHECK UnitSpawnPointOverride.selectedUnitType: " + __instance.selectedUnitType);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] CHECK UnitSpawnPointOverride.selectedPilotDefId: " + __instance.selectedPilotDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    // Info
    [HarmonyPatch(typeof(UnitSpawnPointOverride), "SelectTaggedUnitDef")]
    public static class UnitSpawnPointOverride_SelectTaggedUnitDef_Patch
    {
        public static void Postfix(UnitSpawnPointOverride __instance, UnitDef_MDD __result, MetadataDatabase mdd, TagSet unitTagSet, TagSet unitExcludedTagSet, string lanceName)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                Logger.LogLine("[UnitSpawnPointOverride_SelectTaggedUnitDef_POSTFIX] lanceName: " + lanceName);
                Logger.LogLine("[UnitSpawnPointOverride_SelectTaggedUnitDef_POSTFIX] __result.UnitDefID: " + __result.UnitDefID);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    // Info
    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnUnit")]
    public static class UnitSpawnPointGameLogic_SpawnUnit_Patch
    {
        public static void Prefix(UnitSpawnPointGameLogic __instance, MechDef ___mechDefOverride)
        {
            try
            {
                Logger.LogLine("[UnitSpawnPointGameLogic_SpawnUnit_PREFIX] UnitDefId: " + __instance.UnitDefId);
                if (___mechDefOverride != null)
                {
                    Logger.LogLine("[UnitSpawnPointGameLogic_SpawnUnit_PREFIX] Overridden with: " + ___mechDefOverride.Description.Id);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
}
