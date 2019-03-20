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
            string[] MechsToAdd = new string[] { "mechdef_dragon_DRG-1N", "mechdef_griffin_GRF-1S", "mechdef_thunderbolt_TDR-5SE" };
            string[] WeaponsToAdd = new string[] { "Weapon_Gauss_Gauss_0-STOCK", "Weapon_Gauss_Gauss_1-M7", "Weapon_Gauss_Gauss_2-M9" };
            string[] UpgradesToAdd = new string[] { "Gear_Actuator_Coventry_B60-Extended", "Gear_Actuator_Friedhof_Colossus", "Gear_Actuator_Pitban_Kangaroo" };
            string[] HeatsinksToAdd = new string[] { "Gear_HeatSink_Generic_Double", "Gear_HeatSink_Generic_Thermal-Exchanger-III" };
            int fundsToAdd = 10000000;
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

            // Funds
            __instance.AddFunds(fundsToAdd, null, true);
        }
    }
}
