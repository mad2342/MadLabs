using System;
using Harmony;
using System.Linq;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;

namespace MadLabs.Patches
{
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
                // Rare upgrades will still be added to salvage by Vanillas Replacement-RNG on anything non-weapon
                if (def.ComponentType == ComponentType.Upgrade)
                {
                    Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] Component is an upgrade, skipping original method (BUT STILL calling Postfixes if existent!)");
                    return false;
                }

                // Only touch weapons for now
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
                SimGameConstants simGameConstants = simGameState.Constants;
                int contractDifficulty = __instance.Override.finalDifficulty;
                // Borrowed from Contract.AddWeaponToSalvage()
                float keepInitialRareWeaponChance = ((float)contractDifficulty + simGameConstants.Salvage.VeryRareWeaponChance) / simGameConstants.Salvage.WeaponChanceDivisor;
                
                // Leave a minimum chance
                keepInitialRareWeaponChance = keepInitialRareWeaponChance < Fields.KeepInitialRareWeaponChanceMin ? Fields.KeepInitialRareWeaponChanceMin : keepInitialRareWeaponChance;
                Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] keepInitialRareWeaponChance: " + keepInitialRareWeaponChance);

                float keepInitialRareWeaponRoll = simGameState.NetworkRandom.Float(0f, 1f);
                bool keepInitialRareWeapon = keepInitialRareWeaponRoll < keepInitialRareWeaponChance;

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

                // Set
                def = ___dataManager.WeaponDefs.Get(weaponStockId);
                Logger.LogLine("[Contract_AddMechComponentToSalvage_PREFIX] (" + weaponOriginalId + ") was replaced with stock version (" + def.Description.Id + ")");

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return true;
            }
        }

        // Check
        public static void Postfix(Contract __instance, MechComponentDef def, List<SalvageDef> ___finalPotentialSalvage)
        {
            try
            {
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

    // Normalize MechParts for Salvage (Replacing custom MechDefs with Stock) 
    // Otherwise each different MechDef gets their own MechPart-Stack in the Mechbay...
    [HarmonyPatch(typeof(Contract), "CreateAndAddMechPart")]
    public static class Contract_CreateAndAddMechPart_Patch
    {
        public static void Prefix(Contract __instance, ref MechDef m, DataManager ___dataManager)
        {
            try
            {
                if(!m.MechTags.Contains("unit_madlabs"))
                {
                    return;
                }
                else
                {
                    Logger.LogLine("[Contract_CreateAndAddMechPart_PREFIX] Handling MechPart of MechDef (" + m.Description.Id + ") which belongs to a custom MadLabs unit. Normalizing back to STOCK...");

                    //SimGameState simGameState = __instance.BattleTechGame.Simulation;
                    string currentMechDefId = m.Description.Id;
                    string stockMechDefId = m.ChassisID.Replace("chassisdef", "mechdef");
                    
                    // Replace
                    m = ___dataManager.MechDefs.Get(stockMechDefId);
                    Logger.LogLine("[Contract_CreateAndAddMechPart_PREFIX] MechDef (" + currentMechDefId + ") was replaced with stock version (" + m.Description.Id + ")");
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }
}

