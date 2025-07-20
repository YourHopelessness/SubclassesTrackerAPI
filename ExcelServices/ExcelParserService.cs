﻿using ClosedXML.Excel;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;

namespace SubclassesTracker.Api.ExcelServices
{
    public class ExcelParserService
    {
        public static byte[] ExportToExcel(
            IEnumerable<SkillLineReportEsologsResponse> data)
        {
            XLWorkbook wb = new();

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
                void Dump(IEnumerable<SkillLinesApiResponse> src, string role)
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

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return ms.ToArray();
        }
    }
}
