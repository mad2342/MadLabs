using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BattleTech;
using BattleTech.Data;
using HBS.Collections;

namespace MadLabs
{
    class Utilities
    {
        public static int[] GetMaxAllowedContractDifficultyVariances(SimGameState.SimGameType gameMode, TagSet companyTags)
        {
            //Logger.LogLine("[Utilities.GetMaxAllowedContractDifficultyVariance] companyTags: " + companyTags);

            if (gameMode == SimGameState.SimGameType.CAREER)
            {
                return new int[] { 1, 1 };
            }

            //SimGameState.SimGameType.KAMEA_CAMPAIGN
            if (companyTags.Contains("story_complete"))
            {
                return new int[] { 6, 3 };
            }
            else if (companyTags.Contains("oc09_post_damage_report"))
            {
                return new int[] { 4, 2 };
            }
            else if (companyTags.Contains("oc04_post_argo"))
            {
                return new int[] { 2, 1 };
            }
            else
            {
                return new int[] { 1, 1 };
            }
        }

        // Deprecated
        public static int GetMaxAllowedContractDifficultyVariance(SimGameState.SimGameType gameMode, TagSet companyTags)
        {
            Logger.LogLine("[Utilities.GetMaxAllowedContractDifficultyVariance] companyTags: " + companyTags);
            if (companyTags.Contains("story_complete"))
            {
                return 6;
            }
            else if (companyTags.Contains("oc09_post_damage_report"))
            {
                return 4;
            }
            else if (companyTags.Contains("oc04_post_argo"))
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        public static int GetMaxAllowedMadlabsUnitsByProgression(float globalDifficulty)
        {
            // A global difficulty of 9+ is only possible with setting "Enemy Force Strength" to "Hard" in Game Options
            int d = (int)globalDifficulty;

            if (d >= 9)
            {
                return 3;
            }
            else if (d >= 7)
            {
             return 2;
            }
            else if (d >= 4)
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
            if (companyTags.Contains("story_complete") || daysPassed > 900)
            {
                return 3;
            }
            else if (companyTags.Contains("oc09_post_damage_report") || daysPassed > 450)
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

        public static string GetMechDefIdBasedOnSameChassis(string chassisID, int threatLevel, DataManager dm)
        {
            string replacementMechDefId = "";

            Logger.LogLine("[Utilities.GetMechDefIdBasedOnSameChassis] Get replacement for chassisID: " + chassisID + " and threatLevel: " + threatLevel);

            // Shortcut for STOCK
            if (threatLevel == 0)
            {
                Logger.LogLine("[Utilities.GetMechDefIdBasedOnSameChassis] Requested threatlevel is 0. Returning STOCK variant...");
                return chassisID.Replace("chassisdef", "mechdef");
            }

            string mechTagForThreatLevel = Utilities.GetMechTagForThreatLevel(threatLevel);
            Logger.LogLine("[Utilities.GetMechDefIdBasedOnSameChassis] mechTagForThreatLevel: " + mechTagForThreatLevel);

            List<MechDef> allMechDefs = new List<MechDef>();
            foreach (string key in dm.MechDefs.Keys)
            {
                MechDef mechDef = dm.MechDefs.Get(key);
                allMechDefs.Add(mechDef);
            }

            List<string> mechDefIdsBasedOnSameChassis = allMechDefs
                .Where(mechDef => mechDef.ChassisID == chassisID)
                .Where(mechDef => mechDef.MechTags.Contains(mechTagForThreatLevel))
                .Select(mechDef => mechDef.Description.Id)
                .ToList();

            foreach (string Id in mechDefIdsBasedOnSameChassis)
            {
                Logger.LogLine("[Utilities.GetMechDefIdBasedOnSameChassis] mechDefIdsBasedOnSameChassis(" + chassisID + "): " + Id);
            }

            if (mechDefIdsBasedOnSameChassis.Count > 0)
            {
                mechDefIdsBasedOnSameChassis.Shuffle<string>();
                replacementMechDefId = mechDefIdsBasedOnSameChassis[0];
                Logger.LogLine("[Utilities.GetMechDefIdBasedOnSameChassis] replacementMechDefId: " + replacementMechDefId);
            }
            else
            {
                Logger.LogLine("[Utilities.GetMechDefIdBasedOnSameChassis] Couldn't find a replacement. Falling back to STOCK...");
                replacementMechDefId = chassisID.Replace("chassisdef", "mechdef");
            }

            return replacementMechDefId;
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

        // Deprecated
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

        public static string GetPilotTypeForMechDef(MechDef mechDef, bool random = false)
        {
            string pilotType = "lancer";
            TagSet MechTags = mechDef.MechTags;
            List<string> availablePilotTypes = new List<string>() { "skirmisher", "lancer", "sharpshooter", "flanker", "outrider", "recon", "gladiator", "brawler", "sentinel", "striker", "scout", "vanguard" };
            List<string> appropiatePilotTypes = new List<string>();

            if(random)
            {
                Random rnd = new Random();
                int r = rnd.Next(availablePilotTypes.Count);

                Logger.LogLine("[Utilities.GetPilotTypeForMechDef] Returning random pilotType: " + availablePilotTypes[r]);
                return availablePilotTypes[r];
            }

            if (!MechTags.IsEmpty)
            {
                Logger.LogLine("[Utilities.GetPilotTypeForMechDef] MechTags: " + MechTags);

                // unit_lance_support, unit_lance_tank, unit_lance_assassin, unit_lance_vanguard
                // unit_role_brawler, unit_role_sniper, unit_role_scout
                // skirmisher, lancer, sharpshooter, flanker, outrider, recon, gladiator, brawler, sentinel, striker, scout, vanguard

                // By logical combination
                if (MechTags.Contains("unit_lance_support") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilotTypes.Add("vanguard");
                }
                if (MechTags.Contains("unit_lance_support") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilotTypes.Add("sharpshooter");
                }
                if (MechTags.Contains("unit_lance_support") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilotTypes.Add("recon");
                }

                if (MechTags.Contains("unit_lance_tank") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilotTypes.Add("gladiator");
                }
                if (MechTags.Contains("unit_lance_tank") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilotTypes.Add("lancer");
                }
                if (MechTags.Contains("unit_lance_tank") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilotTypes.Add("outrider");
                }

                if (MechTags.Contains("unit_lance_assassin") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilotTypes.Add("brawler");
                }
                if (MechTags.Contains("unit_lance_assassin") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilotTypes.Add("skirmisher");
                }
                if (MechTags.Contains("unit_lance_assassin") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilotTypes.Add("flanker");
                }

                if (MechTags.Contains("unit_lance_vanguard") && MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilotTypes.Add("scout");
                }
                if (MechTags.Contains("unit_lance_vanguard") && MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilotTypes.Add("striker");
                }
                if (MechTags.Contains("unit_lance_vanguard") && MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilotTypes.Add("sentinel");
                }

                // Add variety by single roles
                if (MechTags.Contains("unit_role_brawler"))
                {
                    appropiatePilotTypes.Add("lancer");
                    appropiatePilotTypes.Add("skirmisher");
                }
                if (MechTags.Contains("unit_role_sniper"))
                {
                    appropiatePilotTypes.Add("sharpshooter");
                }
                if (MechTags.Contains("unit_role_scout"))
                {
                    appropiatePilotTypes.Add("recon");
                    appropiatePilotTypes.Add("scout");
                }

                // Add variety by special tags
                if (MechTags.Contains("unit_indirectFire"))
                {
                    appropiatePilotTypes.Add("vanguard");
                    appropiatePilotTypes.Add("striker");
                }

                // Add variety by Chassis
                if (mechDef.ChassisID.Contains("hatchetman") || mechDef.ChassisID.Contains("dragon") || mechDef.ChassisID.Contains("banshee"))
                {
                    appropiatePilotTypes.Add("brawler");
                    appropiatePilotTypes.Add("gladiator");
                }
            }

            if (appropiatePilotTypes.Count > 0)
            {
                foreach (string Type in appropiatePilotTypes)
                {
                    Logger.LogLine("[Utilities.GetPilotTypeForMechDef] appropiatePilotTypes: " + Type);
                }
                appropiatePilotTypes.Shuffle<string>();
                pilotType = appropiatePilotTypes[0];
            }
            Logger.LogLine("[Utilities.GetPilotTypeForMechDef] Selected pilotType: " + pilotType);

            return pilotType;
        }

        public static int GetPilotSkillLevel(TagSet pilotTagSet)
        {
            if (pilotTagSet.Contains("pilot_npc_d10"))
            {
                return 10;
            }
            if (pilotTagSet.Contains("pilot_npc_d9"))
            {
                return 9;
            }
            if (pilotTagSet.Contains("pilot_npc_d8"))
            {
                return 8;
            }
            if (pilotTagSet.Contains("pilot_npc_d7"))
            {
                return 7;
            }
            if (pilotTagSet.Contains("pilot_npc_d6"))
            {
                return 6;
            }
            if (pilotTagSet.Contains("pilot_npc_d5"))
            {
                return 5;
            }
            if (pilotTagSet.Contains("pilot_npc_d4"))
            {
                return 4;
            }
            if (pilotTagSet.Contains("pilot_npc_d3"))
            {
                return 3;
            }
            if (pilotTagSet.Contains("pilot_npc_d2"))
            {
                return 2;
            }
            if (pilotTagSet.Contains("pilot_npc_d1"))
            {
                return 1;
            }
            // Default
            return 7;
        }

        public static string BuildPilotDefIdFromSkillAndSpec(int skillLevel, string pilotSpecialization)
        {
            return "pilot_d" + skillLevel + "_" + pilotSpecialization;
        }

        public static string GetPilotIdForMechDef(MechDef mechDef, string currentPilotDefId, TagSet currentPilotTagSet, int threatLevel, float globalDifficulty)
        {
            // If no replacement is appropiate fall back to original PilotDef
            string replacementPilotDefId = currentPilotDefId;
            int currentSkillLevel = Utilities.GetPilotSkillLevel(currentPilotTagSet);
            Logger.LogLine("[Utilities.GetPilotIdForMechDef] currentSkillLevel" + currentSkillLevel);

            int requestedSkillLevel = 0;
            switch (threatLevel)
            {
                case 0:
                    requestedSkillLevel = (int)globalDifficulty;
                    break;
                case 1:
                    requestedSkillLevel = 9;
                    break;
                case 2:
                    requestedSkillLevel = 10;
                    break;
                case 3:
                    requestedSkillLevel = 11;
                    break;
            }
            Logger.LogLine("[Utilities.GetPilotIdForMechDef] requestedSkillLevel: " + requestedSkillLevel);

            // Specialization starts at difficulty of 7
            if (requestedSkillLevel > 7 && requestedSkillLevel > currentSkillLevel)
            {
                string pilotSpecialization = Utilities.GetPilotTypeForMechDef(mechDef);
                Logger.LogLine("[Utilities.GetPilotIdForMechDef] pilotSpecialization: " + pilotSpecialization);
                replacementPilotDefId = Utilities.BuildPilotDefIdFromSkillAndSpec(requestedSkillLevel, pilotSpecialization);
            }

            Logger.LogLine("[Utilities.GetPilotIdForMechDef] replacementPilotDefId: " + replacementPilotDefId);
            return replacementPilotDefId;
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
