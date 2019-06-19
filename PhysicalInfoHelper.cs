using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

namespace CommonLib
{
    public class PhysicalInfoHelper
    {
        //微软官方Win32Class 解释 https://msdn.microsoft.com/en-us/library/aa394084(VS.85).aspx

        /// <summary>
        /// 物理内存
        /// </summary>
        /// <returns></returns>
        public static string GetPhysicalMemory()
        {
            string st = "";
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                ulong membs = Convert.ToUInt64(mo.GetPropertyValue("TotalPhysicalMemory"));
                st = Math.Round(membs / 1024.0 / 1024.0).ToString();
                mo.Dispose();
            }
            moc.Dispose();
            mc.Dispose();
            return st;
        }


        /// <summary>
        /// 获得CPU型号
        /// </summary>
        /// <returns></returns>
        public static string GetCpuName()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher();
            searcher.Query = new SelectQuery("Win32_Processor");
            foreach (var o in searcher.Get())
            {
                var mo = (ManagementObject)o;
                return mo["Name"].ToString();
            }
            searcher.Dispose();
            return null;
        }

        /// <summary>
        /// 获得CPU信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetCpuInfo()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            ManagementClass mc = new ManagementClass("Win32_Processor");
            foreach (var o in mc.GetInstances())
            {
                var mo = (ManagementObject)o;
                PropertyDataCollection pdc = mo.Properties;
                foreach (PropertyData pd in pdc)
                {
                    dic.Add(pd.Name, pd.Value);
                }

                mo.Dispose();
                break;
            }
            mc.Dispose();
            return dic;
        }

        /// <summary>
        /// 获得硬盘信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetDiskInfo()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            ManagementClass mc = new ManagementClass("Win32_DiskDrive");
            foreach (var o in mc.GetInstances())
            {
                var mo = (ManagementObject)o;
                PropertyDataCollection pdc = mo.Properties;
                foreach (PropertyData pd in pdc)
                {
                    dic.Add(pd.Name, pd.Value);
                }

                mo.Dispose();
                break;
            }
            mc.Dispose();
            return dic;
        }

        /// <summary>
        /// 获得网卡信息
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetNetworkAdapterInfo()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            foreach (var o in mc.GetInstances())
            {
                var mo = (ManagementObject)o;
                if ((bool)mo["IPEnabled"] != true) continue;
                PropertyDataCollection pdc = mo.Properties;
                foreach (PropertyData pd in pdc)
                {
                    dic.Add(pd.Name, pd.Value);
                }

                mo.Dispose();
                break;
            }
            mc.Dispose();
            return dic;
        }

        /// <summary>
        /// 主板型号
        /// </summary>
        /// <returns></returns>
        public static string GetBoardType()
        {
            string st = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_BaseBoard");
            foreach (var o in mos.Get())
            {
                var mo = (ManagementObject)o;
                st = mo["Product"].ToString();
                mo.Dispose();
            }
            mos.Dispose();
            return st;
        }
        /// <summary>
        /// 系统型号
        /// </summary>
        /// <returns></returns>
        public static string GetModelName()
        {
            string name = "";
            // create management class object
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            //collection to store all management objects
            ManagementObjectCollection moc = mc.GetInstances();
            if (moc.Count != 0)
            {
                foreach (var o in moc)
                {
                    var mo = (ManagementObject)o;
                    name = mo["Model"].ToString();
                    mo.Dispose();
                }
            }
            moc.Dispose();
            mc.Dispose();
            return name;
        }

        /// <summary>
        /// 获得机器码(CPU序列号 + 硬盘ID + 网卡硬件地址)
        /// </summary>
        /// <returns></returns>
        public static string GetMachineCode()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> dic = GetCpuInfo();
            if (dic.ContainsKey("ProcessorId"))
                sb.Append(dic["ProcessorId"]);
            dic = GetDiskInfo();
            if (dic.ContainsKey("SerialNumber"))
                sb.Append(dic["SerialNumber"]);
            dic = GetNetworkAdapterInfo();
            if (dic.ContainsKey("MACAddress"))
                sb.Append(dic["MACAddress"].ToString().Replace(":", ""));

            return sb.ToString();
        }
    }
}
