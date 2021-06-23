using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathCore.Annotations;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    public static class FileInfoExtensions
    {
        public static IEnumerable<string> GetLines(this FileInfo file, IProgress<double> Progress = null)
        {
            using var reader = file.OpenText();
            double length = reader.BaseStream.Length;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                Progress?.Report(reader.BaseStream.Position / length);
                yield return line;
            }
        }

        //public static async IAsyncEnumerable<string> ReadLines(this FileInfo file)
        //{
        //    using (var reader = file.OpenText())
        //        while (!reader.EndOfStream)
        //            yield return await reader.ReadLineAsync();
        //}

        //public static Process ExecuteAsAdmin([NotNull] this FileInfo File, [NotNull] string Args = "", bool UseShellExecute = true) =>
        //    File.Execute(Args, UseShellExecute, "runas");

        //[CanBeNull]
        //public static Process Execute([NotNull] this FileInfo File, [NotNull] string Args, bool UseShellExecute, string Verb) =>
        //    Process.Start(new ProcessStartInfo(File.FullName, Args)
        //    {
        //        UseShellExecute = UseShellExecute,
        //        Verb = Verb
        //    });

        //[NotNull]
        //public static Process Execute([NotNull] this FileInfo File, string Args = "", bool UseShellExecute = true) => Process.Start(new ProcessStartInfo(UseShellExecute ? File.ToString() : File.FullName, Args) { UseShellExecute = UseShellExecute });

        [NotNull]
        public static string GetNameWithoutExtension([NotNull] this FileInfo file) => Path.GetFileNameWithoutExtension(file.Name);

        //[NotNull] public static Process ShowInFileExplorer([NotNull] this FileInfo File) => Process.Start("explorer", $"/select,\"{File.FullName}\"");

        [NotNull]
        public static BufferedStream OpenRead([NotNull] this FileInfo File, int BuffeSize) => new(File.OpenRead(), BuffeSize);

        [NotNull]
        private static byte[] ComputeHash([NotNull] this FileInfo file, [NotNull] HashAlgorithm algorithm)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            if (algorithm is null) throw new ArgumentNullException(nameof(algorithm));
            return file.OpenRead().DisposeAfter(algorithm, (stream, alg) => alg.ComputeHash(stream));
        }

        [NotNull]
        public static byte[] ComputeHashSHA256([NotNull] this FileInfo File) => File.ComputeHash(new SHA256Managed());

        [NotNull]
        public static byte[] ComputeHashMD5([NotNull] this FileInfo File) => File.ComputeHash(MD5.Create());

        [NotNull]
        public static string ComputeHashMD5String([NotNull] this FileInfo File) => string.Join(string.Empty, File.ComputeHashMD5().Select(b => b.ToString("X2")));

        [NotNull, ItemNotNull]
        public static Task<byte[]> ComputeHashSHA256Async([NotNull] this FileInfo File, CancellationToken Cancel = default) => File.Async(file => file.ComputeHashSHA256(), Cancel);

        [NotNull, ItemNotNull]
        public static Task<byte[]> ComputeHashMD5Async([NotNull] this FileInfo File, CancellationToken Cancel = default) => File.Async(file => file.ComputeHashMD5(), Cancel);

        [NotNull, ItemNotNull]
        public static async Task<string> ComputeHashMD5StringAsync([NotNull] this FileInfo File) => Convert.ToBase64String(await File.ComputeHashMD5Async().ConfigureAwait(false));

        [CanBeNull]
        public static string ReadText([NotNull] this FileInfo file)
        {
            using var data = file.OpenText();
            return data.ReadToEnd();
        }

        [CanBeNull]
        public static async Task<string> ReadTextAsync([NotNull] this FileInfo file)
        {
            using var data = file.OpenText();
            return await data.ReadToEndAsync().ConfigureAwait(false);
        }

        [CanBeNull]
        public static string ReadText([NotNull] this FileInfo file, [NotNull] Encoding encoding)
        {
            using var reader = new StreamReader(file.OpenRead(), encoding);
            return reader.ReadToEnd();
        }

        [CanBeNull]
        public static async Task<string> ReadTextAsync([NotNull] this FileInfo file, [NotNull] Encoding encoding)
        {
            using var reader = new StreamReader(file.OpenRead(), encoding);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        public static void WriteText([NotNull] this FileInfo file, [CanBeNull] string str)
        {
            using var writer = file.CreateText();
            writer.Write(str);
        }

        public static async Task WriteTextAsync([NotNull] this FileInfo file, [CanBeNull] string str)
        {
            using var writer = file.CreateText();
            await writer.WriteAsync(str).ConfigureAwait(false);
        }

        public static void WriteText([NotNull] this FileInfo file, [CanBeNull] string str, [NotNull] Encoding encoding)
        {
            using var writer = new StreamWriter(file.Create(), encoding);
            writer.Write(str);
        }

        public static async Task WriteTextAsync([NotNull] this FileInfo file, [CanBeNull] string str, [NotNull] Encoding encoding)
        {
            using var writer = new StreamWriter(file.Create(), encoding);
            await writer.WriteAsync(str).ConfigureAwait(false);
        }

        //public static async Task CheckFileAccessAsync([NotNull] this FileInfo File, int Timeout = 1000, int IterationCount = 100, CancellationToken Cancel = default)
        //{
        //    if (File is null) throw new ArgumentNullException(nameof(File));
        //    File.ThrowIfNotFound();
        //    for (var i = 0; i < IterationCount; i++)
        //        try
        //        {
        //            Cancel.ThrowIfCancellationRequested();
        //            using var stream = File.Open(FileMode.Open, FileAccess.Read);
        //            if (stream.Length > 0)
        //                return;
        //        }
        //        catch (IOException)
        //        {
        //            await Task.Delay(Timeout, Cancel).ConfigureAwait(false);
        //        }
        //    Cancel.ThrowIfCancellationRequested();
        //    throw new InvalidOperationException($"Файл {File.FullName} заблокирован другим процессом");
        //}

        ///// <summary>Проверка, что файл существует</summary>
        ///// <param name="File">Проверяемый файл</param>
        ///// <param name="Message">Сообщение, добавляемое в исключение, если файл не найден</param>
        ///// <returns>Файл, гарантированно существующий</returns>
        ///// <exception cref="T:System.IO.FileNotFoundException">В случае если <paramref name="File"/> не существует.</exception>
        //[NotNull]
        //public static FileInfo ThrowIfNotFound([CanBeNull] this FileInfo File, [CanBeNull] string Message = null)
        //{
        //    var file = File.NotNull("Отсутствует ссылка на файл");
        //    if (!file.Exists) throw new FileNotFoundException(Message ?? "Файл не найден", file.FullName);
        //    return file;
        //}

        public static FileInfo MoveTo([NotNull] this FileInfo SourceFile, [NotNull] FileInfo DestinationFile, bool Override = true)
        {
            DestinationFile.Refresh();
            if (DestinationFile.Exists && !Override) return DestinationFile;
            SourceFile.MoveTo(DestinationFile.FullName);
            DestinationFile.Refresh();
            SourceFile.Refresh();
            return DestinationFile;
        }

        [NotNull]
        public static FileInfo MoveTo([NotNull] this FileInfo SourceFile, [NotNull] DirectoryInfo Destination, bool Override = false)
        {
            var destination_file = Destination.CreateFileInfo(SourceFile.Name);
            if (destination_file.Exists)
                if (Override)
                    destination_file.Delete();
                else
                    return destination_file;

            SourceFile.MoveTo(destination_file);
            destination_file.Refresh();
            SourceFile.Refresh();
            return destination_file;
        }

        [NotNull]
        public static FileInfo CopyTo([NotNull] this FileInfo SourceFile, [NotNull] DirectoryInfo Destination)
        {
            var destination_file = Destination.CreateFileInfo(SourceFile.Name);
            if (destination_file.Exists) return destination_file;

            SourceFile.CopyTo(destination_file);
            destination_file.Refresh();
            return destination_file;
        }

        [NotNull]
        public static FileInfo CopyTo([NotNull] this FileInfo SourceFile, [NotNull] DirectoryInfo Destination, bool Override)
        {
            var destination_file = Destination.CreateFileInfo(SourceFile.Name);
            if (destination_file.Exists || !Override) return destination_file;
            SourceFile.MoveTo(destination_file);
            destination_file.Refresh();
            return destination_file;
        }

        [NotNull] public static FileStream Append([NotNull] this FileInfo File) => File.Open(FileMode.Append, FileAccess.Write);

        //[NotNull] public static FileInfo ChangeExtension([NotNull] this FileInfo File, string NewExtension) => new FileInfo(Path.ChangeExtension(File.ParamNotNull(nameof(File)).FullName, NewExtension));

        //[NotNull]
        //public static FileInfo Zip([NotNull] this FileInfo File, string ArchiveFileName = null, bool Override = true)
        //{
        //    if (File is null) throw new ArgumentNullException(nameof(File));
        //    File.ThrowIfNotFound();

        //    if (ArchiveFileName is null) ArchiveFileName = File.FullName + ".zip";
        //    else if (!Path.IsPathRooted(ArchiveFileName))
        //        ArchiveFileName = File.Directory.CreateFileInfo(ArchiveFileName).FullName;
        //    using var zip_stream = IO.File.Open(ArchiveFileName, FileMode.OpenOrCreate, FileAccess.Write);
        //    using var zip = new ZipArchive(zip_stream);
        //    var file_entry = zip.GetEntry(File.Name);
        //    if (file_entry != null)
        //    {
        //        if (!Override) return new FileInfo(ArchiveFileName);
        //        file_entry.Delete();
        //    }

        //    using (var file_entry_stream = zip.CreateEntry(File.Name).Open())
        //    using (var file_stream = File.OpenRead())
        //        file_stream.CopyTo(file_entry_stream);

        //    return new FileInfo(ArchiveFileName);
        //}

        #region  Почучение процессов которыми занят файл

        [DllImport("Rstrtmgr.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern uint RmStartSession(out uint pSessionHandle, uint dwSessionFlags,
            string strSessionKey);

        [DllImport("Rstrtmgr.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern uint RmRegisterResources(uint dwSessionHandle,
            uint nFiles, string[] rgsFilenames, uint nApplications,
            uint rgApplications, uint nServices, uint rgsServiceNames);

        [DllImport("Rstrtmgr.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern uint RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded,
            ref uint pnProcInfo, [In, Out] ProcessInfo[] rgAffectedApps, ref uint lpdwRebootReasons);

        [DllImport("Rstrtmgr.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern uint RmEndSession(uint dwSessionHandle);

        private const uint __LockFileProcessRebootReasonNone = 0x0;
        private const int __LockFileProcessErrorMoreData = 234;

        /// <summary>Получить массив процессов, блокирующих файл</summary>
        /// <param name="file">Файл, который требуется проверить</param>
        /// <returns>Массив процессов, заблокировавших файл</returns>
        [NotNull, ItemNotNull]
        public static Process[] GetLockProcesses([NotNull] this FileSystemInfo file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            if (!file.Exists) return Array.Empty<Process>();

            var path = file.FullName;
            var key = Guid.NewGuid().ToString();
            if (RmStartSession(out var handle, 0, key) != 0)
                throw new InvalidOperationException("Невозможно перезапустить сессию. Невозможно определить блокирующий процесс");

            try
            {
                uint proc_info = 0;
                var reboot_reasons = __LockFileProcessRebootReasonNone;
                var resources = new[] { path };
                if (RmRegisterResources(handle, (uint)resources.Length, resources, 0, 0, 0, 0) != 0)
                    throw new InvalidOperationException("Невозможно определить дескриптор файла в системе");

                switch (RmGetList(handle, out var proc_info_needed, ref proc_info, null, ref reboot_reasons))
                {
                    case 0: return Array.Empty<Process>();
                    case __LockFileProcessErrorMoreData:
                        {
                            var process_info = new ProcessInfo[proc_info_needed];
                            proc_info = proc_info_needed;
                            if (RmGetList(handle, out proc_info_needed, ref proc_info, process_info, ref reboot_reasons) != 0)
                                throw new InvalidOperationException("Невозможно выполнить перечисление процессов, блокирующих файл");

                            var processes = new Process[(int)proc_info];
                            for (var i = 0; i < proc_info; i++)
                                try
                                {
                                    processes[i] = Process.GetProcessById(process_info[i].Process.ProcessId);
                                }
                                catch (ArgumentException) { }

                            return processes;
                        }
                    default:
                        throw new InvalidOperationException("Невозможно выполнить перечисление прцоессов, блокирующих файл",
                   new InvalidOperationException("Невозможно определить размер результата"));
                }
            }
            finally
            {
                RmEndSession(handle);
            }
        }

        /// <summary>Перечисление процессов, блокирующих файл</summary>
        /// <param name="file">Файл, который требуется проверить</param>
        /// <returns>Перечисление процессов, заблокировавших файл</returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<Process> EnumLockProcesses([NotNull] this FileSystemInfo file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));

            if (!file.Exists) yield break;
            var path = file.FullName;
            var key = Guid.NewGuid().ToString();
            if (RmStartSession(out var handle, 0, key) != 0)
                throw new InvalidOperationException("Невозможно перезапустить сессию. Невозможно определить блокирующий процесс");

            try
            {
                uint proc_info = 0;
                var reboot_reasons = __LockFileProcessRebootReasonNone;
                var resources = new[] { path };
                if (RmRegisterResources(handle, (uint)resources.Length, resources, 0, 0, 0, 0) != 0)
                    throw new InvalidOperationException("Невозможно определить дескриптор файла в системе");

                switch (RmGetList(handle, out var proc_info_needed, ref proc_info, null, ref reboot_reasons))
                {
                    case 0: break;
                    case __LockFileProcessErrorMoreData:
                        {
                            var process_info = new ProcessInfo[proc_info_needed];
                            proc_info = proc_info_needed;
                            if (RmGetList(handle, out proc_info_needed, ref proc_info, process_info, ref reboot_reasons) != 0)
                                throw new InvalidOperationException("Невозможно выполнить перечисление процессов, блокирующих файл");

                            foreach (var id in process_info.Select(p => p.Process.ProcessId))
                            {
                                Process process = null;
                                try
                                {
                                    process = Process.GetProcessById(id);
                                }
                                catch (ArgumentException) { }

                                if (process != null)
                                    yield return process;
                            }
                            break;
                        }
                    default:
                        throw new InvalidOperationException("Невозможно выполнить перечисление прцоессов, блокирующих файл",
                            new InvalidOperationException("Невозможно определить размер результата"));
                }
            }
            finally
            {
                RmEndSession(handle);
            }
        }

        public static Task WaitFileLockAsync(this FileInfo file, CancellationToken Cancel = default) => file
           .EnumLockProcesses()
           .Select(process => process.WaitAsync(Cancel))
           .WhenAll();

        public static async Task<bool> WaitFileLockAsync(this FileInfo file, int Timeout, CancellationToken Cancel = default)
        {
            var processes = file.EnumLockProcesses().Select(process => process.WaitAsync(Cancel));
            var process_wait = Task.WhenAll(processes);
            var delay_task = Task.Delay(Timeout, Cancel);
            var task = await Task.WhenAny(process_wait, delay_task).ConfigureAwait(false);
            return task != delay_task;
        }

        /// <summary>Заблокирован ли файл другим процессом?</summary>
        /// <param name="file">Проверяемый файл</param>
        /// <returns>Истина, если файл заблокирован другим процессом</returns>
        public static bool IsLocked([NotNull] this FileInfo file) => file.EnumLockProcesses().Any();

        [StructLayout(LayoutKind.Sequential)]
        private struct UniqueProcess
        {
            // The product identifier (PID). 
            public readonly int ProcessId;
            // The creation time of the process. 
            private readonly Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }
        /// <summary>Describes an application that is to be registered with the Restart Manager</summary> 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct ProcessInfo
        {
            private const int __MaxAppName = 255;
            private const int __MaxSvcName = 63;

            // Contains an UniqueProcess structure that uniquely identifies the 
            // application by its PID and the time the process began. 
            public readonly UniqueProcess Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = __MaxAppName + 1)]
            // If the process is a service, this parameter returns the  
            // long name for the service. 
            private readonly string AppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = __MaxSvcName + 1)]
            // If the process is a service, this is the short name for the service. 
            private readonly string ServiceShortName;
            // Contains an AppType enumeration value. 
            private readonly AppType ApplicationType;
            // Contains a bit mask that describes the current status of the application. 
            private readonly uint AppStatus;
            // Contains the Terminal Services session ID of the process. 
            private readonly uint SessionId;
            // TRUE if the application can be restarted by the  
            // Restart Manager; otherwise, FALSE. 
            [MarshalAs(UnmanagedType.Bool)] private readonly bool Restartable;
        }
        /// <summary> 
        /// Specifies the type of application that is described by 
        /// the ProcessInfo structure. 
        /// </summary> 
        private enum AppType
        {
            // The application cannot be classified as any other type. 
            Unknown = 0,
            // A Windows application run as a stand-alone process that 
            // displays a top-level window. 
            MainWindow = 1,
            // A Windows application that does not run as a stand-alone 
            // process and does not display a top-level window. 
            OtherWindow = 2,
            // The application is a Windows service. 
            Service = 3,
            // The application is Windows Explorer. 
            Explorer = 4,
            // The application is a stand-alone console application. 
            Console = 5,
            // A system restart is required to complete the installation because 
            // a process cannot be shut down. 
            Critical = 1000
        }

        #endregion
    }
}
