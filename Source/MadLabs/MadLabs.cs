using System.Reflection;
using System.IO;
using Harmony;
using System.Collections.Generic;

namespace MadLabs
{
    public class MadLabs
    {
        public static string LogPath;
        public static string ModDirectory;

        internal static bool EnableComponentGenerator = false;
        internal static bool EnableContractGenerator = false;
        internal static bool EnableDynamicContractDifficultyVariance = true;
        internal static List<string> ContractOverrideIDs = new List<string>() { "Assassinate_Headhunt_P", "Assassinate_Headhunt_PP", "Assassinate_Headhunt_PPP" };
        internal static List<string> ContractOverrideNames = new List<string>() { "Precautionary Strike", "Preventive Strike", "Surgical Strike" };

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 2;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;

            LogPath = Path.Combine(ModDirectory, "MadLabs.log");
            File.CreateText(MadLabs.LogPath);

            var harmony = HarmonyInstance.Create("de.mad.MadLabs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
