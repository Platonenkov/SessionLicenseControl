using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathCore.Annotations;

namespace SessionLicenseControl.Extensions
{
    public static  class ZipFileExtensions
    {
        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="data">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover data if need</param>
        /// <param name="EntryFileName">name of file in archive</param>
        /// <returns>path where file was saved</returns>
        [NotNull]
        public static string SaveToZipFile<T>([NotNull] this T data, [NotNull] string FilePath, string EntryFileName, string secret)
        {
            var file = new FileInfo(FilePath);
            var row = data.EncryptToRow(secret);
            file.AddToZip(EntryFileName, row);
            return FilePath;
        }

        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="data">data to save it</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <param name="EntryFileName">name of file in archive</param>
        /// <returns>path where file was saved</returns>
        [ItemNotNull]
        public static async Task<string> SaveToZipFileAsync<T>([NotNull] this T data, [NotNull] string FilePath, string EntryFileName, string secret)
        {
            var file = new FileInfo(FilePath);
            var row = data.EncryptToRow(secret);
            await file.AddToZipAsync(EntryFileName, row);
            return FilePath;
        }

        public static void AddToZip(this FileInfo ArchiveFile, string EntryFileName, string data)
        {
            var time_out_count = 0;
            while (ArchiveFile.IsLocked() && time_out_count < 100)
            {
                Task.Delay(300);
                time_out_count++;
            }

            using var zip_stream = ArchiveFile.Open(FileMode.OpenOrCreate);
            using var archive = new ZipArchive(zip_stream, ZipArchiveMode.Update);
            var file = archive.Entries.FirstOrDefault(e => e.FullName == EntryFileName);
            file?.Delete();

            file = archive.CreateEntry(EntryFileName, CompressionLevel.Optimal);

            using StreamWriter writer = new StreamWriter(file.Open(), Encoding.UTF8);

            writer.Write(data);
        }
        public static async Task AddToZipAsync(this FileInfo ArchiveFile, string EntryFileName, string data)
        {
            var time_out_count = 0;
            while (ArchiveFile.IsLocked() && time_out_count < 100)
            {
                await Task.Delay(300);
                time_out_count++;
            }

            await using var zip_stream = ArchiveFile.Open(FileMode.OpenOrCreate);
            using var archive = new ZipArchive(zip_stream, ZipArchiveMode.Update);
            var file = archive.Entries.FirstOrDefault(e => e.FullName == EntryFileName);
            file?.Delete();

            file = archive.CreateEntry(EntryFileName, CompressionLevel.Optimal);

            await using StreamWriter writer = new StreamWriter(file.Open(), Encoding.UTF8);

            await writer.WriteAsync(data);
        }
        /// <summary>
        /// Get a lot of data from archive
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="ArchiveFile">archive file</param>
        /// <param name="FileExtension">data file extensions</param>
        /// <param name="Secret">secret string if there was</param>
        /// <returns></returns>
        [ItemNotNull]
        public static List<T> GetAllFromZip<T>(this FileInfo ArchiveFile, string FileExtension, string Secret)
        {
            var sessions = new List<T>();

            // open zip
            using var zip_stream = ArchiveFile.OpenRead();

            using var archive = new ZipArchive(zip_stream, ZipArchiveMode.Read);

            // iterate through zipped objects
            foreach (var archive_entry in archive.Entries.Where(e => e.Name.EndsWith(FileExtension)))
            {
                if (!archive_entry.Name.EndsWith(FileExtension))
                {
                    continue;
                }

                var data = archive_entry.Open().ReadToEndAsString();
                var day = data.DecryptRow<T>(Secret);
                sessions.Add(day);
            }

            return sessions;
        }

        /// <summary>
        /// Get a lot of data from archive
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="ArchiveFile">archive file</param>
        /// <param name="FileExtension">data file extensions</param>
        /// <param name="Secret">secret string if there was</param>
        /// <returns></returns>
        [ItemNotNull]
        public static async Task<List<T>> GetAllFromZipAsync<T>(this FileInfo ArchiveFile, string FileExtension, string Secret)
        {
            var sessions = new List<T>();

            // open zip
            await using var zip_stream = ArchiveFile.OpenRead();

            using var archive = new ZipArchive(zip_stream, ZipArchiveMode.Read);

            // iterate through zipped objects
            foreach (var archive_entry in archive.Entries.Where(e=>e.Name.EndsWith(FileExtension)))
            {
                if (!archive_entry.Name.EndsWith(FileExtension))
                {
                    continue;
                }

                var data = await archive_entry.Open().ReadToEndAsStringAsync(Encoding.UTF8);
                var day = data.DecryptRow<T>(Secret);
                sessions.Add(day);
            }

            return sessions;
        }
        /// <summary>
        /// Get a data from archive
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="ArchiveFile">archive file</param>
        /// <param name="EntryName">data file name</param>
        /// <param name="Secret">secret string if there was</param>
        /// <returns></returns>
        [NotNull]
        public static T GetFromZip<T>(this FileInfo ArchiveFile, string EntryName, string Secret)
        {
            // open zip
            using var zip_stream = ArchiveFile.OpenRead();

            using var archive = new ZipArchive(zip_stream, ZipArchiveMode.Read);
            var file = archive.Entries.FirstOrDefault(e => e.Name == EntryName);
            if (file is null) return default!;

            var data = file.Open().ReadToEndAsString();
            return data.DecryptRow<T>(Secret);
        }
        /// <summary>
        /// Get a data from archive
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="ArchiveFile">archive file</param>
        /// <param name="EntryName">data file name</param>
        /// <param name="Secret">secret string if there was</param>
        /// <returns></returns>
        [ItemNotNull]
        public static async Task<T> GetFromZipAsync<T>(this FileInfo ArchiveFile, string EntryName, string Secret)
        {
            // open zip
            await using var zip_stream = ArchiveFile.OpenRead();

            using var archive = new ZipArchive(zip_stream, ZipArchiveMode.Read);
            var file = archive.Entries.FirstOrDefault(e => e.Name == EntryName);
            if (file is null) return default!;

            var data = await file.Open().ReadToEndAsStringAsync(Encoding.UTF8);
            return data.DecryptRow<T>(Secret);
        }

    }
}
