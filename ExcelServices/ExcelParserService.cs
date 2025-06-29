using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using SubclassesTrackerExtension.Models;

namespace SubclassesTrackerExtension.ExcelServices
{
    public class ExcelParserService
    {
        /// <summary>
        /// Loads skill lines from an Excel file and returns a dictionary mapping skill line names to their respective skill IDs.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary<string, SkillInfo> LoadSkills(string path)
        {
            using var wb = new XLWorkbook(path);
            var ws = wb.Worksheets.First();

            var dict = new Dictionary<string, SkillInfo>();

            // headers
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                dict[row.Cell(2).GetString().Trim()] = new SkillInfo
                {
                    Name = row.Cell(2).GetString().Trim(),
                    Line = row.Cell(3).GetString().Trim()
                };
            }
            return dict;
        }
        public sealed record SkillInfo { public string Name = ""; public string Line = ""; }

        public static void ExportToExcel(
            IEnumerable<SkillLineReportModel> data,
            string path)
        {
            XLWorkbook wb = File.Exists(path)
                   ? new XLWorkbook(path)
                   : new XLWorkbook();

            foreach (var trial in data)
            {
                var sheetName = trial.TrialName.Length > 30
                    ? trial.TrialName[..30]
                    : trial.TrialName;

                if (wb.Worksheets.TryGetWorksheet(sheetName, out var oldWs))
                    oldWs.Delete();

                var ws = wb.Worksheets.Add(sheetName);

                ws.Cell(1, 1).Value = "Role";
                ws.Cell(1, 2).Value = "Skill Line";
                ws.Cell(1, 3).Value = "Players";
                ws.Cell(1, 4).Value = "Unique skills";
                ws.Range("A1:D1").Style.Font.Bold = true;

                int row = 2;
                void Dump(IEnumerable<SkillLinesModel> src, string role)
                {
                    foreach (var l in src)
                    {
                        ws.Cell(row, 1).Value = role;
                        ws.Cell(row, 2).Value = l.LineName;
                        ws.Cell(row, 3).Value = l.PlayersUsingThisLine;
                        ws.Cell(row, 4).Value = l.UniqueSkillsCount;
                        row++;
                    }
                }

                Dump(trial.DdLinesModels, "DD");
                Dump(trial.HealersLinesModels, "Healer");
                Dump(trial.TanksLinesModels, "Tank");

                ws.Columns("A:D").AdjustToContents();
                ws.SheetView.FreezeRows(1);
            }

            wb.SaveAs(path);
        }
    }
}
