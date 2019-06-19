using System;
using System.Runtime.InteropServices;

namespace CommonLib.WinAPI
{
    public static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 CancelIo(IntPtr hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr securityAttributes, Boolean bManualReset, Boolean bInitialState, string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean GetOverlappedResult(IntPtr hFile, IntPtr lpOverlapped, ref Int32 lpNumberOfBytesTransferred, Boolean bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean ReadFile(IntPtr hFile, IntPtr lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 WaitForSingleObject(IntPtr hHandle, Int32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean WriteFile(IntPtr hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToWrite, ref Int32 lpNumberOfBytesWritten, IntPtr lpOverlapped);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr h);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        public static extern int LoadLibrary(
            [MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddress(int hModule,
            [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        public static extern bool FreeLibrary(int hModule);


        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }

        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);


        public const int INVALID_HANDLE_VALUE = -1;
        public const int PAGE_READWRITE = 0x04;
        //共享内存
        [DllImport("Kernel32.dll", EntryPoint = "CreateFileMapping")]
        public static extern IntPtr CreateFileMapping(IntPtr hFile, //HANDLE hFile,
         UInt32 lpAttributes,//LPSECURITY_ATTRIBUTES lpAttributes,  //0
         UInt32 flProtect,//DWORD flProtect
         UInt32 dwMaximumSizeHigh,//DWORD dwMaximumSizeHigh,
         UInt32 dwMaximumSizeLow,//DWORD dwMaximumSizeLow,
         string lpName//LPCTSTR lpName
         );

        [DllImport("Kernel32.dll", EntryPoint = "OpenFileMapping")]
        public static extern IntPtr OpenFileMapping(
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess,
         int bInheritHandle,//BOOL bInheritHandle,
         string lpName//LPCTSTR lpName
         );

        public const int FILE_MAP_ALL_ACCESS = 0x0002;
        public const int FILE_MAP_WRITE = 0x0002;

        [DllImport("Kernel32.dll", EntryPoint = "MapViewOfFile")]
        public static extern IntPtr MapViewOfFile(
         IntPtr hFileMappingObject,//HANDLE hFileMappingObject,
         UInt32 dwDesiredAccess,//DWORD dwDesiredAccess
         UInt32 dwFileOffsetHight,//DWORD dwFileOffsetHigh,
         UInt32 dwFileOffsetLow,//DWORD dwFileOffsetLow,
         UInt32 dwNumberOfBytesToMap//SIZE_T dwNumberOfBytesToMap
         );

        [DllImport("Kernel32.dll", EntryPoint = "UnmapViewOfFile")]
        public static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

    }
}

