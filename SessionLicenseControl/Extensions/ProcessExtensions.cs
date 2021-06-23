using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedMethodReturnValue.Global

namespace System.Diagnostics
{
    public static class ProcessExtensions
    {
        public static Task<int> WaitAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);
            if (process.HasExited) tcs.TrySetResult(process.ExitCode);
            return tcs.Task;
        }

        public static Task<int> WaitAsync(this Process process, CancellationToken Cancel)
        {
            if (Cancel.IsCancellationRequested) return Task.FromCanceled<int>(Cancel);

            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);
            if (process.HasExited) tcs.TrySetResult(process.ExitCode);
            else if (Cancel.CanBeCanceled)
                Cancel.Register(cancel => tcs.TrySetCanceled((CancellationToken)cancel), Cancel);

            return tcs.Task;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public uint ExitStatus;
            public IntPtr PebBaseAddress; // Zero if 32 bit process try get info about 64 bit process 
            public IntPtr AffinityMask;
            public int BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        [DllImport("ntdll.dll", SetLastError = true, ExactSpelling = true)]
        private static extern uint NtQueryInformationProcess(
            IntPtr ProcessHandle,
            uint ProcessInformationClass,
            ref PROCESS_BASIC_INFORMATION ProcessInformation,
            int ProcessInformationLength,
            out int ReturnLength
        );

        public static Process GetMotherProcess(this Process process)
        {
            var info = new PROCESS_BASIC_INFORMATION();
            return NtQueryInformationProcess(process.Handle, 0, ref info, Marshal.SizeOf(info), out var writed) != 0 || writed == 0
                ? throw new Win32Exception(Marshal.GetLastWin32Error())
                : Process.GetProcessById(info.InheritedFromUniqueProcessId.ToInt32());
        }
    }
}