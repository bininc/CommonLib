using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace CommonLib.SerialPorts
{
    public class Jsbyjgs2GSerialPortManager : SerialPortManager
    {
        protected override bool VerifySerialPort(SerialPort port)
        {
            Thread.Sleep(1000);
            while (port.BytesToRead > 0)
            {
                string str = port.ReadLine();
                if (str.StartsWith("$"))
                {
                    if (str.Substring(1, 4) == "GPRS")
                    {
                        port.SetFlowControl(FlowControl.No);
                        port.Encoding= Encoding.GetEncoding("GB2312");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
