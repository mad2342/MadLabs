using HBS.Collections;



namespace MechLabAmendments
{
    public static class Fields
    {
        public static string CurrentLanceName = "mla_default";
        public static int CurrentLanceMadlabsUnitCount = 0;
        public static int MaxAllowedExtraThreatLevelByProgression = 0;
        public static int MaxAllowedMadlabsUnitsPerLance = 0;

        // @ToDo: MaxThreat and/or MaxUnits per Contract?
        //public static int MaxAllowedExtraThreatLevelPerContract = 0;
        //public static int MaxAllowedMadlabsUnitsPerContract = 0;

        public static float KeepInitialRareWeaponChanceMin = 0.02f; //2%

        public static bool IsMechLabActive = false;
    }
}
