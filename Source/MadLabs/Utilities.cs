﻿using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace MadLabs
{
    class Utilities
    {
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
