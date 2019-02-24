using System.Reflection;
using System.IO;
using Harmony;

namespace MechLabAmendments
{
    public class MechLabAmendments
    {
        public static string LogPath;
        public static string ModDirectory;

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 2;

        public static void Init(string directory, string settingsJSON)
        {
            ModDirectory = directory;

            LogPath = Path.Combine(ModDirectory, "MechLabAmendments.log");
            File.CreateText(MechLabAmendments.LogPath);

            var harmony = HarmonyInstance.Create("de.mad.MechLabAmendments");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
