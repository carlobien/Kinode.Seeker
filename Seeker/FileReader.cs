using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seeker
{
    public static class FileReader
    {
        public static List<string> GetDataFileLines(string fileName)
        {
            var list = new List<string>();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName);

            if (File.Exists(path))
            {
                var lines = File.ReadLines(path);

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line)
                        && !line.StartsWith("#"))
                    {
                        list.Add(line.Trim());
                    }
                }
            }

            return list.Distinct().ToList();
        }

        public static void ReadTextFile(string filePath, List<string> matchingEntries)
        {
            try
            {
                var entries = new List<string>();

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        entries.AddRange(checkMatchingEntries(line, filePath, matchingEntries));
                    }
                }

                if (entries.Any())
                {
                    FileLogger.LogReport(entries.Distinct().ToList());
                }
            }
            catch (Exception e)
            {
                FileLogger.LogError(e.Message, filePath);
            }
        }

        public static void ReadWordFile(string filePath, List<string> matchingEntries)
        {
            try
            {
                var entries = new List<string>();

                var bodyContent = string.Empty;

                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(
                    Package.Open(filePath, FileMode.Open, FileAccess.Read)))
                {
                    var body = wordDocument.MainDocumentPart.Document.Body;
                    if (body != null)
                    {
                        bodyContent = body.InnerText;
                    }
                }

                if (!string.IsNullOrEmpty(bodyContent))
                {
                    entries.AddRange(checkMatchingEntries(bodyContent, filePath, matchingEntries));
                }

                if (entries.Any())
                {
                    FileLogger.LogReport(entries.Distinct().ToList());
                }
            }
            catch (Exception e)
            {
                FileLogger.LogError(e.Message, filePath);
            }
        }

        public static void ReadSpreadsheetFile(string filePath, List<string> matchingEntries)
        {
            try
            {
                var entries = new List<string>();
                //var entriesLock = new Object();

                using (SpreadsheetDocument document = SpreadsheetDocument.Open(
                    Package.Open(filePath, FileMode.Open, FileAccess.Read)))
                {
                    var sheets = document.WorkbookPart.Workbook.Descendants<Sheet>().ToList();

                    foreach (var sheet in sheets)
                    {
                        var worksheet = ((WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id)).Worksheet;
                        var allRows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                        foreach (Row currentRow in allRows)
                        {
                            var allCells = currentRow.Descendants<Cell>();

                            Parallel.ForEach(allCells,
                                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                               (currentCell, currentCellLoopState) =>
                               {
                                   var cellContent = string.Empty;

                                   var currentCellValue = currentCell.GetFirstChild<CellValue>();
                                   if (currentCellValue != null)
                                   {
                                       cellContent = currentCellValue.Text;
                                   }

                                   if (!string.IsNullOrEmpty(cellContent))
                                   {
                                       //lock (entriesLock)
                                       //{
                                           
                                       //}

                                       entries.AddRange(checkMatchingEntries(cellContent, filePath, matchingEntries));
                                   }
                               });
                        }
                    }
                }

                if (entries.Any())
                {
                    FileLogger.LogReport(entries.Distinct().ToList());
                }
            }
            catch (Exception e)
            {
                FileLogger.LogError(e.Message, filePath);
            }
        }

        private static List<string> checkMatchingEntries(string content, string filePath, List<string> matchingEntries)
        {
            var result = new List<string>();

            Parallel.ForEach(matchingEntries,
               new ParallelOptions { MaxDegreeOfParallelism = 10 },
               (matchingEntry, matchingEntryLoopState) =>
               {
                   if (content.ToLower().Contains(matchingEntry.ToLower()))
                   {
                       result.Add(string.Format("'{0}' found in {1}",
                           matchingEntry,
                           filePath));
                   }
               });

            return result;
        }
    }
}
