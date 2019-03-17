using System;
using System.IO;

namespace MadLabs
{
    public class Logger
    {
        static string filePath = $"{MadLabs.ModDirectory}/MadLabs.log";
        public static void LogError(Exception ex)
        {
            if (MadLabs.DebugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[MadLabs @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace + "" + Environment.NewLine);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        public static void LogLine(String line, bool separator = false)
        {
            if (MadLabs.DebugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[MadLabs @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine(prefix + line);

                    if(separator)
                    {
                        writer.WriteLine("---");
                    }
                }
            }
        }
    }
}
