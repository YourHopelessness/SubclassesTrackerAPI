using SubclassesTrackerExtension.Models;
using System.IO.Compression;
using System.IO.Packaging;

namespace SubclassesTrackerExtension.Utils
{
    public class ZipHelper
    {
        private const long BUFFER_SIZE = 4096;

        public static byte[] GenerateDataCollectionZipArchive(DataCollectionResultModel dataCollectionResult)
        {
            byte[] all_stats = dataCollectionResult.LinesStats;
            byte[] stats_with_score = dataCollectionResult.LinesStatsWithScore;

            using MemoryStream ms = new();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                var zipArchiveEntry = archive.CreateEntry("all_stats.xlsx", CompressionLevel.Fastest);
                using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(all_stats, 0, all_stats.Length);

                zipArchiveEntry = archive.CreateEntry("stats_with_score.xlsx", CompressionLevel.Fastest);
                using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(stats_with_score, 0, stats_with_score.Length);
            }
            return ms.ToArray();
        }

        public static void AddFileToZip(string zipFilename, string fileToAdd)
        {
            using Package zip = Package.Open(zipFilename, FileMode.OpenOrCreate);
            string destFilename = ".\\" + Path.GetFileName(fileToAdd);

            Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
            if (zip.PartExists(uri))
            {
                zip.DeletePart(uri);
            }
            PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);

            using FileStream fileStream = new(fileToAdd, FileMode.Open, FileAccess.Read);
            using Stream dest = part.GetStream();
            CopyStream(fileStream, dest);
        }

        private static void CopyStream(FileStream inputStream, Stream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bytesRead;
            }
        }
    }
}
