namespace MadLabs
{
    public static class Fields
    {
        public static string CurrentLanceName = "mla_default";
        public static int CurrentLanceMadlabsUnitCount = 0;
        public static int MaxAllowedExtraThreatLevelByProgression = 0;
        public static int MaxAllowedMadlabsUnitsPerLance = 0;

        public static int MaxAllowedPlusPlusPlusUnitsPerContract = 1;
        public static int CurrentContractPlusPlusPlusUnits = 0;

        public static int MaxAllowedTotalThreatLevelPerContract = 0;
        public static int CurrentContractTotalThreatLevel = 0;

        public static int GlobalDifficulty = -1;

        public static float KeepInitialRareWeaponChanceMin = 0.02f; //2%

        public static bool IsMechLabActive = false;
    }
}
