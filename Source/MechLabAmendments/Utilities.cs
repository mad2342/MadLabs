using System.Collections.Generic;
using BattleTech;
using Harmony;
using HBS.Collections;

namespace MechLabAmendments
{
    class Utilities
    {
        public static string GetPilotIdForMechDef(MechDef mechDef)
        {
            string pilotId = "pilot_madlabs_lancer";
            // return "pilot_madlabs_lancer"
            TagSet MechTags = mechDef.MechTags;
            List<string> appropiatePilots = new List<string>();
            if (!MechTags.IsEmpty)
            {
                string[] MechTagsItems = (string[])AccessTools.Field(typeof(TagSet), "items").GetValue(MechTags);
                foreach (string Tag in MechTagsItems)
                {
                    Logger.LogLine("[Utilities.GetPilotIdForMechDef] MechTagsItem: " + Tag);
                }

                // unit_lance_support, unit_lance_tank, unit_lance_assassin, unit_lance_vanguard
                // unit_role_brawler, unit_role_sniper, unit_role_scout
                // skirmisher, lancer, sharpshooter, flanker, outrider, recon, gladiator, brawler, sentinel, striker, scout, vanguard

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
            }

            foreach (string Id in appropiatePilots)
            {
                Logger.LogLine("[Utilities.GetPilotIdForMechDef] appropiatePilot: " + Id);
            }
            appropiatePilots.Shuffle<string>();
            pilotId = appropiatePilots[0];
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
