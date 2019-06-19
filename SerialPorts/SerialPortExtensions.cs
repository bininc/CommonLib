using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace CommonLib.SerialPorts
{
    internal static class DCB
    {
        internal const int FBINARY = 0; //指定是否允许二进制模式
        internal const int FPARITY = 1; //指定奇偶校验是否允许，在为true时具体采用何种校验看Parity 设置
        internal const int FOUTXCTSFLOW = 2; //是否监控CTS(clear-to-send)信号来做输出流控
        internal const int FOUTXDSRFLOW = 3; //是否监控DSR (data-set-ready) 信号来做输出流控
        internal const int FDTRCONTROL = 4; //DTR(data-terminal-ready)流控，可取值如下：DTR_CONTROL_DISABLE
        internal const int FDSRSENSITIVITY = 6;
        internal const int FTXCONTINUEONXOFF = 7;
        internal const int FOUTX = 8;
        internal const int FINX = 9;
        internal const int FERRORCHAR = 10;
        internal const int FNULL = 11;
        internal const int FRTSCONTROL = 12;
        internal const int FABORTONOERROR = 14;
        internal const int FDUMMY2 = 15;
    }

    internal enum FlowControl
    {
        No,
        CtsRts,
        CtsDtr,
        DsrRts,
        DsrDtr,
        XonXoff,
    }

    internal static class SerialPortExtensions
    {
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void SetField(this SerialPort port, string field, object value)
        {
            if (port == null)
                throw new NullReferenceException();
            if (port.BaseStream == null)
                throw new InvalidOperationException("Cannot change fields until after the port has been opened.");
            try
            {
                object baseStream = port.BaseStream;
                Type baseStreamType = baseStream.GetType();
                FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
                object dcbValue = dcbFieldInfo.GetValue(baseStream);
                Type dcbType = dcbValue.GetType();
                dcbType.GetField(field).SetValue(dcbValue, value);
                dcbFieldInfo.SetValue(baseStream, dcbValue);
            }
            catch (SecurityException) { throw; }
            catch (OutOfMemoryException) { throw; }
            catch (Win32Exception) { throw; }
            catch (Exception)
            {
                throw;
            }
        }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void SetFlag(this SerialPort port, int flag, int value)
        {
            object BaseStream = port.BaseStream;
            Type SerialStream = BaseStream.GetType();
            SerialStream.GetMethod("SetDcbFlag", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(BaseStream, new object[] { flag, value });
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void SetFlowControl(this SerialPort port, FlowControl fc)
        {
            switch (fc)
            {
                case FlowControl.No:
                    port.SetFlag(DCB.FOUTXCTSFLOW, 0);
                    port.SetFlag(DCB.FOUTXDSRFLOW, 0);
                    port.SetFlag(DCB.FOUTX, 0);
                    port.SetFlag(DCB.FINX, 0);
                    break;
                case FlowControl.CtsRts:
                    port.SetFlag(DCB.FOUTXCTSFLOW, 1);
                    port.SetFlag(DCB.FOUTXDSRFLOW, 0);
                    port.SetFlag(DCB.FRTSCONTROL, 0x02);
                    port.SetFlag(DCB.FOUTX, 0);
                    port.SetFlag(DCB.FINX, 0);
                    break;
                case FlowControl.CtsDtr:
                    port.SetFlag(DCB.FOUTXCTSFLOW, 1);
                    port.SetFlag(DCB.FOUTXDSRFLOW, 0);
                    port.SetFlag(DCB.FDTRCONTROL, 0x02);
                    port.SetFlag(DCB.FOUTX, 0);
                    port.SetFlag(DCB.FINX, 0);
                    break;
                case FlowControl.DsrRts:
                    port.SetFlag(DCB.FOUTXCTSFLOW, 0);
                    port.SetFlag(DCB.FOUTXDSRFLOW, 1);
                    port.SetFlag(DCB.FRTSCONTROL, 0x02);
                    port.SetFlag(DCB.FOUTX, 0);
                    port.SetFlag(DCB.FINX, 0);
                    break;
                case FlowControl.DsrDtr:
                    port.SetFlag(DCB.FOUTXCTSFLOW, 0);
                    port.SetFlag(DCB.FOUTXDSRFLOW, 1);
                    port.SetFlag(DCB.FDTRCONTROL, 0x02);
                    port.SetFlag(DCB.FOUTX, 0);
                    port.SetFlag(DCB.FINX, 0);
                    break;
                case FlowControl.XonXoff:
                    port.SetFlag(DCB.FOUTXCTSFLOW, 0);
                    port.SetFlag(DCB.FOUTXDSRFLOW, 0);
                    port.SetFlag(DCB.FOUTX, 1);
                    port.SetFlag(DCB.FINX, 1);
                    port.SetField("XonChar", (byte)0x11);
                    port.SetField("XoffChar", (byte)0x13);
                    port.SetField("XonLim", (ushort)100);
                    port.SetField("XoffLim", (ushort)100);
                    break;
            }

            port.UpdateComm();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void UpdateComm(this SerialPort port)
        {
            object baseStream = port.BaseStream;
            Type baseStreamType = baseStream.GetType();
            FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
            object dcbValue = dcbFieldInfo.GetValue(baseStream);
            SafeFileHandle portFileHandle = (SafeFileHandle)baseStreamType.GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseStream);
            IntPtr hGlobal = Marshal.AllocHGlobal(Marshal.SizeOf(dcbValue));
            try
            {
                Marshal.StructureToPtr(dcbValue, hGlobal, false);
                if (!SetCommState(portFileHandle, hGlobal))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                if (hGlobal != IntPtr.Zero)
                    Marshal.FreeHGlobal(hGlobal);
            }
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle hFile, IntPtr lpDCB);
    }
}
