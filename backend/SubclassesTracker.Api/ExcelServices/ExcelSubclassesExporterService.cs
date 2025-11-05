using ClosedXML.Excel;
using SubclassesTracker.Models.Responses.Api;
using SubclassesTracker.Models.Responses.Esologs;

namespace SubclassesTracker.Api.ExcelServices
{
    public partial class ExcelExporterService
    {
        private static readonly Lock SubclassesFileLock = new(); // For thread-safe file access

        /// <summary>
        /// Appends (or creates) a worksheet for a single zone into an Excel file.
        /// Thread-safe for concurrent background updates.
        /// </summary>
        public static void ExportTrialSheet(
            SkillLineReportEsologsResponse trial,
            string filePath)
        {
            if (trial is null) return;

            lock (SubclassesFileLock)
            {
                using var wb = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook();

                var sheetName = trial.TrialName.Length > 30 ? trial.TrialName[..30] : trial.TrialName;

                if (wb.Worksheets.TryGetWorksheet(sheetName, out var oldWs))
                    oldWs.Delete();

                var ws = wb.Worksheets.Add(sheetName);

                ws.Cell(1, 1).Value = "Role";
                ws.Cell(1, 2).Value = "Skill Line";
                ws.Cell(1, 3).Value = "Players";
                ws.Range("A1:C1").Style.Font.Bold = true;

                var row = 2;
                void Dump(IEnumerable<SkillLinesApiResponse>? src, string role)
                {
                    if (src is null) return;
                    foreach (var l in src)
                    {
                        ws.Cell(row, 1).Value = role;
                        ws.Cell(row, 2).Value = l.LineName;
                        ws.Cell(row, 3).Value = l.PlayersUsingThisLine;
                        row++;
                    }
                }

                Dump(trial.DdLinesModels, "DD");
                Dump(trial.HealersLinesModels, "Healer");
                Dump(trial.TanksLinesModels, "Tank");

                ws.Columns("A:C").AdjustToContents();
                ws.SheetView.FreezeRows(1);

                wb.SaveAs(filePath);
            }
        }

        public static void ExportAllZonesSheet(
            SkillLineReportEsologsResponse summary,
            string filePath)
            => ExportTrialSheet(summary, filePath);
    }
}
