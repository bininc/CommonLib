using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLib.WinAPI
{
    public class RegisterHelper
    {
        /// <summary>
        /// 检查是否有指定的开机启动项
        /// </summary>
        /// <returns></returns>
        public static bool CheckAutoStart(string name, string runPath, bool isLocalMachine = false)
        {
            RegDomain domain = RegDomain.CurrentUser;
            if (isLocalMachine)
                domain = RegDomain.LocalMachine;
            Register run = new Register(@"Software\Microsoft\Windows\CurrentVersion\Run", domain);
            object value = run.ReadRegeditKey(name);
            if (value == null)
                return false;
            if (value.ToString() != runPath)
                return false;
            return true;
        }

        /// <summary>
        /// 添加开机启动项
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="runPath">启动路径</param>
        /// <param name="isLocalMachine">注册表位置</param>
        /// <returns></returns>
        public static bool AddAutoStart(string name, string runPath, bool isLocalMachine = false)
        {
            RegDomain domain = RegDomain.CurrentUser;
            if (isLocalMachine)
                domain = RegDomain.LocalMachine;
            Register run = new Register(@"Software\Microsoft\Windows\CurrentVersion\Run", domain);
            return run.WriteRegeditKey(name, runPath);
        }

        /// <summary>
        /// 移除开机启动项
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="isLocalMachine">注册表位置</param>
        /// <returns></returns>
        public static bool RemoveAutoStart(string name, bool isLocalMachine = false)
        {
            RegDomain domain = RegDomain.CurrentUser;
            if (isLocalMachine)
                domain = RegDomain.LocalMachine;
            Register run = new Register(@"Software\Microsoft\Windows\CurrentVersion\Run", domain);
            return run.DeleteRegeditKey(name);
        }
    }
}
