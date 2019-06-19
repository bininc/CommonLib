using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CommonLib.WinAPI;
using System.Runtime.InteropServices;

namespace CommonLib
{
    public class OTAUpdateHelper
    {
        public const string UpTool_AppID = "OTAUpdate";
        public const string UpTool_File = "OTAUpdate.exe";
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="appId">客户端标识</param>
        /// <returns></returns>
        public static string CheckUpdate(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId)) return null;

            try
            {
                var pss = Process.GetProcessesByName(UpTool_AppID);
                Process p = pss.FirstOrDefault();
                if (p != null)
                {
                    p.Kill();
                    p.Close();
                }

                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = UpTool_File;
                p.StartInfo.CreateNoWindow = true;
                //p.EnableRaisingEvents = true;

                string upres = null;
                p.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null && e.Data.StartsWith("end_"))
                        {
                            upres = e.Data.Substring(4);
                        }
                    };
                p.Start();
                p.BeginOutputReadLine();

                int i = 0;
                while (i < 10)
                {
                    bool createNew;
                    Mutex mutex = new Mutex(true, UpTool_AppID, out createNew);
                    mutex.Close();
                    if (!createNew)
                    {
                        ShareMemoryManager manager = new ShareMemoryManager(UpTool_AppID);
                        bool suc = manager.WriteString("checkupdate_" + appId);
                        manager.Dispose();
                        if (suc) break;
                    }

                    i++;
                    Thread.Sleep(1000);
                }

                p.WaitForExit();
                p.Close();

                return upres;
            }
            catch (Exception e)
            {
                LogHelper.WriteError(e, "CheckUpdate");
                return null;
            }
        }

        /// <summary>
        /// 启动更新程序
        /// </summary>
        /// <param name="args">参数</param>
        public static void RunOTAUpdate(string args)
        {
            if(string.IsNullOrWhiteSpace(args)) return;

            try
            {
                var pss = Process.GetProcessesByName(UpTool_AppID);
                Process p = pss.FirstOrDefault();
                if (p != null)
                {
                    p.Kill();
                    p.Close();
                }

                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = UpTool_File;
                p.StartInfo.CreateNoWindow = false;
                //p.EnableRaisingEvents = true;
                p.Start();

                int i = 0;
                while (i < 10)
                {
                    bool createNew;
                    Mutex mutex = new Mutex(true, UpTool_AppID, out createNew);
                    mutex.Close();
                    if (!createNew)
                    {
                        ShareMemoryManager manager = new ShareMemoryManager(UpTool_AppID);
                        bool suc= manager.WriteString(args);
                        manager.Dispose();
                        if(suc) break;
                    }

                    i++;
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e + " " + args);
                LogHelper.WriteError(e, args);
            }
        }
    }
}
