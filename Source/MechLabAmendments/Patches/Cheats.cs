using BattleTech;
using Harmony;

namespace MechLabAmendments.Patches
{
    /*
    [HarmonyPatch(typeof(SimGameState), "_OnAttachUXComplete")]
    public static class SimGameState__OnAttachUXComplete_Patch
    {
        public static void Postfix(SimGameState __instance, StatCollection ___companyStats)
        {
            Logger.LogLine("[SimGameState__OnAttachUXComplete_POSTFIX] Adding stuff.");

            string[] MechsToAdd = new string[] { "mechdef_cyclops_CP-10-Q", "mechdef_trebuchet_TBT-5N" };
            string[] WeaponsToAdd = new string[] { "Weapon_Gauss_Gauss_1-M7", "Weapon_Gauss_Gauss_2-M9" };


            foreach (string Id in MechsToAdd)
            {
                __instance.AddMechByID(Id, true);
            }
            foreach (string Id in WeaponsToAdd)
            {
                int num = 10;
                int i = 0;
                while (i < num)
                {
                    __instance.AddItemStat(Id, typeof(WeaponDef), false);
                    i++;
                }
            }
        }
    }
    */
}
