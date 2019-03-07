using System;
using Harmony;
using BattleTech;
using BattleTech.Data;
using HBS.Collections;
using BattleTech.Framework;
using System.Collections.Generic;
using HBS.Data;
using System.Linq;

namespace MechLabAmendments.Patches
{
    // Get some Contract/Progression information
    [HarmonyPatch(typeof(Contract), "Begin")]
    public static class Contract_Begin_Patch
    {
        public static void Prefix(Contract __instance)
        {
            try
            {
                SimGameState simGameState = __instance.BattleTechGame.Simulation;
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.CompanyTags: " + simGameState.CompanyTags);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.DaysPassed: " + simGameState.DaysPassed);
                Logger.LogLine("[Contract_Begin_PREFIX] simGameState.GlobalDifficulty: " + simGameState.GlobalDifficulty);

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

                // If we encounter untagged (players lance or manually defined in the contracts json) units we don't touch them
                if (!__instance.IsUnitDefTagged || !__instance.IsPilotDefTagged)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.IsUnitDefTagged: " + __instance.IsUnitDefTagged);
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.IsPilotDefTagged: " + __instance.IsPilotDefTagged);
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Unit or Pilot was specified exactly via configuration. Aborting.");
                    return;
                }

                if (Fields.CurrentLanceName != lanceName)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] This is a new Lance, resetting Fields.CurrentLanceMadlabUnitCount");
                    Fields.CurrentLanceName = lanceName;
                    Fields.CurrentLanceMadlabsUnitCount = 0;
                }



                /*
                foreach (string key in ___dataManager.MechDefs.Keys)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ___dataManager.MechDefs.Keys: " + key);
                }
                // No generic PilotDefs in here, just Ronins
                foreach (string key in ___dataManager.PilotDefs.Keys)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ___dataManager.PilotDefs.Keys: " + key);
                }
                */



                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedUnitDefId: " + __instance.selectedUnitDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedUnitType: " + __instance.selectedUnitType);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedPilotDefId: " + __instance.selectedPilotDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");

                if (__instance.selectedUnitType == UnitType.Mech)
                {
                    string selectedMechDefId = __instance.selectedUnitDefId;
                    MechDef selectedMechDef = null;
                    string stockMechDefId = "";
                    string replacementMechDefId = "";

                    if (!___dataManager.MechDefs.TryGet(selectedMechDefId, out selectedMechDef))
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") couldn't get fetched. Aborting...");
                        return;
                    }
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") successfully requested. Continuing...");

                    if (selectedMechDef.MechTags.Contains("unit_madlabs"))
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") is a custom MadLabs unit.");

                        // Prepare load requests
                        LoadRequest loadRequest = ___dataManager.CreateLoadRequest(null, false);



                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.MaxAllowedMadlabsUnitsPerLance: " + Fields.MaxAllowedMadlabsUnitsPerLance);
                        if (Fields.CurrentLanceMadlabsUnitCount < Fields.MaxAllowedMadlabsUnitsPerLance)
                        {
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] " + Fields.CurrentLanceMadlabsUnitCount + "/" + Fields.MaxAllowedMadlabsUnitsPerLance + " MadLabs Units in this Lance. Will get an Madlabs Pilot.");



                            // Get ThreatLevel
                            int selectedMechsExtraThreatLevel = Utilities.GetExtraThreatLevelFromMechDef(selectedMechDef);
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") has extraThreatLevel: " + selectedMechsExtraThreatLevel);

                            // If selected Mech has a threatlevel too big for current game progression, select same chassis with lower threat rating
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Fields.MaxAllowedExtraThreatLevelByProgression: " + Fields.MaxAllowedExtraThreatLevelByProgression);
                            if (selectedMechsExtraThreatLevel > Fields.MaxAllowedExtraThreatLevelByProgression)
                            {
                                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Normalizing threatlevel");
                                string mechTagForThreatLevel = Utilities.GetMechTagForThreatLevel(Fields.MaxAllowedExtraThreatLevelByProgression);
                                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] mechTagForThreatLevel: " + mechTagForThreatLevel);
                                List<MechDef> allMechDefs = new List<MechDef>();
                                foreach (string key in ___dataManager.MechDefs.Keys)
                                {
                                    MechDef mechDef = ___dataManager.MechDefs.Get(key);
                                    allMechDefs.Add(mechDef);
                                }
                                List<string> mechDefIdsBasedOnSameChassis = allMechDefs
                                    .Where(mechDef => mechDef.ChassisID == selectedMechDef.ChassisID)
                                    .Where(mechDef => mechDef.MechTags.Contains(mechTagForThreatLevel))
                                    .Select(mechDef => mechDef.Description.Id)
                                    .ToList();

                                foreach (string Id in mechDefIdsBasedOnSameChassis)
                                {
                                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] mechDefIdsBasedOnSameChassis("+ selectedMechDef.ChassisID + "): " + Id);
                                }
                                

                                if (mechDefIdsBasedOnSameChassis.Count > 0)
                                {
                                    mechDefIdsBasedOnSameChassis.Shuffle<string>();
                                    replacementMechDefId = mechDefIdsBasedOnSameChassis[0];
                                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] replacementMechDefId: " + replacementMechDefId);

                                    // Replace
                                    __instance.selectedUnitDefId = replacementMechDefId;

                                    // Add to load request
                                    loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, __instance.selectedUnitDefId, new bool?(false));
                                }
                                else
                                {
                                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Couldn't find a replacement, falling back to stock. Will still get an Madlabs Pilot.");
                                    // Fall back to stock
                                    stockMechDefId = selectedMechDef.ChassisID.Replace("chassisdef", "mechdef");
                                    __instance.selectedUnitDefId = stockMechDefId;
                                }
                            }



                            // Try to put another pilot in
                            __instance.selectedPilotDefId = Utilities.GetPilotIdForMechDef(selectedMechDef);

                            // Add to load request
                            loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, __instance.selectedPilotDefId, new bool?(false));
                        }
                        else
                        {
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] " + Fields.CurrentLanceMadlabsUnitCount + "/" + Fields.MaxAllowedMadlabsUnitsPerLance + "  MadLabs Units in this Lance. Resetting selectedMechDefId(" + selectedMechDefId + ") to its stock variant.");

                            // Replace with stock
                            stockMechDefId = selectedMechDef.ChassisID.Replace("chassisdef", "mechdef");
                            __instance.selectedUnitDefId = stockMechDefId;

                            // Add to load request
                            loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, __instance.selectedUnitDefId, new bool?(false));
                        }

                        // Count
                        Fields.CurrentLanceMadlabsUnitCount++;

                        // Fire load requests
                        loadRequest.ProcessRequests(1000u);
                    }
                    else
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") is NOT a custom MadLabs unit. Let it pass unchanged.");
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
