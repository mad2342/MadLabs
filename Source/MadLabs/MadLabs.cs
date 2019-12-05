using System.Reflection;
using System.IO;
using Harmony;
using Newtonsoft.Json;
using System;

namespace MadLabs
{
    public class MadLabs
    {
        internal static string LogPath;
        internal static string ModDirectory;
        internal static Settings Settings;

        // BEN: Debug (0: nothing, 1: errors, 2:all)
        internal static int DebugLevel = 2;

        public static void Init(string directory, string settings)
        {
            ModDirectory = directory;

            LogPath = Path.Combine(ModDirectory, "MadLabs.log");
            File.CreateText(MadLabs.LogPath);

            try
            {
                Settings = JsonConvert.DeserializeObject<Settings>(settings);
            }
            catch (Exception)
            {
                Settings = new Settings();
            }

            var harmony = HarmonyInstance.Create("de.mad.MadLabs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
