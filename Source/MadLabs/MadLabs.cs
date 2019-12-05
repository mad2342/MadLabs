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
