using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Kinode.Seeker
{
    public static class FileLogger
    {
        private static readonly string ReportPath = ConfigurationManager.AppSettings["ReportPath"];

        public static void LogReport(List<string> entries)
        {
            try
            {
                if (!Directory.Exists(ReportPath))
                {
                    Directory.CreateDirectory(ReportPath);
                }

                var filePath = string.Format(@"{0}\Report - {1} {2}.txt",
                    ReportPath,
                    Environment.MachineName,
                    DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT));

                using (var sw = new StreamWriter(filePath, true))
                {
                    foreach (var entry in entries)
                    {
                        sw.WriteLine(DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT) + " " + entry);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }

        public static void LogError(string error, string extra = "")
        {
            try
            {
                if (!Directory.Exists(ReportPath))
                {
                    Directory.CreateDirectory(ReportPath);
                }

                var filePath = string.Format(@"{0}\Error - {1} {2}.txt",
                   ReportPath,
                   Environment.MachineName,
                   DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT));

                using (var sw = new StreamWriter(filePath, true))
                {
                    var line = string.Format("{0} {1}{2}",
                        DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT),
                        error,
                        !string.IsNullOrEmpty(extra)
                            ? " " + extra : string.Empty);

                    sw.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }
    }
}
