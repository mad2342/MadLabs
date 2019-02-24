using System;
using Harmony;
using BattleTech.UI;
using HBS;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using MechLabAmendments.Extensions;

namespace MechLabAmendments.Patches
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



    [HarmonyPatch(typeof(MechLabPanel), "HandleEnterKeypress")]
    public static class MechLabPanel_HandleEnterKeypress_Patch
    {
        public static bool Prefix(MechLabPanel __instance)
        {
            Logger.LogLine("[MechLabPanel_HandleEnterKeypress_PREFIX] Disable EnterKeyPress");
            return false;
        }
    }


    // @ToDo: Limit this to only concern generic popups triggered by my patches
    [HarmonyPatch(typeof(GenericPopup), "HandleEnterKeypress")]
    public static class GenericPopup_HandleEnterKeypress_Patch
    {
        public static bool Prefix(GenericPopup __instance)
        {
            Logger.LogLine("[GenericPopup_HandleEnterKeypress_PREFIX] __instance.ParentModule.PrefabName: " + __instance.ParentModule.PrefabName);
            Logger.LogLine("[GenericPopup_HandleEnterKeypress_PREFIX] Disable EnterKeyPress");
            return false;
        }
    }
}
