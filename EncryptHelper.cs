using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CommonLib
{
    public static class EncryptHelper
    {
        #region RSA加密算法

        /// <summary>
        /// 创建一个公钥
        /// </summary>
        /// <returns></returns>
        public static void RSA_Keys(out string strPublicKey, out string strPrivateKey)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            strPublicKey = Convert.ToBase64String(provider.ExportCspBlob(false));
            strPrivateKey = Convert.ToBase64String(provider.ExportCspBlob(true));
            provider.Dispose();
        }

        /// <summary>
        /// RSA加密（自动生成公钥私钥）
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="strPublicKey">公钥</param>
        /// <param name="strPrivateKey">私钥</param>
        /// <returns></returns>
        public static string RSA_Encrypt(string text, out string strPublicKey, out string strPrivateKey)
        {
            strPrivateKey = strPublicKey = String.Empty;
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(text);
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                strPublicKey = Convert.ToBase64String(provider.ExportCspBlob(false));
                strPrivateKey = Convert.ToBase64String(provider.ExportCspBlob(true));

                //OAEP padding is only available on Microsoft Windows XP or later. 
                byte[] bytesCypherText = provider.Encrypt(dataToEncrypt, false);
                provider.Dispose();
                string strCypherText = Convert.ToBase64String(bytesCypherText);
                return strCypherText;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// RSA加密（公钥加密）
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="strPublicKey">公钥</param>
        /// <returns></returns>
        public static string RSA_Encrypt(string text, string strPublicKey)
        {
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(text);
                byte[] bytesPublicKey = Convert.FromBase64String(strPublicKey);

                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.ImportCspBlob(bytesPublicKey);

                //OAEP padding is only available on Microsoft Windows XP or later. 
                byte[] bytesCypherText = provider.Encrypt(dataToEncrypt, false);
                provider.Dispose();

                string strCypherText = Convert.ToBase64String(bytesCypherText);
                return strCypherText;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="strCypherText">加密密文</param>
        /// <param name="strPrivateKey">私钥</param>
        /// <returns></returns>
        public static string RSA_Decrypt(string strCypherText, string strPrivateKey)
        {
            try
            {
                byte[] dataToDecrypt = Convert.FromBase64String(strCypherText);
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                //RSA.ImportParameters(RSAKeyInfo);
                byte[] bytesPrivateKey = Convert.FromBase64String(strPrivateKey);
                provider.ImportCspBlob(bytesPrivateKey);

                //OAEP padding is only available on Microsoft Windows XP or later. 
                byte[] bytesText = provider.Decrypt(dataToDecrypt, false);
                provider.Dispose();

                string text = Encoding.UTF8.GetString(bytesText);
                return text;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region HMACMD5算法
        //数据签名
        public static byte[] SignData(string key, byte[] data)
        {
            byte[] bytesKey = Encoding.UTF8.GetBytes(key);
            HMAC alg = new HMACMD5();
            //设置密钥
            alg.Key = bytesKey;
            //计算哈希值
            byte[] hash = alg.ComputeHash(data);
            alg.Dispose();

            //返回具有签名的数据（哈希值+数组本身）
            return hash.Concat(data).ToArray();
        }
        //数据认证
        public static bool VerityData(string key, byte[] data)
        {
            byte[] bytesKey = Encoding.UTF8.GetBytes(key);
            HMAC alg = new HMACMD5();
            //提取收到的哈希值
            var receivedHash = data.Take(alg.HashSize >> 3);
            //提取数据本身
            var dataContent = data.Skip(alg.HashSize >> 3).ToArray();
            //设置密钥
            alg.Key = bytesKey;
            //计算数据哈希值和收到的哈希值
            var computedHash = alg.ComputeHash(dataContent);
            alg.Dispose();

            //如果相等则数据正确
            return receivedHash.SequenceEqual(computedHash);
        }

        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="key">加密密钥</param>
        /// <returns></returns>
        public static string MD5_Encrypt(string text, string key)
        {
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(text);
                byte[] bytesKey = Encoding.UTF8.GetBytes(key);
                HMAC hmac = new HMACMD5(bytesKey);
                byte[] hash = hmac.ComputeHash(dataToEncrypt);
                hmac.Dispose();
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2")); // hex format
                }
                return result.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// 用MD5加密字符串，可选择生成16位或者32位的加密字符串
        /// </summary>
        /// <param name="password">待加密的字符串</param>
        /// <param name="bit">位数，一般取值16 或 32</param>
        /// <returns>返回的加密后的字符串</returns>
        public static string MD5Encrypt(string password, int bit = 32)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            hashedDataBytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder tmp = new StringBuilder();
            foreach (byte i in hashedDataBytes)
            {
                tmp.Append(i.ToString("X2"));
            }
            if (bit == 16)
                return tmp.ToString().Substring(8, 16);
            else if (bit == 32)
                return tmp.ToString();//默认情况
            else
                return string.Empty;
        }

        #endregion

        #region 文件MD5计算
        /// <summary>  
        /// 通过MD5CryptoServiceProvider类中的ComputeHash方法直接传入一个FileStream类实现计算MD5  
        /// 操作简单，代码少，调用即可  
        /// </summary>  
        /// <param name="path">文件地址</param>  
        /// <returns>MD5Hash</returns>  
        public static string GetMd5ByFilePath(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"{nameof(path)}<{path}>, 不存在");
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            string result = GetMd5ByStream(fs);
            fs.Close();
            return result;
        }
        /// <summary>  
        /// 通过MD5CryptoServiceProvider类中的ComputeHash方法直接传入一个FileStream类实现计算MD5  
        /// 操作简单，代码少，调用即可  
        /// </summary>  
        /// <param name="stream">字节流</param>  
        /// <returns>MD5Hash</returns>  
        public static string GetMd5ByStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] buffer = md5Provider.ComputeHash(stream);
            string result = BitConverter.ToString(buffer);
            result = result.Replace("-", "");
            md5Provider.Clear();
            return result;
        }

        /// <summary>  
        /// 通过HashAlgorithm的TransformBlock方法对流进行叠加运算获得MD5  
        /// 实现稍微复杂，但可使用与传输文件或接收文件时同步计算MD5值  
        /// 可自定义缓冲区大小，计算速度较快  
        /// </summary>  
        /// <param name="path">文件地址</param>  
        /// <returns>MD5Hash</returns>  
        public static string GetMd5ByFilePath2(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"{nameof(path)}<{path}>, 不存在");

            FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            string md5 = GetMd5ByStream2(fileStream);
            fileStream.Close();
            return md5;
        }

        /// <summary>  
        /// 通过HashAlgorithm的TransformBlock方法对流进行叠加运算获得MD5  
        /// 实现稍微复杂，但可使用与传输文件或接收文件时同步计算MD5值  
        /// 可自定义缓冲区大小，计算速度较快  
        /// </summary>  
        /// <param name="stream">字节流</param>  
        /// <returns>MD5Hash</returns>  
        public static string GetMd5ByStream2(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            int bufferSize = 1024 * 16;//自定义缓冲区大小16K  
            byte[] buffer = new byte[bufferSize];
            HashAlgorithm hashAlgorithm = new MD5CryptoServiceProvider();
            int readLength = 0;//每次读取长度 
            stream.Position = 0;    //从头开始读 
            var output = new byte[bufferSize];
            while ((readLength = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                //计算MD5  
                hashAlgorithm.TransformBlock(buffer, 0, readLength, output, 0);
            }
            //完成最后计算，必须调用(由于上一部循环已经完成所有运算，所以调用此方法时后面的两个参数都为0)  
            hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
            string md5 = BitConverter.ToString(hashAlgorithm.Hash);
            hashAlgorithm.Clear();
            md5 = md5.Replace("-", "");
            return md5;
        }
        #endregion
    }
}
