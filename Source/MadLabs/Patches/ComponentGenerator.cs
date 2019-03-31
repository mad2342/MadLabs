using BattleTech;
using Harmony;

namespace MadLabs.Patches
{
    [HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    public static class SimGameState__OnAttachUXComplete_ComponentGenerator
    {
        public static bool Prepare()
        {
            return MadLabs.EnableComponentGenerator;
        }

        public static void Postfix(SimGameState __instance, StatCollection ___companyStats)
        {
            string[] MechsToAdd = new string[] { "mechdef_orion_ON2-Mb" };
            string[] WeaponsToAdd = new string[] { "Weapon_Gauss_Gauss_0-STOCK" };
            string[] UpgradesToAdd = new string[] { "Gear_TargetingTrackingSystem_RCA_InstaTrac-XII" };
            string[] HeatsinksToAdd = new string[] { "Gear_HeatSink_Generic_Double" };
            string[] AmmoToAdd = new string[] { "Ammo_AmmunitionBox_Generic_GAUSS" };
            int fundsToAdd = 1000000;
            int amount = 3;

            foreach (string Id in MechsToAdd)
            {
                __instance.AddMechByID(Id, true);
            }
            foreach (string Id in WeaponsToAdd)
            {
                int num = amount;
                int i = 0;
                while (i < num)
                {
                    __instance.AddItemStat(Id, typeof(WeaponDef), false);
                    i++;
                }
                Logger.LogLine("[SimGameState__OnAttachUXComplete_POSTFIX] Added " + Id + "(" + num + ") to inventory.");
            }
            foreach (string Id in UpgradesToAdd)
            {
                int num = amount;
                int i = 0;
                while (i < num)
                {
                    __instance.AddItemStat(Id, typeof(UpgradeDef), false);
                    i++;
                }
                Logger.LogLine("[SimGameState__OnAttachUXComplete_POSTFIX] Added " + Id + "(" + num + ") to inventory.");
            }
            foreach (string Id in HeatsinksToAdd)
            {
                int num = amount;
                int i = 0;
                while (i < num)
                {
                    __instance.AddItemStat(Id, typeof(HeatSinkDef), false);
                    i++;
                }
                Logger.LogLine("[SimGameState__OnAttachUXComplete_POSTFIX] Added " + Id + "(" + num + ") to inventory.");
            }
            foreach (string Id in AmmoToAdd)
            {
                int num = amount;
                int i = 0;
                while (i < num)
                {
                    __instance.AddItemStat(Id, typeof(AmmunitionBoxDef), false);
                    i++;
                }
                Logger.LogLine("[SimGameState__OnAttachUXComplete_POSTFIX] Added " + Id + "(" + num + ") to inventory.");
            }

            // Funds
            __instance.AddFunds(fundsToAdd, null, true);
        }
    }
}
