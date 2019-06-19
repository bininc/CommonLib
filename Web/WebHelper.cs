using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CommonLib.Web
{
    public class WebHelper
    {
        /// <summary>
        /// 获取真实IP
        /// </summary>
        /// <returns></returns>
        public static string GetRequestIP()
        {
            var result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(result))
            {
                return "0.0.0.0";
            }

            if (result == "::1")
                result = "127.0.0.1";

            return result;
        }
    }
}
