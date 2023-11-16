using System;
using System.IO;
using System.Threading.Tasks;

namespace PrepareStella.Scripts.Preparing
{
    /// <summary>
    ///     Deletes the ReShade cache files and displays the space saved.
    /// </summary>
    internal static class DeleteReShadeCache
    {
        private const long KilobyteInBytes = 1024;
        private const long MegabyteInBytes = 1024 * KilobyteInBytes;

        public static async Task RunAsync()
        {
            int deletedFilesCount = 0;
            long savedSpace = 0;
            string cacheDirectoryPath = Path.Combine(Program.ResourcesGlobal, "ReShade", "Cache");

            if (Directory.Exists(cacheDirectoryPath))
            {
                string[] files = Directory.GetFiles(cacheDirectoryPath);
                foreach (string filePath in files)
                {
                    FileInfo file = new FileInfo(filePath);
                    savedSpace += file.Length;
                    await DeleteFileAsync(filePath);
                    deletedFilesCount++;
                }
            }
            else
            {
                Console.WriteLine(@"Creating cache folder...");
                Directory.CreateDirectory(cacheDirectoryPath);
            }

            if (deletedFilesCount > 0)
            {
                string spaceUnit = savedSpace > MegabyteInBytes ? "MB" : "KB";
                double spaceSaved = savedSpace / (double)(savedSpace > MegabyteInBytes ? MegabyteInBytes : KilobyteInBytes);
                Console.WriteLine($@"Deleted {deletedFilesCount} cache files and saved {spaceSaved:F2} {spaceUnit}.");
            }
            else
            {
                Console.WriteLine(@"No cache files found to delete.");
            }
        }

        private static async Task DeleteFileAsync(string filePath)
        {
            try
            {
                await Task.Run(() => File.Delete(filePath));
                Start.Logger.Info($"Deleted cache file: {filePath}");
            }
            catch (Exception ex)
            {
                Start.Logger.Info($"Error deleting file {filePath}: {ex.Message}");
            }
        }
    }
}
