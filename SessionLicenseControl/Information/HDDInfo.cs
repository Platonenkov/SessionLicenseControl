using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SessionLicenseControl.Information
{
    public static class HDDInfo
    {
        [Flags]
        public enum FileSystemFeature : uint
        {
            /// <summary>
            /// The file system supports case-sensitive file names.
            /// </summary>
            CaseSensitiveSearch = 1,
            /// <summary>
            /// The file system preserves the case of file names when it places a name on disk.
            /// </summary>
            CasePreservedNames = 2,
            /// <summary>
            /// The file system supports Unicode in file names as they appear on disk.
            /// </summary>
            UnicodeOnDisk = 4,
            /// <summary>
            /// The file system preserves and enforces access control lists (ACL).
            /// </summary>
            PersistentACLS = 8,
            /// <summary>
            /// The file system supports file-based compression.
            /// </summary>
            FileCompression = 0x10,
            /// <summary>
            /// The file system supports disk quotas.
            /// </summary>
            VolumeQuotas = 0x20,
            /// <summary>
            /// The file system supports sparse files.
            /// </summary>
            SupportsSparseFiles = 0x40,
            /// <summary>
            /// The file system supports re-parse points.
            /// </summary>
            SupportsReparsePoints = 0x80,
            /// <summary>
            /// The specified volume is a compressed volume, for example, a DoubleSpace volume.
            /// </summary>
            VolumeIsCompressed = 0x8000,
            /// <summary>
            /// The file system supports object identifiers.
            /// </summary>
            SupportsObjectIDs = 0x10000,
            /// <summary>
            /// The file system supports the Encrypted File System (EFS).
            /// </summary>
            SupportsEncryption = 0x20000,
            /// <summary>
            /// The file system supports named streams.
            /// </summary>
            NamedStreams = 0x40000,
            /// <summary>
            /// The specified volume is read-only.
            /// </summary>
            ReadOnlyVolume = 0x80000,
            /// <summary>
            /// The volume supports a single sequential write.
            /// </summary>
            SequentialWriteOnce = 0x100000,
            /// <summary>
            /// The volume supports transactions.
            /// </summary>
            SupportsTransactions = 0x200000,
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVolumeInformation
        (
            string strPathName,
            StringBuilder strVolumeNameBuffer,
            int lngVolumeNameSize,
            out uint lngVolumeSerialNumber,
            out uint lngMaximumComponentLength,
            out FileSystemFeature lngFileSystemFlags,
            StringBuilder strFileSystemNameBuffer,
            int lngFileSystemNameSize
        );

        public static uint GetSerialNimber(string drive) => new DriveInfo(drive).GetSerialNimber();

        public static uint GetSerialNimber(this DriveInfo drive)
        {
            const int MAX_SIZE = 0x105;
            var vol_name = new StringBuilder(MAX_SIZE);
            var sys_name = new StringBuilder(MAX_SIZE);
            uint vol_serial, max_comp_count;
            FileSystemFeature system_flags;
            GetVolumeInformation
            (
                drive.Name,
                vol_name, MAX_SIZE,
                out vol_serial,
                out max_comp_count,
                out system_flags,
                sys_name, MAX_SIZE
            );
            return vol_serial;
        }

        public static void Test()
        {
            var drive = new DriveInfo("c:\\");
            var sn = drive.GetSerialNimber();
            Console.WriteLine(sn);
        }
    }
}
