using System;
using System.IO;



namespace MechLabAmendments
{
    public class Logger
    {
        static string filePath = $"{MechLabAmendments.ModDirectory}/MechLabAmendments.log";
        public static void LogError(Exception ex)
        {
            if (MechLabAmendments.DebugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[MechLabAmendments @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace + "" + Environment.NewLine);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        public static void LogLine(String line, bool separator = false)
        {
            if (MechLabAmendments.DebugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[MechLabAmendments @ " + DateTime.Now.ToString() + "]";
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
