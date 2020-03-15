using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kinode.Seeker
{
    public static class Manager
    {
        private static readonly string[] AllowedExtensions = { ".txt", ".csv", ".sql", ".msg", ".doc", ".docx", ".docm", ".dotm", ".xls", ".xlsx", ".xlsm", ".xltm" };

        public static void Run()
        {
            try
            {
                var matchingEntries = FileReader.GetDataFileLines("MatchingEntries.txt");
                if (!matchingEntries.Any())
                {
                    Console.WriteLine("Missing/Blank MatchingEntries");
                    return;
                }

                var excludeDirectories = new List<string>
                {
                    Directory.GetCurrentDirectory(),
                    ConfigurationManager.AppSettings["ReportPath"]
                };

                var excludedDirectoriesFile = FileReader.GetDataFileLines("ExcludedDirectories.txt");
                if (excludedDirectoriesFile.Any())
                {
                    excludeDirectories.AddRange(excludedDirectoriesFile);
                }

                var wildcardExcludeDirectories = excludeDirectories.Where(x => x.StartsWith("*")).ToList();
                var regularExcludeDirectories = excludeDirectories.Except(wildcardExcludeDirectories).ToList();

                var drives = Directory.GetLogicalDrives();

                foreach (var drive in drives)
                {
                    var fileEntriesToRead = getFileEntriesToRead(drive, regularExcludeDirectories, wildcardExcludeDirectories);

                    var ctr = 1;
                    Console.WriteLine("------------------------------");
                    Console.WriteLine("Retrieved " + fileEntriesToRead.Count + " files in " + drive);

                    foreach (var fileEntry in fileEntriesToRead)
                    {
                        var fileInfo = new FileInfo(fileEntry);

                        Console.WriteLine(string.Format("{0}/{1} Reading: {2} ({3})",
                            ctr, fileEntriesToRead.Count,
                            fileEntry,
                            FileSize.GetSuffix(fileInfo.Length)));

                        if (fileInfo.Extension.Equals(".txt", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".csv", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".sql", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".msg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FileReader.ReadTextFile(fileEntry, matchingEntries);
                        }
                        else if (fileInfo.Extension.Equals(".doc", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".docx", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".docm", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".dotm", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FileReader.ReadWordFile(fileEntry, matchingEntries);
                        }
                        else if (fileInfo.Extension.Equals(".xls", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".xlsx", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".xlsm", StringComparison.InvariantCultureIgnoreCase)
                            || fileInfo.Extension.Equals(".xltm", StringComparison.InvariantCultureIgnoreCase))
                        {
                            FileReader.ReadSpreadsheetFile(fileEntry, matchingEntries);
                        }

                        ctr++;
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.LogError(e.Message);
            }
        }

        public static List<string> getFileEntriesToRead(string drive,
            List<string> regularExcludeDirectories, List<string> wildcardExcludeDirectories)
        {
            var result = new List<string>();

            var fileEntries = getFileEntries(drive, regularExcludeDirectories)
                .Distinct().OrderBy(x => x).ToList();

            foreach (var fileEntry in fileEntries)
            {
                var exclude = false;

                foreach (var wildcardExcludeDirectory in wildcardExcludeDirectories)
                {
                    if (fileEntry.ToLower().Contains(wildcardExcludeDirectory.ToLower().Replace("*", string.Empty)))
                    {
                        exclude = true;
                        break;
                    }
                }

                if (!exclude)
                {
                    result.Add(fileEntry);
                }
            }

            return result;
        }

        private static List<string> getFileEntries(string directoryPath, List<string> regularExcludeDirectories)
        {
            var result = new List<string>();

            try
            {
                result = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(x => AllowedExtensions.Any(x.ToLower().EndsWith)).ToList();

                var directories = new DirectoryInfo(directoryPath)
                    .GetDirectories("*", SearchOption.TopDirectoryOnly).ToList();

                Parallel.ForEach(directories,
                    new ParallelOptions { MaxDegreeOfParallelism = 10 },
                    (directory, loopState) =>
                    {
                        if (!regularExcludeDirectories.Contains(directory.FullName, StringComparer.InvariantCultureIgnoreCase))
                        {
                            result.AddRange(getFileEntries(directory.FullName, regularExcludeDirectories));
                        }
                    });
            }
            catch (Exception e)
            {
                FileLogger.LogError(e.Message);
            }

            return result;
        }
    }
}
