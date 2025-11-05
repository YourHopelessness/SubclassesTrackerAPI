using ClosedXML.Excel;

namespace SubclassesTracker.Api.ExcelServices
{
    public partial class ExcelExporterService
    {
        /// <summary>
        /// Merges all Excel files in a folder into a single workbook.
        /// Each sheet from the input files is copied to the output workbook.
        /// </summary>
        /// <param name="inputFolder">Path to folder with .xlsx files.</param>
        /// <param name="outputPath">Full path to merged output file.</param>
        public static void MergeExcels(string inputFolder, string outputPath)
        {
            if (!Directory.Exists(inputFolder))
                throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");

            var files = Directory.GetFiles(inputFolder, "*.xlsx", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
                throw new FileNotFoundException($"No Excel files found in {inputFolder}");

            using var mergedWb = new XLWorkbook();

            foreach (var file in files)
            {
                using var wb = new XLWorkbook(file);
                foreach (var ws in wb.Worksheets)
                {
                    var newName = ws.Name;
                    var counter = 1;

                    // If a sheet with the same name exists, append a counter to make it unique
                    while (mergedWb.Worksheets.Any(s => s.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        counter++;
                        newName = $"{ws.Name} ({counter})";
                    }

                    ws.CopyTo(mergedWb, newName);
                }
            }

            mergedWb.SaveAs(outputPath);
        }
    }
}
