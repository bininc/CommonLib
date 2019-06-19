using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonLib
{
    public class LocalNetInfo
    {
        /// <summary>
        ///  判断是否为正确的IP地址
        /// </summary>
        /// <param name="strIPadd">需要判断的字符串</param>
        /// <returns>true = 是 false = 否</returns>
        public static bool IsRightIP(string strIPadd)
        {
            //利用正则表达式判断字符串是否符合IPv4格式
            if (Regex.IsMatch(strIPadd, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
            {
                //根据小数点分拆字符串
                var ips = strIPadd.Split('.');
                if (ips.Length == 4 || ips.Length == 6)
                    if (int.Parse(ips[0]) < 256 && (int.Parse(ips[1]) < 256) & (int.Parse(ips[2]) < 256) &
                        (int.Parse(ips[3]) < 256))
                        //正确
                        return true;
                    //如果不符合
                    else
                        //错误
                        return false;
                return false;
            }

            return false;
        }

        /// <summary>
        ///     得到本机IP
        /// </summary>
        public static string GetLocalIP()
        {
            //本机IP地址
            var strLocalIP = "";

            //得到计算机名
            var strPcName = Dns.GetHostName();

            //得到本机IP地址数组
            var ipEntry = Dns.GetHostEntry(strPcName);

            //遍历数组
            foreach (var IPadd in ipEntry.AddressList)

                //判断当前字符串是否为正确IP地址
                if (IsRightIP(IPadd.ToString()))
                {
                    //得到本地IP地址
                    strLocalIP = IPadd.ToString();

                    //结束循环
                    break;
                }


            //返回本地IP地址
            return strLocalIP;
        }


        //得到网关地址

        public static string GetGateway()
        {
            //网关地址
            var strGateway = "";

            //获取所有网卡
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            //遍历数组
            foreach (var netWork in nics)
            {
                //单个网卡的IP对象
                var ip = netWork.GetIPProperties();

                //获取该IP对象的网关
                var gateways = ip.GatewayAddresses;
                foreach (var gateWay in gateways)
                {
                    //如果能够Ping通网关
                    if (PingIpOrDomainName(gateWay.Address.ToString()))
                    {
                        //得到网关地址
                        strGateway = gateWay.Address.ToString();
                        //跳出循环
                        break;
                    }
                }

                //如果已经得到网关地址
                if (strGateway.Length > 0)
                    break;
            }


            //返回网关地址
            return strGateway;
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

        /// <summary>
        /// 用于检查网络是否可以连接互联网,true表示连接成功,false表示连接失败
        /// </summary>
        /// <returns></returns>
        public static bool IsConnectInternet()
        {
            int Description = 0;
            bool suc = InternetGetConnectedState(out Description, 0);
            return suc;
        }

        /// <summary>
        ///     用于检查IP地址或域名是否可以使用TCP/IP协议访问(使用Ping命令),true表示Ping成功,false表示Ping失败
        /// </summary>
        /// <param name="strIpOrDName">输入参数,表示IP地址或域名</param>
        /// <returns></returns>
        public static bool PingIpOrDomainName(string strIpOrDName)
        {
            try
            {
                var objPingSender = new Ping();
                var objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                var data = "";
                var buffer = Encoding.UTF8.GetBytes(data);
                var intTimeout = 120;
                var objPinReply = objPingSender.Send(strIpOrDName, intTimeout, buffer, objPinOptions);
                var strInfo = objPinReply.Status.ToString();
                if (strInfo == "Success")
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static PingReply PingIp(string strIp)
        {
            try
            {
                var objPingSender = new Ping();
                var objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                var data = "";
                var buffer = Encoding.UTF8.GetBytes(data);
                var intTimeout = 120;
                var objPinReply = objPingSender.Send(strIp, intTimeout, buffer, objPinOptions);
                return objPinReply;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}