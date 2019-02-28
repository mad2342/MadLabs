using System;
using Harmony;
using BattleTech;
using BattleTech.Data;
using HBS.Collections;
using BattleTech.Framework;

namespace MechLabAmendments.Patches
{
    // Try to influence unit spawns
    [HarmonyPatch(typeof(UnitSpawnPointOverride), "GenerateUnit")]
    public static class UnitSpawnPointOverride_GenerateUnit_Patch
    {
        public static void Postfix(UnitSpawnPointOverride __instance, string lanceName, DataManager ___dataManager)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.IsUnitDefTagged: " + __instance.IsUnitDefTagged);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.IsPilotDefTagged: " + __instance.IsPilotDefTagged);

                // If we encounter untagged (thus manually defined in the contracts json) units we don't touch them
                if (!__instance.IsUnitDefTagged || !__instance.IsPilotDefTagged)
                {
                    return;
                }



                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] lanceName: " + lanceName);
                bool IsNewLance = Fields.CurrentLanceName != lanceName;

                if (IsNewLance)
                {
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] This is a new Lance, resetting Fields.CurrentLanceMadlabUnitCount");
                    Fields.CurrentLanceName = lanceName;
                    Fields.CurrentLanceMadlabUnitCount = 0;
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

                    if (!___dataManager.MechDefs.TryGet(selectedMechDefId, out selectedMechDef))
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Request for MechDef of selectedMechDefId(" + selectedMechDefId + ") failed. Aborting...");
                        return;
                    }
                    Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Request for MechDef of selectedMechDefId(" + selectedMechDefId + ") succeeded. Continuing...");

                    if (selectedMechDef.MechTags.Contains("unit_madlabs"))
                    {
                        Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] selectedMechDefId(" + selectedMechDefId + ") is a custom MadLabs unit.");

                        // Prepare load requests
                        LoadRequest loadRequest = ___dataManager.CreateLoadRequest(null, false);

                        //Count
                        Fields.CurrentLanceMadlabUnitCount++;

                        if (Fields.CurrentLanceMadlabUnitCount <= Fields.MaxLanceMadlabUnitCount)
                        {
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Only " + (Fields.CurrentLanceMadlabUnitCount - 1) + " custom MadLabs units in this Lance. Letting it pass and putting an elite pilot in.");

                            // Try to put another pilot in
                            __instance.selectedPilotDefId = Utilities.GetPilotIdForMechDef(selectedMechDef);

                            // Add to load request
                            loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, __instance.selectedPilotDefId, new bool?(false));
                        }
                        else
                        {
                            Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] Already " + Fields.MaxLanceMadlabUnitCount + " custom MadLabs units in this Lance. Resetting selectedMechDefId(" + selectedMechDefId + ") to its stock variant.");

                            // Replace with stock
                            stockMechDefId = selectedMechDef.ChassisID.Replace("chassisdef", "mechdef");
                            __instance.selectedUnitDefId = stockMechDefId;

                            // Add to load request
                            loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, __instance.selectedUnitDefId, new bool?(false));
                        }

                        // Fire load requests
                        loadRequest.ProcessRequests(1000u);
                    }
                }

                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] ---");
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedUnitDefId: " + __instance.selectedUnitDefId);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedUnitType: " + __instance.selectedUnitType);
                Logger.LogLine("[UnitSpawnPointOverride_GenerateUnit_POSTFIX] UnitSpawnPointOverride.selectedPilotDefId: " + __instance.selectedPilotDefId);
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
