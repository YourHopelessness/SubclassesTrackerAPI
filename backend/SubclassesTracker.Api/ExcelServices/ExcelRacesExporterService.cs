using ClosedXML.Excel;
using SubclassesTracker.Models.Responses.Api;

namespace SubclassesTracker.Api.ExcelServices
{
    public partial class ExcelExporterService
    {
        public static byte[] ExportRacesDataToExcel(List<RacialReportApiResponse> racialReports)
        {
            using var wb = new XLWorkbook();

            foreach (var trial in racialReports)
            {
                var sheetName = trial.TrialName.Length > 30
                    ? trial.TrialName[..30]
                    : trial.TrialName;

                if (wb.Worksheets.TryGetWorksheet(sheetName, out var oldWs))
                    oldWs.Delete();

                var ws = wb.Worksheets.Add(sheetName);

                ws.Cell(1, 1).Value = "Role";
                ws.Cell(1, 2).Value = "Race";
                ws.Cell(1, 3).Value = "Count";

                int row = 2;
                void Dump(IDictionary<string, int> src, string role)
                {
                    foreach (var l in src)
                    {
                        ws.Cell(row, 1).Value = role;
                        ws.Cell(row, 2).Value = l.Key;
                        ws.Cell(row, 3).Value = l.Value;
                        row++;
                    }
                }

                Dump(trial.DdRacesQuantity, "DD");
                Dump(trial.HealerRacesQuantity, "Healer");
                Dump(trial.TankRacesQuantity, "Tank");

                ws.Columns("A:D").AdjustToContents();
                ws.SheetView.FreezeRows(1);
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return ms.ToArray();
        }
    }
}
