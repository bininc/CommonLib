using CommonLib.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommLiby;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Web;
using CommLiby.CheckSum;

namespace CommonLib
{
    public static class Common
    {
        /// <summary>
        /// 数据集是否为空
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static bool DataSetIsEmpty(DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 数据集是否为空
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static bool DataSetIsNotEmpty(DataSet dataSet)
        {
            return !DataSetIsEmpty(dataSet);
        }

        /// <summary>
        /// 验证数据集是否为空
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static DataSet VerifyDataSetEmpty(DataSet dataSet)
        {
            if (DataSetIsEmpty(dataSet))
                return null;
            else
                return dataSet;
        }

        /// <summary>
        /// 验证DataTable是否有数据 否则传入datatable赋为null
        /// </summary>
        /// <param name="dt">The dt.</param>
        public static DataTable DataTableVerify(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                dt = null;
            }
            return dt;
        }

        /// <summary>
        /// 验证DataTable是否有数据 否则传入datatable赋为null
        /// </summary>
        /// <param name="dt">The dt.</param>
        public static bool DataTableIsEmpty(DataTable dt)
        {
            if (DataTableVerify(dt) == null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 验证DataTable是否有数据 否则传入datatable赋为null
        /// </summary>
        /// <param name="dt">The dt.</param>
        public static bool DataTableIsNotEmpty(DataTable dt)
        {
            return !DataTableIsEmpty(dt);
        }

        /// <summary>
        /// 计算分页数据
        /// </summary>
        public static void CalPageData(int rowCount, int pageSize, ref int pageIndex, ref int pageCount)
        {
            if (pageIndex < 1) pageIndex = 1;   //非正常值控制
            if (pageSize < 1) pageSize = 1;     //非正常值控制
            pageCount = (int)Math.Ceiling(rowCount / (double)pageSize);
            pageIndex = (pageCount > 0 && pageIndex > pageCount) ? pageCount : pageIndex;   //过滤无效索引
        }

        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="val">数据值</param>
        /// <returns></returns>
        public static object Convert2Type(Type type, object val)
        {
            if (string.IsNullOrWhiteSpace(val?.ToString()) && type.IsValueType)
                return DBNull.Value;
            //val = val.ToString();   //先转换成String类型再进行转换
            if (val is DBNull)
                return null;
            else if (type == typeof(BoundaryPoint))
            {
                return null;
            }
            else
            {
                return Common_liby.Convert2Type(type, val);
            }
        }

        /// <summary>
        /// 数据类型转换
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="val">数据值</param>
        /// <returns></returns>
        public static T Convert2Type<T>(object val)
        {
            return Common_liby.Convert2Type<T>(val);
        }

        /// <summary>
        /// 获取一个GUID作为数据库表或者表单的主键
        /// </summary>
        /// <returns></returns>
        public static string GetGuidString()
        {
            return CommLiby.Common_liby.GetGuidString();
        }


        /// <summary>
        /// 将数组数据导入到当前行
        /// </summary>
        /// <returns></returns>
        public static void ImportDataFromArray(this DataRow dr, object[] array)
        {
            if (dr == null || array == null) return;
            if (dr.Table.Columns.Count != array.Length) return;
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                DataColumn column = dr.Table.Columns[i];
                Type dataType = column.DataType;
                dr[column] = Convert2Type(dataType, array[i]);
            }
        }


        /// <summary>
        /// 数据行值是否为空字符串
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool IsEmpty(this DataRow dr, string columnName)
        {
            if (dr == null) return true;
            if (dr.IsNull(columnName)) return false;
            return dr[columnName].ToString() == string.Empty;
        }

        /// <summary>
        /// 数据行值是否为空字符串
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static bool IsEmpty(this DataRow dr, int columnIndex)
        {
            if (dr == null) return true;
            if (dr.IsNull(columnIndex)) return false;
            return dr[columnIndex].ToString() == string.Empty;
        }

        /// <summary>
        /// 判断数据行是Null或者空字符串
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this DataRow dr, string columnName)
        {
            if (dr == null) return true;
            return dr.IsNull(columnName) || dr.IsEmpty(columnName);
        }

        /// <summary>
        /// 判断数据行是Null或者空字符串
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this DataRow dr, int columnIndex)
        {
            if (dr == null) return true;
            return dr.IsNull(columnIndex) || dr.IsEmpty(columnIndex);
        }

        /// <summary>
        /// 获取数据行String类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetDataRowStringValue(this DataRow dr, string columnName)
        {
            try
            {
                if (dr == null || dr.IsNull(columnName))
                    return null;
                else
                    return dr[columnName].ToString();
            }
            catch
            {
                return null;
            }
        }
        public static string GetDataRowStringValue(this DataRow dr, int columnIndex)
        {
            try
            {
                if (dr == null || dr[columnIndex] == null)
                    return null;
                else
                    return dr[columnIndex].ToString();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取数据行指定字段int类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static int GetDataRowIntValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return -1;
            else
            {
                int val = -1;
                bool suc = int.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非int类型
            }
        }
        public static int GetDataRowIntValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return -1;
            else
            {
                int val = -1;
                bool suc = int.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非int类型
            }
        }

        /// <summary>
        /// 获取数据行指定字段int类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static uint GetDataRowUintValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return uint.MaxValue;
            else
            {
                uint val;
                bool suc = uint.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return uint.MaxValue;  //非uint类型
            }
        }
        public static uint GetDataRowUintValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return uint.MaxValue;
            else
            {
                uint val;
                bool suc = uint.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return uint.MaxValue;  //非uint类型
            }
        }
        /// <summary>
        /// 获取数据行指定字段long类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static long GetDataRowLongValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return long.MaxValue;
            else
            {
                long val;
                bool suc = long.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return long.MaxValue;  //非uint类型
            }
        }
        public static long GetDataRowLongValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return long.MaxValue;
            else
            {
                long val;
                bool suc = long.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return long.MaxValue;  //非uint类型
            }
        }

        /// <summary>
        /// 获取数据行指定字段double类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static double GetDataRowDoubleValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return -1;
            else
            {
                double val = -1;
                bool suc = double.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非double类型
            }
        }
        public static double GetDataRowDoubleValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return -1;
            else
            {
                double val = -1;
                bool suc = double.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非double类型
            }
        }

        /// <summary>
        /// 获取数据行指定字段float类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static float GetDataRowFloatValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return -1;
            else
            {
                float val = -1;
                bool suc = float.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非double类型
            }
        }
        public static float GetDataRowFloatValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return -1;
            else
            {
                float val = -1;
                bool suc = float.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非double类型
            }
        }
        /// <summary>
        /// 获取数据行指定字段decimal类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static decimal GetDataRowDecimalValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return -1;
            else
            {
                decimal val = -1;
                bool suc = decimal.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非double类型
            }
        }
        public static decimal GetDataRowDecimalValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return -1;
            else
            {
                decimal val = -1;
                bool suc = decimal.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return -2;  //非double类型
            }
        }
        /// <summary>
        /// 获取数据行指定字段DateTime类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static DateTime GetDataRowDateTimeValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return DateTime.MinValue;
            else
            {
                DateTime val = DateTime.MinValue;
                bool suc = DateTime.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return DateTime.MaxValue; //非DateTime类型
            }
        }
        public static DateTime GetDataRowDateTimeValue(this DataRow dr, int columnIndex)
        {
            if (dr.IsNullOrEmpty(columnIndex))
                return DateTime.MinValue;
            else
            {
                DateTime val = DateTime.MinValue;
                bool suc = DateTime.TryParse(dr.GetDataRowStringValue(columnIndex).Trim(), out val);
                if (suc)
                    return val;
                else
                    return DateTime.MaxValue; //非DateTime类型
            }
        }
        /// <summary>
        /// 获取数据行指定字段byte[]类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static byte[] GetDataRowBytesValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return new byte[0];
            else
            {
                byte[] bytes = dr[columnName] as byte[];
                if (bytes != null)
                    return bytes;
                else
                    return new byte[0];
            }
        }

        /// <summary>
        /// 获取数据航制定字段byte类型值
        /// </summary>
        /// <returns></returns>
        public static byte GetDataRowByteValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return byte.MinValue;
            else
            {
                byte val = byte.MinValue;
                bool suc = byte.TryParse(dr[columnName].ToString().Trim(), out val);
                if (suc)
                    return val;
                else
                    return byte.MaxValue; //非byte类型
            }
        }

        /// <summary>
        /// 获取指定字段的Bool类型值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool? GetDataRowBoolValue(this DataRow dr, string columnName)
        {
            if (dr.IsNullOrEmpty(columnName))
                return null;
            else
            {
                bool val = false;
                bool suc = bool.TryParse(dr.GetDataRowStringValue(columnName).Trim(), out val);
                if (suc)
                    return val;
                else
                    return null; //非byte类型
            }
        }

        /// <summary>
        /// 判断Table是否为空
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsEmpty(this DataTable table)
        {
            return DataTableIsEmpty(table);
        }
        /// <summary>
        /// 判断Table是否不为空
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(this DataTable table)
        {
            return DataTableIsNotEmpty(table);
        }

        /// <summary>
        /// 判断DataSet是否为空
        /// </summary>
        /// <returns></returns>
        public static bool IsEmpty(this DataSet ds)
        {
            return DataSetIsEmpty(ds);
        }
        /// <summary>
        /// 判断DataSet是否不为空
        /// </summary>
        /// <returns></returns>
        public static bool IsNotEmpty(this DataSet ds)
        {
            return DataSetIsNotEmpty(ds);
        }

        /// <summary>
        /// 判断其实路径是否为WebServer类型路径
        /// </summary>
        /// <param name="StartPath"></param>
        /// <returns></returns>
        public static bool IsWebSite()
        {
            return HttpContext.Current != null;
            //string StartPath = Environment.CurrentDirectory.ToLower();
            //if (StartPath == @"c:\windows\system32\inetsrv"
            //               || StartPath == @"c:\windows\syswow64\inetsrv"
            //               || StartPath == @"c:\windows\microsoft.net\framework\v2.0.50727"
            //               || StartPath == @"c:\windows\microsoft.net\framework64\v2.0.50727"
            //               || StartPath == @"c:\windows\microsoft.net\framework\v4.0.30319"
            //               || StartPath == @"c:\windows\microsoft.net\framework64\v4.0.30319"
            //               || StartPath == @"c:\program files\iis express"
            //               || StartPath == @"c:\program files (x86)\iis express"
            //    )
            //{
            //    //webRoot
            //    return true;
            //}
            //else
            //{
            //    //WinformRoot
            //    return false;
            //}
        }

        /// <summary>
        /// 获取根目录文件路径
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static string GetRootPath(string fileName = null)
        {
            string path = "";
            if (IsWebSite())
            {
                path = HttpContext.Current.Server.MapPath("~/");
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }
            if (fileName != null)
                return Path.Combine(path, fileName);
            else
                return path;
        }

        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件名（路径）</param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取文件CRC32值
        /// </summary>
        /// <param name="fileName">文件名（路径）</param>
        /// <returns></returns>
        public static string GetCRC32FromFile(string fileName)
        {
            try
            {
                byte[] filedata = File.ReadAllBytes(fileName);
                long crc32 = Crc32.Get(filedata);
                filedata = null;
                return crc32.ToString("X");
            }
            catch (Exception ex)
            {
                throw new Exception("GetCRC32FromFile() fail,error:" + ex.Message);
            }
        }

        /// <summary>
        /// 由结构体转换为byte数组
        /// </summary>
        public static byte[] StructureToByte<T>(T structure)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }

        /// <summary>
        /// 由byte数组转换为结构体
        /// </summary>
        public static T ByteToStructure<T>(byte[] dataBuffer)
        {
            object structure = null;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(dataBuffer, 0, allocIntPtr, size);
                structure = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)structure;
        }

        public static string GetAppVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (assembly == null) return null;

            return assembly.GetName().Version.ToString();
        }

        public static string GetBinFileVersion(string path)
        {
            try
            {
                FileVersionInfo fvInfo = FileVersionInfo.GetVersionInfo(path);
                return fvInfo.FileVersion;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //Change the RegistryKey value to enable hardware accelration
        public static void EnableHWAcceleration()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Avalon.Graphics\", true);
            if (registryKey != null && registryKey.GetValue("DisableHWAcceleration")?.ToString() == "1")
            {
                registryKey.SetValue("DisableHWAcceleration", 0);
            }
        }
    }
}