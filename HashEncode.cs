using System;
using System.Security.Cryptography;
using System.Text;

namespace CommonLib
{    /// <summary>
    /// 得到随机安全码（哈希加密）。
    /// </summary>
    public class HashEncode
    {
        public HashEncode()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        /// <summary>
        /// 得到随机哈希加密字符串
        /// </summary>
        /// <returns></returns>
        public static string GetSecurity()
        {
            string security = HashEncoding(GetRandomValue());
            return security;
        }
        /// <summary>
        /// 得到一个随机数值
        /// </summary>
        /// <returns></returns>
        public static string GetRandomValue()
        {
            Random seed = new Random();
            string randomVaule = seed.Next(1, int.MaxValue).ToString();
            return randomVaule;
        }
        /// <summary>
        /// 哈希加密一个字符串
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public static string HashEncoding(string security)
        {
            UnicodeEncoding code = new UnicodeEncoding();
            byte[] message = code.GetBytes(security);
            SHA512Managed arithmetic = new SHA512Managed();
            var value = arithmetic.ComputeHash(message);
            security = "";
            foreach (byte b in value)
            {
                security += (int)b + "O";
            }
            return security;
        }
    }
}

