using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace CommonLib
{
    public static class StringHelper
    {
        #region 字符串与Base64之间的转换
        /// <summary>
        /// 将字符串转成Base64字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="strEncoding">字符串编码</param>
        /// <returns></returns>
        public static string ConvertToBase64String(string str, Encoding strEncoding = null)
        {
            if (str == null) return null;
            if (strEncoding == null)
                strEncoding = Encoding.UTF8;
            byte[] strBytes = strEncoding.GetBytes(str);
            return Convert.ToBase64String(strBytes);
        }

        /// <summary>
        /// 将字符串转成Base64字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="strEncoding">字符串编码</param>
        /// <returns></returns>
        public static string ToBase64String(this string str, Encoding strEncoding = null)
        {
            return ConvertToBase64String(str, strEncoding);
        }

        /// <summary>
        /// 将Base64字符串还原成原字符串
        /// </summary>
        /// <param name="base64Str">Base64字符串</param>
        /// <param name="strEncoding">字符串编码</param>
        /// <returns></returns>
        public static string ConvertFromBase64String(string base64Str, Encoding strEncoding = null)
        {
            if (base64Str == null) return null;
            if (strEncoding == null)
                strEncoding = Encoding.UTF8;
            byte[] strBytes = Convert.FromBase64String(base64Str);
            return strEncoding.GetString(strBytes, 0, strBytes.Length);
        }

        /// <summary>
        /// 将Base64字符串还原成原字符串
        /// </summary>
        /// <param name="base64Str">Base64字符串</param>
        /// <param name="strEncoding">字符串编码</param>
        /// <returns></returns>
        public static string FromBase64String(this string base64Str, Encoding strEncoding = null)
        {
            return ConvertFromBase64String(base64Str, strEncoding);
        } 
        #endregion

        #region 数组相关操作
        ///<summary>
        /// GZip压缩数组
        ///</summary>
        ///<param name="data">需要压缩的数组</param>
        ///<returns>压缩后的数组</returns>
        public static byte[] GZipCompressBytes(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress);
            gZipStream.Write(data, 0, data.Length);
            gZipStream.Close();
            byte[] bytes = stream.ToArray();
            stream.Close();
            return bytes;
        }

        /// <summary>
        /// GZip解压数组
        /// </summary>
        /// <param name="data">压缩的数组</param>
        /// <returns>解压后的数组</returns>
        public static byte[] GZipDecompressBytes(byte[] data)
        {
            MemoryStream sourceStream = new MemoryStream(data);
            MemoryStream stream = new MemoryStream();
            GZipStream gZipStream = new GZipStream(sourceStream, CompressionMode.Decompress);

            byte[] bytes = new byte[64];
            int n;
            while ((n = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
            {
                stream.Write(bytes, 0, n);
            }
            gZipStream.Close();
            sourceStream.Close();
            byte[] resBytes = stream.ToArray();
            stream.Close();
            return resBytes;
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStrToBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToHexStr(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }
        #endregion

        #region 字符串压缩
        /// <summary>  
        /// 对字符串进行压缩  
        /// </summary>  
        /// <param name="str">待压缩的字符串</param>  
        /// <returns>压缩后的字符串</returns>  
        public static string CompressString(string str)
        {
            string compressString = "";
            byte[] compressBeforeByte = Encoding.UTF8.GetBytes(str);
            byte[] compressAfterByte = GZipCompressBytes(compressBeforeByte);
            compressString = Convert.ToBase64String(compressAfterByte);
            return compressString;
        }
        /// <summary>  
        /// 对字符串进行解压缩  
        /// </summary>  
        /// <param name="str">待解压缩的字符串</param>  
        /// <returns>解压缩后的字符串</returns>  
        public static string DecompressString(string str)
        {
            string compressString = "";
            byte[] compressBeforeByte = Convert.FromBase64String(str);
            byte[] compressAfterByte = GZipDecompressBytes(compressBeforeByte);
            compressString = Encoding.UTF8.GetString(compressAfterByte);
            return compressString;
        }
        #endregion
    }
}
