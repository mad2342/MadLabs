using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;
using HBS.Collections;

namespace MechLabAmendments
{
    class Utilities
    {
        public static int GetMaxAllowedMadlabsUnitsByProgression(float globalDifficulty)
        {
            int d = (int)globalDifficulty;

            if (d >= 9)
            {
                return 3;
            }
            else if (d >= 7)
            {
             return 2;
            }
            else if (d >= 5)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static int GetMaxAllowedExtraThreatLevelByProgression(int daysPassed, TagSet companyTags)
        {
            if (companyTags.Contains("oc12_post_itrom_attack,") || daysPassed > 900)
            {
                return 3;
            }
            else if (companyTags.Contains("oc08_post_unearthed_secrets") || daysPassed > 450)
            {
                return 2;
            }
            else if (companyTags.Contains("oc04_post_argo") || daysPassed > 150)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static string GetMechTagForThreatLevel(int threatLevel)
        {
            switch(threatLevel)
            {
                case 1:
                    return "unit_components_plus";
                case 2:
                    return "unit_components_plusplus";
                case 3:
                    return "unit_components_plusplusplus";
                default:
                    return "unit_components_neutral";
            }
        }


        public static int GetExtraThreatLevelFromMechDef(MechDef mechDef, bool log = false)
        {
            int result = 0;
            MechComponentRef[] mechDefInventory = mechDef.Inventory;

            // Ignore fixed components, add up only weapons and upgrades (ignore MGs)
            MechComponentRef[] mechDefInventoryFiltered = mechDefInventory
                .Where(component => component.ComponentDefType == ComponentType.Weapon || component.ComponentDefType == ComponentType.Upgrade)
                .Where(component => component.Def.PrefabIdentifier != "MachineGun")
                .Where(component => component.IsFixed != true)
                .ToArray();

            int weaponCount = mechDefInventoryFiltered
                .Where(component => component.ComponentDefType == ComponentType.Weapon)
                .Where(component => component.IsFixed != true)
                .ToArray().Length;
            int upgradeCount = mechDefInventoryFiltered
                .Where(component => component.ComponentDefType == ComponentType.Upgrade)
                .Where(component => component.IsFixed != true)
                .ToArray().Length;
            int componentCount = weaponCount + upgradeCount;

            // Get count of *all* plus weapons AND individual counts
            int componentCountVariant = mechDefInventoryFiltered
                .Where(component => component.Def.ComponentTags.Contains("component_type_variant") || component.Def.ComponentTags.Contains("component_type_lostech"))
                .ToArray()
                .Length;
            int componentCountVariant1 = mechDefInventoryFiltered
                .Where(component => component.Def.ComponentTags.Contains("component_type_variant1"))
                .ToArray()
                .Length;
            int componentCountVariant2 = mechDefInventoryFiltered
                .Where(component => component.Def.ComponentTags.Contains("component_type_variant2"))
                .ToArray()
                .Length;
            int componentCountVariant3 = mechDefInventoryFiltered
                .Where(component => component.Def.ComponentTags.Contains("component_type_variant3") || component.Def.ComponentTags.Contains("component_type_lostech"))
                .ToArray()
                .Length;

            if (log)
            {
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] componentCount: " + componentCount);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] componentCountVariant: " + componentCountVariant);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] componentCountVariant1: " + componentCountVariant1);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] componentCountVariant2: " + componentCountVariant2);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] componentCountVariant3: " + componentCountVariant3);
            }

            // Threat ranges
            Range<int> neutralRange = new Range<int>(componentCount, (int)(componentCount * 1.5));
            Range<int> plus1Range = new Range<int>((int)(componentCount * 1.5 + 1), (int)(componentCount * 2.5));
            Range<int> plus2Range = new Range<int>((int)(componentCount * 2.5 + 1), (int)(componentCount * 3.5));
            Range<int> plus3Range = new Range<int>((int)(componentCount * 3.5 + 1), (int)(componentCount * 4));

            if (log)
            {
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] neutralRange: " + neutralRange);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] plus1Range: " + plus1Range);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] plus2Range: " + plus2Range);
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] plus3Range: " + plus3Range);
            }

            // Simple threat classification: Stock gives 1 point, every + adds another
            int componentClassification = (componentCount - componentCountVariant) + (componentCountVariant1 * 2) + (componentCountVariant2 * 3) + (componentCountVariant3 * 4);

            if (log)
            {
                Logger.LogLine("[Utilities.GetExtraThreatLevelFromMechDef] componentClassification: " + componentClassification);
            }

            if (plus1Range.ContainsValue(componentClassification))
            {
                result = 1;
            }
            else if (plus2Range.ContainsValue(componentClassification))
            {
                result = 2;
            }
            else if (plus3Range.ContainsValue(componentClassification))
            {
                result = 3;
            }

            return result;
        }

        public static string GetMechTagAccordingToExtraThreatlevel(int threatLevel)
        {
            switch(threatLevel)
            {
                case 3:
                    return "unit_components_plusplusplus";
                case 2:
                    return "unit_components_plusplus";
                case 1:
                    return "unit_components_plus";
                default:
                    return "unit_components_neutral";
            }
        }

        // Deprecated
        public static string TagMechDefAccordingToInventory(MechComponentRef[] mechDefInventory)
        {
            string result = "unit_components_neutral";

            // Ignore fixed components, add up weapons and upgrades
            int weaponCount = mechDefInventory
                .Where(component => component.ComponentDefType == ComponentType.Weapon)
                .Where(component => component.IsFixed != true)
                .ToArray().Length;
            int upgradeCount = mechDefInventory
                .Where(component => component.ComponentDefType == ComponentType.Upgrade)
                .Where(component => component.IsFixed != true)
                .ToArray().Length;
            int componentCount = weaponCount + upgradeCount;

            // Get count of *all* plus weapons AND individual counts
            int componentCountVariant = mechDefInventory.Where(component => component.Def.ComponentTags.Contains("component_type_variant")).ToArray().Length;
            int componentCountVariant1 = mechDefInventory.Where(component => component.Def.ComponentTags.Contains("component_type_variant1")).ToArray().Length;
            int componentCountVariant2 = mechDefInventory.Where(component => component.Def.ComponentTags.Contains("component_type_variant2")).ToArray().Length;
            int componentCountVariant3 = mechDefInventory.Where(component => component.Def.ComponentTags.Contains("component_type_variant3")).ToArray().Length;

            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] componentCount: " + componentCount);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] componentCountVariant: " + componentCountVariant);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] componentCountVariant1: " + componentCountVariant1);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] componentCountVariant2: " + componentCountVariant2);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] componentCountVariant3: " + componentCountVariant3);

            // Threat ranges
            Range<int> neutralRange = new Range<int>(componentCount, (int)(componentCount * 1.5));
            Range<int> plus1Range = new Range<int>((int)(componentCount * 1.5 + 1), (int)(componentCount * 2.5));
            Range<int> plus2Range = new Range<int>((int)(componentCount * 2.5 + 1), (int)(componentCount * 3.5));
            Range<int> plus3Range = new Range<int>((int)(componentCount * 3.5 + 1), (int)(componentCount * 4));

            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] neutralRange: " + neutralRange);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] plus1Range: " + plus1Range);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] plus2Range: " + plus2Range);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] plus3Range: " + plus3Range);

            // Simple threat classification: Stock gives 1 point, every + adds another
            int componentClassification = (componentCount - componentCountVariant) + (componentCountVariant1 * 2) + (componentCountVariant2 * 3) + (componentCountVariant3 * 4);
            Logger.LogLine("[Utilities.TagMechDefAccordingToInventory] componentClassification: " + componentClassification);

            if (plus1Range.ContainsValue(componentClassification))
            {
                result = "unit_components_plus";
            }
            else if (plus2Range.ContainsValue(componentClassification))
            {
                result = "unit_components_plusplus";
            }
            else if (plus3Range.ContainsValue(componentClassification))
            {
                result = "unit_components_plusplusplus";
            }

            return result;
        }



        public static string GetPilotIdForMechDef(MechDef mechDef)
        {
            string pilotId = "pilot_madlabs_lancer";
            TagSet MechTags = mechDef.MechTags;
            List<string> appropiatePilots = new List<string>();

            if (!MechTags.IsEmpty)
            {
                Logger.LogLine("[Utilities.GetPilotIdForMechDef] MechTags: " + MechTags);

                // unit_lance_support, unit_lance_tank, unit_lance_assassin, unit_lance_vanguard
                // unit_role_brawler, unit_role_sniper, unit_role_scout
                // skirmisher, lancer, sharpshooter, flanker, outrider, recon, gladiator, brawler, sentinel, striker, scout, vanguard

                // By logical combination
                if (MechTags.Contains("unit_lance_support") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilots.Add("pilot_madlabs_vanguard");
                }
                if (MechTags.Contains("unit_lance_support") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilots.Add("pilot_madlabs_sharpshooter");
                }
                if (MechTags.Contains("unit_lance_support") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilots.Add("pilot_madlabs_recon");
                }

                if (MechTags.Contains("unit_lance_tank") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilots.Add("pilot_madlabs_gladiator");
                }
                if (MechTags.Contains("unit_lance_tank") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilots.Add("pilot_madlabs_lancer");
                }
                if (MechTags.Contains("unit_lance_tank") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilots.Add("pilot_madlabs_outrider");
                }

                if (MechTags.Contains("unit_lance_assassin") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilots.Add("pilot_madlabs_brawler");
                }
                if (MechTags.Contains("unit_lance_assassin") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilots.Add("pilot_madlabs_skirmisher");
                }
                if (MechTags.Contains("unit_lance_assassin") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilots.Add("pilot_madlabs_flanker");
                }

                if (MechTags.Contains("unit_lance_vanguard") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilots.Add("pilot_madlabs_scout");
                }
                if (MechTags.Contains("unit_lance_vanguard") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilots.Add("pilot_madlabs_striker");
                }
                if (MechTags.Contains("unit_lance_vanguard") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilots.Add("pilot_madlabs_sentinel");
                }

                // Add variety by single roles
                if (MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilots.Add("pilot_madlabs_brawler");
                }
                if (MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilots.Add("pilot_madlabs_sharpshooter");
                }
                if (MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilots.Add("pilot_madlabs_recon");
                    appropiatePilots.Add("pilot_madlabs_scout");
                }

                // Add variety by special tags
                if (MechTags.Contains("unit_indirectFire"))
                {
                    appropiatePilots.Add("pilot_madlabs_vanguard");
                    appropiatePilots.Add("pilot_madlabs_striker");
                }

                // Add variety by Chassis
                if (mechDef.ChassisID.Contains("hatchetman") || mechDef.ChassisID.Contains("dragon") || mechDef.ChassisID.Contains("banshee"))
                {
                    appropiatePilots.Add("pilot_madlabs_brawler");
                    appropiatePilots.Add("pilot_madlabs_gladiator");
                }
            }

            if (appropiatePilots.Count > 0)
            {
                foreach (string Id in appropiatePilots)
                {
                    Logger.LogLine("[Utilities.GetPilotIdForMechDef] appropiatePilots: " + Id);
                }
                appropiatePilots.Shuffle<string>();
                pilotId = appropiatePilots[0];
            }
            Logger.LogLine("[Utilities.GetPilotIdForMechDef] Selected pilotId: " + pilotId);

            return pilotId;
        }



        public static List<InventoryItemElement_Simple> ComponentsToInventoryItems(List<MechComponentRef> mechComponents, bool log = false)
        {
            List<InventoryItemElement_Simple> mechInventoryItems = new List<InventoryItemElement_Simple>();

            foreach (MechComponentRef component in mechComponents)
            {
                if (log)
                {
                    Logger.LogLine("[Utilities.ComponentsToInventory] mechComponents contains: " + component.ComponentDefID);
                }

                if (mechInventoryItems.Exists(x => x.ComponentRef.ComponentDefID == component.ComponentDefID))
                {
                    mechInventoryItems.Find(x => x.ComponentRef.ComponentDefID == component.ComponentDefID).ModifyQuantity(1);
                }
                else
                {
                    InventoryItemElement_Simple item = new InventoryItemElement_Simple();
                    item.ComponentRef = component;
                    item.Quantity = 1;
                    mechInventoryItems.Add(item);
                }
            }

            if (log)
            {
                foreach (InventoryItemElement_Simple item in mechInventoryItems)
                {
                    Logger.LogLine("[Utilities.ComponentsToInventory] mechInventoryItems contains: " + item.ComponentRef.ComponentDefID + "(" + item.Quantity + ")");
                }
            }

            return mechInventoryItems;
        }
    }
}
