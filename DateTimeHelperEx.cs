using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib.WinAPI;

namespace CommonLib
{
    public static class DateTimeHelperEx
    {
        /// <summary>
        /// 设置系统时间
        /// </summary>
        /// <param name="dt">要设置的时间</param>
        /// <returns></returns>
        public static bool SetSystemTime(DateTime dt)
        {
            bool flag = false;
            Kernel32.SystemTime sysTime = new Kernel32.SystemTime();
            sysTime.wYear = Convert.ToUInt16(dt.Year);
            sysTime.wMonth = Convert.ToUInt16(dt.Month);
            sysTime.wDay = Convert.ToUInt16(dt.Day);
            sysTime.wHour = Convert.ToUInt16(dt.Hour);
            sysTime.wMinute = Convert.ToUInt16(dt.Minute);
            sysTime.wSecond = Convert.ToUInt16(dt.Second);
            sysTime.wMiliseconds = Convert.ToUInt16(dt.Millisecond);

            try
            {
                flag = Kernel32.SetLocalTime(ref sysTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("SetSystemDateTime函数执行异常" + e.Message);
            }
            return flag;
        }
    }
}
