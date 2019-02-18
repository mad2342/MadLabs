using System;
using System.Reflection;
using System.IO;
using Harmony;
using BattleTech.UI;
using HBS;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using HBS.Collections;
using BattleTech.Framework;

namespace MechLabAmendments
{
    public class MechLabAmendments
    {
        public static string LogPath;
        public static string ModDirectory;

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 2;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;

            LogPath = Path.Combine(ModDirectory, "MechLabAmendments.log");
            File.CreateText(MechLabAmendments.LogPath);

            var harmony = HarmonyInstance.Create("de.mad.MechLabAmendments");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }



    [HarmonyPatch(typeof(MechLabPanel), "HandleEnterKeypress")]
    public static class MechLabPanel_HandleEnterKeypress_Patch
    {
        public static bool Prefix(MechLabPanel __instance)
        {
            Logger.LogLine("[MechLabPanel_HandleEnterKeypress_PREFIX] Disable EnterKeyPress");
            return false;
        }
    }
    [HarmonyPatch(typeof(GenericPopup), "HandleEnterKeypress")]
    public static class GenericPopup_HandleEnterKeypress_Patch
    {
        public static bool Prefix(GenericPopup __instance)
        {
            Logger.LogLine("[GenericPopup_HandleEnterKeypress_PREFIX] Disable EnterKeyPress");
            return false;
        }
    }



    [HarmonyPatch(typeof(MechLabMechInfoWidget), "SetData")]
    public static class MechLabMechInfoWidget_SetData_Patch
    {
        public static void Prefix(MechLabMechInfoWidget __instance)
        {
            try
            {
                MechLabPanel mechLabPanel = (MechLabPanel)AccessTools.Field(typeof(MechLabMechInfoWidget), "mechLab").GetValue(__instance);

                if (!mechLabPanel.IsSimGame)
                {
                    Logger.LogLine("[MechLabMechInfoWidget_SetData_PREFIX] This is NOT SimGame. Aborting.");
                    return;
                }

                Logger.LogLine("[MechLabMechInfoWidget_SetData_PREFIX] Disable text validation, expand character limit");

                TMP_InputField mechNickname = (TMP_InputField)AccessTools.Field(typeof(MechLabMechInfoWidget), "mechNickname").GetValue(__instance);
                mechNickname.characterLimit = 20;
                mechNickname.contentType = TMP_InputField.ContentType.Standard;
                mechNickname.characterValidation = TMP_InputField.CharacterValidation.None;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }



    [HarmonyPatch(typeof(MechLabMechInfoWidget), "OnNameInputEndEdit")]
    public static class MechLabMechInfoWidget_OnNameInputEndEdit_Patch
    {
        public static void Postfix(MechLabMechInfoWidget __instance)
        {
            try
            {
                MechLabPanel mechLabPanel = (MechLabPanel)AccessTools.Field(typeof(MechLabMechInfoWidget), "mechLab").GetValue(__instance);

                if (!mechLabPanel.IsSimGame)
                {
                    Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] This is NOT SimGame. Aborting.");
                    return;
                }

                TMP_InputField mechNickname = (TMP_InputField)AccessTools.Field(typeof(MechLabMechInfoWidget), "mechNickname").GetValue(__instance);
                string mechDefaultVariant = mechLabPanel.activeMechDef.Chassis.VariantName;
                Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] mechDefaultVariant: " + mechDefaultVariant);
                string currentNickname = mechNickname.text;

                if (string.IsNullOrEmpty(currentNickname) || (currentNickname.Length > 0 && currentNickname.Substring(0, 1) != "/"))
                {
                    Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] no command");
                    return;
                }

                bool triggerLoad = false;
                bool triggerStock = false;
                bool triggerSave = false;

                List<string> loadCommands = new List<string>() { "/load ", "/l ", "/apply " };
                List<string> stockCommands = new List<string>() { "/stock" };
                stockCommands.Add("/" + mechDefaultVariant);
                stockCommands.Add("/" + mechDefaultVariant.ToLower());
                List<string> saveCommands = new List<string>() { "/save ", "/s ", "/export " };
                List<string> allCommands = loadCommands.Concat(stockCommands).Concat(saveCommands).ToList();

                string mechDefIdSuffix = "";
                string mechDefId = "";
                string mechDefName = mechLabPanel.activeMechDef.Chassis.Description.Name;

                foreach (string command in allCommands)
                {
                    if (currentNickname.Contains(command))
                    {
                        mechDefIdSuffix = currentNickname.Replace(command, "");
                        mechDefIdSuffix = mechDefIdSuffix.Replace(" ", "-");
                        mechDefIdSuffix = mechDefIdSuffix.ToUpper();

                        if (loadCommands.Contains(command))
                        {
                            triggerLoad = true;

                            mechDefId = $"{mechLabPanel.activeMechDef.Description.Id}_{mechDefIdSuffix}";
                            Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] triggerLoad: " + mechDefId);
                        }
                        else if (stockCommands.Contains(command))
                        {
                            triggerStock = true;

                            Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] triggerStock");
                        }
                        else if (saveCommands.Contains(command))
                        {
                            triggerSave = true;

                            mechDefName = mechDefName + " " + mechDefIdSuffix;
                            mechDefId = $"{mechLabPanel.activeMechDef.Description.Id}_{mechDefIdSuffix}";
                            Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] triggerSave: " + mechDefId);
                        }
                        else
                        {
                            Logger.LogLine("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] no action defined for command: " + command);
                        }

                    }
                }

                // @ToDo: Try to use LoadingCurtain

                if (triggerLoad)
                {
                    GenericPopupBuilder
                        .Create("Apply Loadout", "If you have all necessary components available this will apply saved loadout: " + mechDefId)
                        .AddButton("Cancel", null, true, null)
                        .AddButton("Apply", new Action(() => mechLabPanel.ApplyLoadout(mechDefId)), true, null)
                        .CancelOnEscape()
                        .AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.5f, true)
                        .SetAlwaysOnTop()
                        .SetOnClose(delegate
                        {
                            // Nothing yet 
                        })
                        .Render();
                }
                else if (triggerStock)
                {
                    GenericPopupBuilder
                        .Create("Set To Stock", "If you have all necessary components available this will set the current loadout to stock")
                        .AddButton("Cancel", null, true, null)
                        .AddButton("Apply", new Action(() => mechLabPanel.SetToStock()), true, null)
                        .CancelOnEscape()
                        .AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.5f, true)
                        .SetAlwaysOnTop()
                        .SetOnClose(delegate
                        {
                            // Nothing yet
                        })
                        .Render();
                }
                else if (triggerSave)
                {
                    GenericPopupBuilder
                        .Create("Export MechDef", "This will export current MechDef to /Mods/MechLabAmendments/MechDefs/" + mechDefId + ".json")
                        .AddButton("Cancel", null, true, null)
                        .AddButton("Export", new Action(() => mechLabPanel.ExportCurrentMechDefToJson(mechDefId, mechDefName)), true, null)
                        .CancelOnEscape()
                        .AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.5f, true)
                        .SetAlwaysOnTop()
                        .SetOnClose(delegate
                        {
                            // Nothing yet
                        })
                        .Render();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    // Try to limit the amount of plus weapons in salvage
    // In Vanilla the OpFor *never* fields Mechs with rare weapons, these get generated on salvage creation
    [HarmonyPatch(typeof(Contract), "AddMechComponentToSalvage")]
    public static class Contract_AddMechComponentToSalvage_Patch
    {
        public static bool Prefix(Contract __instance, ref MechComponentDef def, DataManager ___dataManager)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] Handling def: " + def.Description.Id + "(ComponentType: " + def.ComponentType + ")");

                // If rare upgrades are found, just cancel original method completely -> Skip adding anything to salvage for this component
                if (def.ComponentType == ComponentType.Upgrade)
                {
                    Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] Component is an upgrade, skipping original method (BUT STILL calling Postfixes if existent!)");
                    return false;
                }

                // Don't touch upgrades for now
                if (def.ComponentType != ComponentType.Weapon)
                {
                    return true;
                }
                // Don't touch stock weapons either
                if (def.Description.Rarity <= 0)
                {
                    return true;
                }

                // At this point only RARE WEAPONS should be left to handle
                // Adding in a random chance to keep exactly the item that will be passed into the original method.
                // Default behaviour is to replace any rare weapon with its stock version and let vanillas algorithms decide if some of them will be replaced by rare counterparts
                // Note that even if the current component is rare and it passes the test to be kept, the original method will still probably replace it with *another* rare component.
                SimGameState simGameState = __instance.BattleTechGame.Simulation;
                float keepInitialRareWeaponChance = simGameState.NetworkRandom.Float(0f, 1f);
                bool keepInitialRareWeapon = keepInitialRareWeaponChance > 0.8f; // 20%
                
                // Currently handled rare weapons should pass into original method as is
                if (keepInitialRareWeapon)
                {
                    Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] (" + def.Description.Id + ") passes into original method");
                    return true;
                }
                
                // Resetting rare weapon to its stock version if above check to potentially keep it fails
                WeaponDef weaponDef = def as WeaponDef;
                string weaponOriginalId = def.Description.Id;
                //Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] (" + weaponOriginalId + ") is going to be replaced by its stock version");
                string weaponStockId = def.ComponentType.ToString() + "_" + weaponDef.Type.ToString() + "_" + weaponDef.WeaponSubType.ToString() + "_0-STOCK";
                //Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] (" + weaponStockId + ") is current weapons stock version");

                using (MetadataDatabase metadataDatabase = new MetadataDatabase())
                {
                    def = ___dataManager.WeaponDefs.Get(weaponStockId);
                }
                Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] (" + weaponOriginalId + ") was replaced with stock version (" + def.Description.Id + ")");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return true;
            }
        }

        public static void Postfix(Contract __instance, MechComponentDef def, List<SalvageDef> ___finalPotentialSalvage)
        {
            try
            {
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
                //Logger.LogLine("[Contract_AddMechComponentToSalvage_POSTFIX] Checking def: " + def.Description.Id);

                // Apart from MechComponents also MechParts fill this List
                //Logger.LogLine("[Contract_AddMechComponentToSalvage_POSTFIX] ___finalPotentialSalvage.Count: " + ___finalPotentialSalvage.Count);

                // If prefix returns false this is empty on first call. Need to check first
                if (___finalPotentialSalvage != null && ___finalPotentialSalvage.Count > 0)
                {
                    SalvageDef lastAddedSalvageItem = ___finalPotentialSalvage.Last();
                    MechComponentDef lastAddedMechComponent = lastAddedSalvageItem.MechComponentDef ?? null;

                    // Check only rare weapons
                    if (def.ComponentType == ComponentType.Weapon && def.Description.Rarity > 0)
                    {
                        Logger.LogLine("[Contract_AddMechComponentToSalvage_POSTFIX] (" + def.Description.Id + ") passed into original method");
                    }
                    // Check if method DID transform components
                    if (lastAddedMechComponent != null && (def.Description.Id != lastAddedMechComponent.Description.Id))
                    {
                        Logger.LogLine("[Contract_AddMechComponentToSalvage_POSTFIX] (" + def.Description.Id + ") was changed to (" + lastAddedMechComponent.Description.Id + ")");
                    }
                    // Check all final salvage
                    if (lastAddedMechComponent != null)
                    //if (lastAddedMechComponent != null && lastAddedMechComponent.Description.Rarity > 0)
                    {
                        Logger.LogLine("[Contract_AddMechComponentToSalvage_POSTFIX] (" + lastAddedMechComponent.Description.Id + ") was added to salvage");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    // Try to influence unit spawns
    [HarmonyPatch(typeof(LanceSpawnerGameLogic), "ContractInitialize")]
    public static class LanceSpawnerGameLogic_ContractInitialize_Patch
    {
        public static void Prefix(LanceSpawnerGameLogic __instance)
        {
            try
            {
                Contract activeContract = __instance.Combat.ActiveContract;

                /* Info about players lance only
                foreach (string key in activeContract.Lances.Lances.Keys)
                {
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] activeContract.Lances.Lances.Key: " + key);
                    foreach (SpawnableUnit spawnableUnit in activeContract.Lances.Lances[key])
                    {
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] activeContract.Lances.Lances.spawnableUnit.UnitId: " + spawnableUnit.UnitId);
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] activeContract.Lances.Lances.spawnableUnit.unitType: " + spawnableUnit.unitType.ToString());
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] activeContract.Lances.Lances.spawnableUnit.PilotId: " + spawnableUnit.PilotId);
                    }
                }
                */

                UnitSpawnPointGameLogic[] unitSpawnPointGameLogicList = __instance.unitSpawnPointGameLogicList;
                Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogicList.Length: " + unitSpawnPointGameLogicList.Length);
                int CountMadlabUnitsInThisLance = 0;

                for (int i = 0; i < unitSpawnPointGameLogicList.Length; i++)
                {
                    UnitSpawnPointGameLogic unitSpawnPointGameLogic = unitSpawnPointGameLogicList[i];

                    Logger.LogLine("---");
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.HasUnitToSpawn: " + unitSpawnPointGameLogic.HasUnitToSpawn);
                    //Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.EncounterTags: " + unitSpawnPointGameLogic.EncounterTags);
                    //Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.team: " + unitSpawnPointGameLogic.team);
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.unitType: " + unitSpawnPointGameLogic.unitType);
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.unitTagSet: " + unitSpawnPointGameLogic.unitTagSet.ToString());
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.unitExcludedTagSet: " + unitSpawnPointGameLogic.unitExcludedTagSet.ToString());
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.IsTaggedUnit: " + unitSpawnPointGameLogic.IsTaggedUnit);
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] unitSpawnPointGameLogic.UnitDefId: " + unitSpawnPointGameLogic.UnitDefId);
                    Logger.LogLine("---");

                    if (!unitSpawnPointGameLogic.HasUnitToSpawn || unitSpawnPointGameLogic.unitType != UnitType.Mech)
                    {
                        continue;
                    }

                    // This doesn't work out as expected. Initially existent lances don't spawn any units. Reinforcements work though.
                    //SpawnableUnit OverrideSpawnableUnit = new SpawnableUnit(unitSpawnPointGameLogic.team, "mechdef_centurion_CN9-AL", unitSpawnPointGameLogic.pilotDefId, UnitType.Mech);
                    //unitSpawnPointGameLogic.OverrideSpawn(OverrideSpawnableUnit);



                    // Brute force reflection way works...
                    string originalMechDefId = unitSpawnPointGameLogic.UnitDefId;
                    MechDef originalMechDef = null;
                    string overrideMechDefId = "";
                    MechDef overrideMechDef = null;

                    if (!unitSpawnPointGameLogic.Combat.DataManager.MechDefs.TryGet(originalMechDefId, out originalMechDef))
                    {
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] originalMechDefId(" + originalMechDefId + ") request failed");
                        continue;
                    }
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] originalMechDef.Description.Id: " + originalMechDef.Description.Id);
                    Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] originalMechDef.MechTags: " + originalMechDef.MechTags);

                    // Check original MechDef
                    if (originalMechDef.MechTags.Contains("unit_madlabs"))
                    {
                        CountMadlabUnitsInThisLance++;
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] Found a custom MadLabs unit. CountMadlabUnitsInThisLance: " + CountMadlabUnitsInThisLance);
                    }
                    if (CountMadlabUnitsInThisLance <= 2)
                    {
                        continue;
                    }
                    else
                    {
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] Reached limit of a custom MadLabs units in this lance. CountMadlabUnitsInThisLance: " + CountMadlabUnitsInThisLance);
                    }
                    



                    // Replace with stock
                    overrideMechDefId = originalMechDef.ChassisID.Replace("chassisdef", "mechdef");

                    // Check overridden MechDef
                    if (!unitSpawnPointGameLogic.Combat.DataManager.MechDefs.TryGet(overrideMechDefId, out overrideMechDef))
                    {
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] overrideMechDefId(" + overrideMechDefId + ") request failed");
                        continue;
                    }
                    if (!overrideMechDef.DependenciesLoaded(1000u))
                    {
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] overrideMechDefId(" + overrideMechDefId + ") dependencies failed");
                        continue;
                    }

                    // Replace
                    if (overrideMechDef != null)
                    {
                        Logger.LogLine("[LanceSpawnerGameLogic_ContractInitialize_PREFIX] overrideMechDefId(" + overrideMechDefId + ") request & dependencies succeeded");

                        overrideMechDef.Refresh();
                        AccessTools.Field(typeof(UnitSpawnPointGameLogic), "mechDefOverride").SetValue(unitSpawnPointGameLogic, overrideMechDef);
                    }
                }
                Logger.LogLine("----------------------------------------------------------------------------------------------------");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(UnitSpawnPointOverride), "SelectTaggedUnitDef")]
    public static class UnitSpawnPointOverride_SelectTaggedUnitDef_Patch
    {
        public static void Prefix(UnitSpawnPointOverride __instance, MetadataDatabase mdd, TagSet unitTagSet, TagSet unitExcludedTagSet, string lanceName)
        {
            try
            {
                //Logger.LogLine("[UnitSpawnPointOverride_SelectTaggedUnitDef_PREFIX] unitTagSet: " + unitTagSet.ToString());
                //Logger.LogLine("[UnitSpawnPointOverride_SelectTaggedUnitDef_PREFIX] unitExcludedTagSet: " + unitExcludedTagSet.ToString());
                //Logger.LogLine("[UnitSpawnPointOverride_SelectTaggedUnitDef_PREFIX] lanceName: " + lanceName);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public static void Postfix(UnitSpawnPointOverride __instance, UnitDef_MDD __result, MetadataDatabase mdd, TagSet unitTagSet, TagSet unitExcludedTagSet, string lanceName)
        {
            try
            {
                Logger.LogLine("[UnitSpawnPointOverride_SelectTaggedUnitDef_POSTFIX] __result.UnitDefID: " + __result.UnitDefID);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

    /* 
    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "RequestUnitLoad")]
    public static class UnitSpawnPointGameLogic_RequestUnitLoad_Patch
    {
        public static void Prefix(UnitSpawnPointGameLogic __instance)
        {
            try
            {
                UnitType unitType = __instance.unitType;
                Logger.LogLine("[UnitSpawnPointGameLogic_RequestUnitLoad_PREFIX] unitType: " + unitType);
                string unitDefId = __instance.UnitDefId;
                Logger.LogLine("[UnitSpawnPointGameLogic_RequestUnitLoad_PREFIX] unitDefId: " + unitDefId);
                TagSet unitTagSet = __instance.unitTagSet;
                Logger.LogLine("[UnitSpawnPointGameLogic_RequestUnitLoad_PREFIX] unitTagSet: " + unitTagSet.ToString());
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
    */

    [HarmonyPatch(typeof(UnitSpawnPointGameLogic), "SpawnUnit")]
    public static class UnitSpawnPointGameLogic_SpawnUnit_Patch
    {
        public static void Prefix(UnitSpawnPointGameLogic __instance, MechDef ___mechDefOverride)
        {
            try
            {
                Logger.LogLine("[UnitSpawnPointGameLogic_SpawnUnit_PREFIX] __instance.UnitDefId: " + __instance.UnitDefId);
                if (___mechDefOverride != null)
                {
                    Logger.LogLine("[UnitSpawnPointGameLogic_SpawnUnit_PREFIX] ___mechDefOverride: " + ___mechDefOverride.Description.Id);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        } 
    }



    /*
    [HarmonyPatch(typeof(UIModule), "OnAddedToHierarchy")]
    public static class UIModule_OnAddedToHierarchy_Patch
    {
        public static void Postfix(UIModule __instance)
        {
            try
            {
                if (__instance.PrefabName == "uixPrfPanl_ML_stockconfigModalPopup")
                {
                    UserInterfaceExtender UIE = new UserInterfaceExtender();
                    UIE.ExtendStockInfoPopup();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
    */
}
