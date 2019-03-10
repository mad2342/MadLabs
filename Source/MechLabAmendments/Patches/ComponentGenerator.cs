using BattleTech;
using Harmony;

namespace MechLabAmendments.Patches
{
    [HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    public static class SimGameState__OnAttachUXComplete_Patch
    {
        public static bool Prepare()
        {
            return MechLabAmendments.EnableComponentGenerator;
        }

        public static void Postfix(SimGameState __instance, StatCollection ___companyStats)
        {
            string[] MechsToAdd = new string[] { "mechdef_cyclops_CP-10-Q" };
            string[] WeaponsToAdd = new string[] { "Weapon_Gauss_Gauss_1-M7", "Weapon_Gauss_Gauss_2-M9" };
            string[] UpgradesToAdd = new string[] { "Gear_Gyro_Friedhof_Sparrow" };
            string[] HeatsinksToAdd = new string[] { "Gear_HeatSink_Generic_Double" };

            foreach (string Id in MechsToAdd)
            {
                __instance.AddMechByID(Id, true);
            }
            foreach (string Id in WeaponsToAdd)
            {
                int num = 5;
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
                int num = 3;
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
                int num = 5;
                int i = 0;
                while (i < num)
                {
                    __instance.AddItemStat(Id, typeof(HeatSinkDef), false);
                    i++;
                }
                Logger.LogLine("[SimGameState__OnAttachUXComplete_POSTFIX] Added " + Id + "(" + num + ") to inventory.");
            }
        }
    }
}
