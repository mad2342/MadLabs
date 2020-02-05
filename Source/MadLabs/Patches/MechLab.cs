using System;
using Harmony;
using BattleTech.UI;
using HBS;
using System.Linq;
using System.Collections.Generic;
using MadLabs.Extensions;
using BattleTech.UI.TMProWrapper;
using BattleTech;

namespace MadLabs.Patches
{
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
                    Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] This is NOT SimGame. Aborting.");
                    return;
                }

                HBS_InputField mechNickname = (HBS_InputField)AccessTools.Field(typeof(MechLabMechInfoWidget), "mechNickname").GetValue(__instance);
                string mechDefaultVariant = mechLabPanel.activeMechDef.Chassis.VariantName;
                Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] mechDefaultVariant: " + mechDefaultVariant);
                string currentNickname = mechNickname.text;

                if (string.IsNullOrEmpty(currentNickname) || (currentNickname.Length > 0 && currentNickname.Substring(0, 1) != "/"))
                {
                    Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] no command");
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
                List<string> validateCommands = new List<string>() { "/validate", "/v" };
                List<string> chassisCommands = new List<string>() { "/chassis ", "/c " };
                List<string> allCommands = loadCommands.Concat(stockCommands).Concat(saveCommands).Concat(validateCommands).Concat(chassisCommands).ToList();

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
                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] triggerLoad: " + mechDefId);
                        }
                        else if (stockCommands.Contains(command))
                        {
                            triggerStock = true;

                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] triggerStock");
                        }
                        else if (saveCommands.Contains(command))
                        {
                            triggerSave = true;

                            mechDefName = mechDefName + " " + mechDefIdSuffix;
                            mechDefId = $"{mechLabPanel.activeMechDef.Description.Id}_{mechDefIdSuffix}";
                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] triggerSave: " + mechDefId);
                        }
                        else if (validateCommands.Contains(command))
                        {
                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] validating...");
                            mechLabPanel.ValidateAllMechDefTonnages();
                        }
                        else if (chassisCommands.Contains(command))
                        {
                            string variant = currentNickname.Replace(command, "").ToUpper();

                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] forcing new chassis in stock loadout...");
                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] variant: " + variant);

                            MechDef newMechDef = mechLabPanel.GetMechDefFromVariantName(variant);
                            if (newMechDef != null)
                            {
                                Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] loading: " + newMechDef.Description.Id);
                                mechLabPanel.LoadMech(newMechDef);
                                return;
                            }
                        }
                        else
                        {
                            Logger.Debug("[MechLabMechInfoWidget_OnNameInputEndEdit_POSTFIX] no action defined for command: " + command);
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
                        .Create("Export MechDef", "This will export current MechDef to /Mods/MadLabs/MechDefs/" + mechDefId + ".json")
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
                Logger.Error(e);
            }
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
                    Logger.Debug("[MechLabMechInfoWidget_SetData_PREFIX] This is NOT SimGame. Aborting.");
                    return;
                }

                Logger.Debug("[MechLabMechInfoWidget_SetData_PREFIX] Disable text validation, expand character limit");

                HBS_InputField mechNickname = (HBS_InputField)AccessTools.Field(typeof(MechLabMechInfoWidget), "mechNickname").GetValue(__instance);
                mechNickname.characterLimit = 20;
                mechNickname.contentType = HBS_InputField.ContentType.Standard;
                mechNickname.characterValidation = HBS_InputField.CharacterValidation.None;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }



    [HarmonyPatch(typeof(MechLabPanel), "HandleEnterKeypress")]
    public static class MechLabPanel_HandleEnterKeypress_Patch
    {
        public static bool Prefix(MechLabPanel __instance)
        {
            Logger.Debug("[MechLabPanel_HandleEnterKeypress_PREFIX] Disable EnterKeyPress");
            return false;
        }
    }



    [HarmonyPatch(typeof(MechLabPanel), "ExitMechLab")]
    public static class MechLabPanel_ExitMechLab_Patch
    {
        public static void Postfix(MechLabPanel __instance)
        {
            Fields.IsMechLabActive = false;
            Logger.Debug("[MechLabPanel_ExitMechLab_POSTFIX] Fields.IsMechLabActive: " + Fields.IsMechLabActive);
        }
    }



    [HarmonyPatch(typeof(MechLabPanel), "OnRequestResourcesComplete")]
    public static class MechLabPanel_OnRequestResourcesComplete_Patch
    {
        public static void Postfix(MechLabPanel __instance)
        {
            if (!__instance.IsSimGame)
            {
                Logger.Debug("[MechLabPanel_OnRequestResourcesComplete_POSTFIX] This is NOT SimGame. Aborting.");
                return;
            }

            Fields.IsMechLabActive = true;
            Logger.Debug("[MechLabPanel_OnRequestResourcesComplete_POSTFIX] Fields.IsMechLabActive: " + Fields.IsMechLabActive);
        }
    }



    [HarmonyPatch(typeof(GenericPopup), "HandleEnterKeypress")]
    public static class GenericPopup_HandleEnterKeypress_Patch
    {
        public static bool Prefix(GenericPopup __instance)
        {
            Logger.Debug("[GenericPopup_HandleEnterKeypress_PREFIX] Fields.IsMechLabActive: " + Fields.IsMechLabActive);
            
            if (Fields.IsMechLabActive)
            {
                Logger.Debug("[GenericPopup_HandleEnterKeypress_PREFIX] Disable EnterKeyPress");
                return false;
            }
            return true;
        }
    }



    // Allow high quality and custom gear in Skirmish MechLab
    [HarmonyPatch(typeof(MechLabPanel), "ComponentDefTagsValid")]
    public static class MechLabPanel_ComponentDefTagsValid_Patch
    {
        public static void Postfix(MechLabPanel __instance, MechComponentDef def, ref bool __result)
        {
            if (__instance.IsSimGame)
            {
                //Logger.Debug("[MechLabPanel_ComponentDefTagsValid_POSTFIX] This is SimGame. Aborting.");
                return;
            }

            Logger.Debug("[MechLabPanel_ComponentDefTagsValid_POSTFIX] __result: " + __result);
            Logger.Debug("[MechLabPanel_ComponentDefTagsValid_POSTFIX] def.Description.Id: " + def.Description.Id);

            __result = !def.ComponentTags.Contains("BLACKLISTED") && !def.ComponentTags.Contains("component_type_debug") && def.Description.Purchasable;

            Logger.Debug("[MechLabPanel_ComponentDefTagsValid_POSTFIX] __result: " + __result);
            Logger.Debug("---");
        }
    }
}
