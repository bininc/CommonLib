using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CommLiby;

namespace CommonLib.WinAPI
{
    /// <summary>
    /// .NET wrapper for Version Helper functions.
    /// http://msdn.microsoft.com/library/windows/desktop/dn424972.aspx </summary>
    public static class OSVersionHelper
    {
        #region Supplementary data types

        /// <summary>
        /// Operating systems, the information which is stored within
        /// the class <seealso cref="OSVersionHelper"/>.
        /// </summary>
        public enum KnownOS
        {
            /// <summary>
            /// Windows 2000.
            /// </summary>
            [Description("Windows 2000")]
            Windows2000,
            /// <summary>
            /// Windows 2000 SP1.
            /// </summary>
            [Description("Windows 2000 SP1")]
            Windows2000SP1,
            /// <summary>
            /// Windows 2000 SP2.
            /// </summary>
            [Description("Windows 2000 SP2")]
            Windows2000SP2,
            /// <summary>
            /// Windows 2000 SP3.
            /// </summary>
            [Description("Windows 2000 SP3")]
            Windows2000SP3,
            /// <summary>
            /// Windows 2000 SP4.
            /// </summary>
            [Description("Windows 2000 SP4")]
            Windows2000SP4,
            /// <summary>
            /// Windows XP.
            /// </summary>
            [Description("Windows XP")]
            WindowsXP,

            /// <summary>
            /// Windows XP SP1.
            /// </summary>
            [Description("Windows XP SP1")]
            WindowsXPSP1,

            /// <summary>
            /// Windows XP SP2.
            /// </summary>
            [Description("Windows XP SP2")]
            WindowsXPSP2,

            /// <summary>
            /// Windows XP SP3.
            /// </summary>
            [Description("Windows XP SP3")]
            WindowsXPSP3,
            /// <summary>
            /// Windows Server 2003.
            /// </summary>
            [Description("Windows Server 2003", 1)]
            WindowsServer2003,
            /// <summary>
            /// Windows Vista.
            /// </summary>
            [Description("Windows Vista")]
            WindowsVista,
            /// <summary>
            /// Windows Server 2008.
            /// </summary>
            [Description("Windows Server 2008", 1)]
            WindowsServer2008,
            /// <summary>
            /// Windows Vista SP1.
            /// </summary>
            [Description("Windows Vista SP1")]
            WindowsVistaSP1,
            /// <summary>
            /// Windows Server 2008 SP1.
            /// </summary>
            [Description("Windows Server 2008 SP1", 1)]
            WindowsServer2008SP1,
            /// <summary>
            /// Windows Vista SP2.
            /// </summary>
            [Description("Windows Vista SP2")]
            WindowsVistaSP2,
            /// <summary>
            /// Windows Server 2008 SP2.
            /// </summary>
            [Description("Windows Server 2008 SP2", 1)]
            WindowsServer2008SP2,
            /// <summary>
            /// Windows 7.
            /// </summary>
            [Description("Windows 7")]
            Windows7,
            /// <summary>
            /// Windows Server 2008 R2.
            /// </summary>
            [Description("Windows Server 2008 R2", 1)]
            WindowsServer2008R2,
            /// <summary>
            /// Windows 7 SP1.
            /// </summary>
            [Description("Windows 7 SP1")]
            Windows7SP1,
            /// <summary>
            /// Windows Server 2008 R2 SP1.
            /// </summary>
            [Description("Windows Server 2008 R2 SP1", 1)]
            WindowsServer2008R2SP1,
            /// <summary>
            /// Windows 8.
            /// </summary>
            [Description("Windows 8")]
            Windows8,
            /// <summary>
            /// Windows Server 2012.
            /// </summary>
            [Description("Windows Server 2012", 1)]
            WindowsServer2012,
            /// <summary>
            /// Windows 8.1.
            /// </summary>
            [Description("Windows 8.1")]
            Windows8Point1,
            /// <summary>
            /// Windows Server 2012 R2.
            /// </summary>
            [Description("Windows Server 2012 R2", 1)]
            WindowsServer2012R2,
            /// <summary>
            /// Windows 10.
            /// </summary>
            [Description("Windows 10")]
            Windows10,
            /// <summary>
            /// Windows Server 2016.
            /// </summary>
            [Description("Windows Server 2016", 1)]
            WindowsServer2016
        }

        /// <summary>
        /// Information about operating system.
        /// </summary>
        private sealed class OsEntry : IEquatable<OsEntry>
        {
            #region Properties

            /// <summary>
            /// The major version number of the operating system.
            /// </summary>
            public uint MajorVersion { get; private set; }

            /// <summary>
            /// The minor version number of the operating system.
            /// </summary>
            public uint MinorVersion { get; private set; }

            /// <summary>
            /// The major version number of the latest Service Pack installed
            /// on the system. For example, for Service Pack 3, the major
            /// version number is 3. If no Service Pack has been installed,
            /// the value is zero.
            /// </summary>
            public ushort ServicePackMajor { get; private set; }

            /// <summary>
            /// Flag indicating if the running OS matches, or is greater
            /// than, the OS specified with this entry. Should be initialized
            /// with <see cref="VerifyVersionInfo"/> method.
            /// </summary>
            public bool? MatchesOrGreater { get; set; }

            #endregion // Properties

            #region Constructor

            /// <summary>
            /// Creates a new entry of operating system.
            /// </summary>
            /// <param name="majorVersion">The major version number of the
            /// operating system.</param>
            /// <param name="minorVersion">The minor version number of the
            /// operating system.</param>
            /// <param name="servicePackMajor">The major version number of the
            /// latest Service Pack installed on the system. For example, for
            /// Service Pack 3, the major version number is 3. If no Service
            /// Pack has been installed, the value is zero.</param>
            public OsEntry(uint majorVersion, uint minorVersion,
                ushort servicePackMajor)
            {
                this.MajorVersion = majorVersion;
                this.MinorVersion = minorVersion;
                this.ServicePackMajor = servicePackMajor;
            }
            public static OsEntry CurrentOsEntry()
            {
                Version curVersion = Environment.OSVersion.Version;
                return new OsEntry((uint)curVersion.Major, (uint)curVersion.Minor, (ushort)curVersion.MajorRevision);
            }


            #endregion // Constructor

            public bool Equals(OsEntry other)
            {
                if (other == null) return false;

                return MajorVersion == other.MajorVersion && MinorVersion == other.MinorVersion &&
                      ServicePackMajor == other.ServicePackMajor;
            }
        }

        #endregion // Supplementary data types

        #region PInvoke data type declarations

        /// <summary>
        /// Wrapper for OSVERSIONINFOEX structure.
        /// http://msdn.microsoft.com/library/windows/desktop/ms724833.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct OsVersionInfoEx
        {
            /// <summary>
            /// The size of this data structure, in bytes.
            /// </summary>
            public uint OSVersionInfoSize;

            /// <summary>
            /// The major version number of the operating system.
            /// </summary>
            public uint MajorVersion;

            /// <summary>
            /// The minor version number of the operating system.
            /// </summary>
            public uint MinorVersion;

            /// <summary>
            /// The build number of the operating system.
            /// </summary>
            public uint BuildNumber;

            /// <summary>
            /// The operating system platform.
            /// </summary>
            public uint PlatformId;

            /// <summary>
            /// A null-terminated string, such as "Service Pack 3", that
            /// indicates the latest Service Pack installed on the system. If
            /// no Service Pack has been installed, the string is empty.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string CSDVersion;

            /// <summary>
            /// The major version number of the latest Service Pack installed
            /// on the system. For example, for Service Pack 3, the major
            /// version number is 3. If no Service Pack has been installed,
            /// the value is zero.
            /// </summary>
            public ushort ServicePackMajor;

            /// <summary>
            /// The minor version number of the latest Service Pack installed
            /// on the system. For example, for Service Pack 3, the minor
            /// version number is 0.
            /// </summary>
            public ushort ServicePackMinor;

            /// <summary>
            /// A bit mask that identifies the product suites available on the
            /// system, e.g., flags indicating if the operating system is
            /// Datacenter Server or Windows XP Embedded.
            /// </summary>
            public ushort SuiteMask;

            /// <summary>
            /// Any additional information about the system, e.g., flags
            /// indicating if the operating system is a domain controller,
            /// a server or a workstation.
            /// </summary>
            public byte ProductType;

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            public byte Reserved;
        }

        #endregion // PInvoke data type declarations

        #region PInvoke function declarations

        /// <summary>
        /// <para>Wrapper for VerSetConditionMask function (
        /// http://msdn.microsoft.com/library/windows/desktop/ms725493.aspx).
        /// </para>
        /// <para>
        /// Sets the bits of a 64-bit value to indicate the comparison
        /// operator to use for a specified operating system version
        /// attribute. This method is used to build the dwlConditionMask
        /// parameter of the <see cref="VerifyVersionInfo"/> method.
        /// </para>
        /// </summary>
        /// <param name="dwlConditionMask">
        /// <para>A value to be passed as the dwlConditionMask parameter of
        /// the <see cref="VerifyVersionInfo"/> method. The function stores
        /// the comparison information in the bits of this variable.
        /// </para>
        /// <para>
        /// Before the first call to VerSetConditionMask, initialize this
        /// variable to zero. For subsequent calls, pass in the variable used
        /// in the previous call.
        /// </para>
        /// </param>
        /// <param name="dwTypeBitMask">A mask that indicates the member of
        /// the <see cref="OsVersionInfoEx"/> structure whose comparison
        /// operator is being set.</param>
        /// <param name="dwConditionMask">The operator to be used for the
        /// comparison.</param>
        /// <returns>Condition mask value.</returns>
        [DllImport("kernel32.dll")]
        private static extern ulong VerSetConditionMask(ulong dwlConditionMask,
           uint dwTypeBitMask, byte dwConditionMask);

        /// <summary>
        /// <para>
        /// Wrapper for VerifyVersionInfo function (
        /// http://msdn.microsoft.com/library/windows/desktop/ms725492.aspx).
        /// </para>
        /// <para>
        /// Compares a set of operating system version requirements to the
        /// corresponding values for the currently running version of the
        /// system.
        /// </para>
        /// </summary>
        /// <param name="lpVersionInfo">A pointer to an
        /// <see cref="OsVersionInfoEx"/> structure containing the operating
        /// system version requirements to compare.</param>
        /// <param name="dwTypeMask">A mask that indicates the members of the
        /// <see cref="OsVersionInfoEx"/> structure to be tested.</param>
        /// <param name="dwlConditionMask">The type of comparison to be used
        /// for each lpVersionInfo member being compared. Can be constructed
        /// with <see cref="VerSetConditionMask"/> method.</param>
        /// <returns>True if the current Windows OS satisfies the specified
        /// requirements; otherwise, false.</returns>
        [DllImport("kernel32.dll")]
        private static extern bool VerifyVersionInfo(
            [In] ref OsVersionInfoEx lpVersionInfo,
            uint dwTypeMask, ulong dwlConditionMask);

        #endregion // PInvoke declarations

        #region Local fields

        private static Dictionary<KnownOS, OsEntry> osEntries;

        private static bool? isServer = null;

        private static ulong? versionOrGreaterMask;
        private static uint? versionOrGreaterTypeMask;

        #endregion // Local fields

        #region Constructor

        /// <summary>
        /// Initializes dictionary of operating systems.
        /// </summary>
        static OSVersionHelper()
        {
            osEntries = new Dictionary<KnownOS, OsEntry>();
            osEntries.Add(KnownOS.Windows2000, new OsEntry(5, 0, 0));
            osEntries.Add(KnownOS.Windows2000SP1, new OsEntry(5, 0, 1));
            osEntries.Add(KnownOS.Windows2000SP2, new OsEntry(5, 0, 2));
            osEntries.Add(KnownOS.Windows2000SP3, new OsEntry(5, 0, 3));
            osEntries.Add(KnownOS.Windows2000SP4, new OsEntry(5, 0, 4));
            osEntries.Add(KnownOS.WindowsXP, new OsEntry(5, 1, 0));
            osEntries.Add(KnownOS.WindowsXPSP1, new OsEntry(5, 1, 1));
            osEntries.Add(KnownOS.WindowsXPSP2, new OsEntry(5, 1, 2));
            osEntries.Add(KnownOS.WindowsXPSP3, new OsEntry(5, 1, 3));
            osEntries.Add(KnownOS.WindowsServer2003, new OsEntry(5, 2, 0));
            osEntries.Add(KnownOS.WindowsVista, new OsEntry(6, 0, 0));
            osEntries.Add(KnownOS.WindowsServer2008, new OsEntry(6, 0, 0));
            osEntries.Add(KnownOS.WindowsVistaSP1, new OsEntry(6, 0, 1));
            osEntries.Add(KnownOS.WindowsServer2008SP1, new OsEntry(6, 0, 1));
            osEntries.Add(KnownOS.WindowsVistaSP2, new OsEntry(6, 0, 2));
            osEntries.Add(KnownOS.WindowsServer2008SP2, new OsEntry(6, 0, 2));
            osEntries.Add(KnownOS.Windows7, new OsEntry(6, 1, 0));
            osEntries.Add(KnownOS.WindowsServer2008R2, new OsEntry(6, 1, 0));
            osEntries.Add(KnownOS.Windows7SP1, new OsEntry(6, 1, 1));
            osEntries.Add(KnownOS.WindowsServer2008R2SP1, new OsEntry(6, 1, 1));
            osEntries.Add(KnownOS.Windows8, new OsEntry(6, 2, 0));
            osEntries.Add(KnownOS.WindowsServer2012, new OsEntry(6, 2, 0));
            osEntries.Add(KnownOS.Windows8Point1, new OsEntry(6, 3, 0));
            osEntries.Add(KnownOS.WindowsServer2012R2, new OsEntry(6, 3, 0));
            osEntries.Add(KnownOS.Windows10, new OsEntry(10, 0, 0));
            osEntries.Add(KnownOS.WindowsServer2016, new OsEntry(10, 0, 0));
        }

        #endregion // Constructor

        #region Public methods

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the provided version information. This method is useful in
        /// confirming a version of Windows Server that doesn't share a
        /// version number with a client release.
        /// </summary>
        /// <param name="majorVersion">The major OS version number.</param>
        /// <param name="minorVersion">The minor OS version number.</param>
        /// <param name="servicePackMajor">The major Service Pack version
        /// number.</param>
        /// <returns>True if the the running OS matches, or is greater
        /// than, the specified version information; otherwise, false.
        /// </returns>
        public static bool IsWindowsVersionOrGreater(
            uint majorVersion, uint minorVersion, ushort servicePackMajor)
        {
            OsVersionInfoEx osvi = new OsVersionInfoEx();
            osvi.OSVersionInfoSize = (uint)Marshal.SizeOf(osvi);
            osvi.MajorVersion = majorVersion;
            osvi.MinorVersion = minorVersion;
            osvi.ServicePackMajor = servicePackMajor;

            // These constants initialized with corresponding definitions in
            // winnt.h (part of Windows SDK)
            const uint VER_MINORVERSION = 0x0000001;
            const uint VER_MAJORVERSION = 0x0000002;
            const uint VER_SERVICEPACKMAJOR = 0x0000020;
            const byte VER_GREATER_EQUAL = 3;

            if (!versionOrGreaterMask.HasValue)
            {
                versionOrGreaterMask = VerSetConditionMask(
                    VerSetConditionMask(
                        VerSetConditionMask(
                            0, VER_MAJORVERSION, VER_GREATER_EQUAL),
                        VER_MINORVERSION, VER_GREATER_EQUAL),
                    VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);
            }

            if (!versionOrGreaterTypeMask.HasValue)
            {
                versionOrGreaterTypeMask = VER_MAJORVERSION |
                    VER_MINORVERSION | VER_SERVICEPACKMAJOR;
            }

            return VerifyVersionInfo(ref osvi, versionOrGreaterTypeMask.Value,
                versionOrGreaterMask.Value);
        }

        /// <summary>
        /// Indicates if the running OS version matches, or is greater than,
        /// the provided OS.
        /// </summary>
        /// <param name="os">OS to compare running OS to.</param>
        /// <returns>True if the the running OS matches, or is greater
        /// than, the specified OS; otherwise, false.</returns>
        public static bool IsWindowsVersionOrGreater(KnownOS os)
        {
            try
            {
                OsEntry osEntry = osEntries[os];
                if (!osEntry.MatchesOrGreater.HasValue)
                {
                    osEntry.MatchesOrGreater = IsWindowsVersionOrGreater(
                        osEntry.MajorVersion, osEntry.MinorVersion,
                        osEntry.ServicePackMajor);
                }

                return osEntry.MatchesOrGreater.Value;
            }
            catch (KeyNotFoundException e)
            {
                throw new ArgumentException("Unknown operating system. " +
                    "Use overloaded version of IsWindowsVersionOrGreater " +
                    "with version parameters.", e);
            }
        }
        /// <summary>
        /// 获得操作系统名称
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersionName()
        {
            OsEntry curEntry = OsEntry.CurrentOsEntry();
            var kvs = osEntries.Where(x => x.Value.Equals(curEntry));
            if (kvs.Any())
            {
                string osName = null;
                var das = kvs.Select(kv => DescriptionAttribute.GetDescription(kv.Key));
                if (das.Count() == 2)
                {
                    if (IsWindowsServer)
                        osName = das.First(da => da.Tag == 1).Text;
                    else
                        osName = das.First(da => da.Tag == 0).Text;
                }
                else
                {
                    osName = das.First().Text;
                }
                return $"{osName} ({(Environment.Is64BitOperatingSystem ? "64" : "32")}位)";
            }
            else
                return "UnKnow";
        }

        #endregion // Public methods

        #region Public properties

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows XP version.
        /// </summary>
        public static bool IsWindowsXPOrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsXP); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows XP with Service Pack 1 (SP1) version.
        /// </summary>
        public static bool IsWindowsXPSP1OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsXPSP1); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows XP with Service Pack 2 (SP2) version.
        /// </summary>
        public static bool IsWindowsXPSP2OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsXPSP2); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows XP with Service Pack 3 (SP3) version.
        /// </summary>
        public static bool IsWindowsXPSP3OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsXPSP3); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows Vista version.
        /// </summary>
        public static bool IsWindowsVistaOrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsVista); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows Vista with Service Pack 1 (SP1) version.
        /// </summary>
        public static bool IsWindowsVistaSP1OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsVistaSP1); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows Vista with Service Pack 2 (SP2) version.
        /// </summary>
        public static bool IsWindowsVistaSP2OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.WindowsVistaSP2); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows 7 version.
        /// </summary>
        public static bool IsWindows7OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.Windows7); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows 7 with Service Pack 1 (SP1) version.
        /// </summary>
        public static bool IsWindows7SP1OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.Windows7SP1); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows 8 version.
        /// </summary>
        public static bool IsWindows8OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.Windows8); }
        }

        /// <summary>
        /// Indicates if the current OS version matches, or is greater than,
        /// the Windows 8.1 version.
        /// </summary>
        public static bool IsWindows8Point1OrGreater
        {
            get { return IsWindowsVersionOrGreater(KnownOS.Windows8Point1); }
        }

        /// <summary>
        /// Indicates if the current OS is a Windows Server release.
        /// </summary>
        public static bool IsWindowsServer
        {
            get
            {
                if (!isServer.HasValue)
                {
                    // These constants initialized with corresponding
                    // definitions in winnt.h (part of Windows SDK)
                    const byte VER_NT_WORKSTATION = 0x0000001;
                    const uint VER_PRODUCT_TYPE = 0x0000080;
                    const byte VER_EQUAL = 1;

                    OsVersionInfoEx osvi = new OsVersionInfoEx();
                    osvi.OSVersionInfoSize = (uint)Marshal.SizeOf(osvi);
                    osvi.ProductType = VER_NT_WORKSTATION;
                    ulong dwlConditionMask = VerSetConditionMask(
                        0, VER_PRODUCT_TYPE, VER_EQUAL);

                    return !VerifyVersionInfo(
                        ref osvi, VER_PRODUCT_TYPE, dwlConditionMask);
                }

                return isServer.Value;
            }
        }

        #endregion // Public properties
    }
}
