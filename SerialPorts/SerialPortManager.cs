using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Text;
using CommLiby;

namespace CommonLib.SerialPorts
{
    public class SerialPortManager
    {
        private static List<SerialPortManager> managers = new List<SerialPortManager>();
        private static List<string> usedPorts = new List<string>();
        private static Thread scanPortTh = null;
        private static bool stopScan = false;
        private static Task checkTask = null;
        private static readonly object lockobj = new object();

        /// <summary>
        /// 串口状态发生改变事件
        /// </summary>
        public event StateChangedHandler StateChanged;
        /// <summary>
        /// 接收到的Bytes数组
        /// </summary>
        public event BytesReceivedHandler BytesReceived;
        /// <summary>
        /// 接收到的一行数据信息
        /// </summary>
        public event LineReceivedHandler LineReceived;

        public SerialPort currentPort;
        private int _baudRate = 115200;
        private int _portNum = -1;
        private Parity _parity = Parity.None;
        private int _dataBits = 8;
        private StopBits _stopBits = StopBits.One;
        private bool userCustomName = false;
        private string _portName;
        private DateTime lastRecTime = DateTime.MaxValue;
        private TQueue<byte> recQueue = new TQueue<byte>();
        private bool Released = false;
        private Thread dealRecDataTh;

        public string PortName
        {
            get { return _portName; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _portName = value;
                    int suc = GetPortNum();
                    if (suc != -1)
                    {
                        _portNum = suc;
                        userCustomName = true;
                    }
                }
            }
        }

        public int PortNum
        {
            get { return _portNum; }
            private set
            {
                if (value != -1)
                {
                    _portNum = value;
                    _portName = "COM" + _portNum;
                    userCustomName = true;
                }
            }
        }

        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        public Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        public int DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        public StopBits StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        public bool Connected { get; private set; }
        public Encoding Encoding { get { return currentPort?.Encoding; } }

        protected SerialPortManager()
        {
            lock (managers)
            {
                managers.Add(this);
            }
        }

        public SerialPortManager(int baudRate = 115200, string portName = null) : this()
        {
            PortName = portName;
            _baudRate = baudRate;
        }

        public SerialPortManager(int baudRate = 115200, int portNum = -1) : this()
        {
            PortNum = portNum;
            _baudRate = baudRate;
        }

        public SerialPortManager(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : this()
        {
            PortName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
        }

        public int GetPortNum()
        {
            string num = _portName.TrimStart('C', 'O', 'M');
            int n = -1;
            bool suc = int.TryParse(num, out n);
            return n;
        }

        protected virtual bool OnStateChanged(int no, string msg)
        {
            return true;
        }

        protected void InvokeStateChanged(int no, string msg)
        {
            if (OnStateChanged(no, msg))
                StateChanged?.BeginInvoke(no, msg, null, null);
        }

        public void Scan()
        {
            InvokeStateChanged(1, "配置参数...");

            lock (lockobj)
            {
                if (scanPortTh == null)
                {
                    scanPortTh = new Thread(() =>
                    {
                        while (!stopScan)
                        {
                            lock (managers)
                            {
                                foreach (SerialPortManager m in managers)
                                {
                                    if (m.Connected) continue;

                                    m.InvokeStateChanged(2, "扫描设备...");

                                    string[] portNames = SerialPort.GetPortNames();
                                    foreach (string portName in portNames) //遍历已经扫描到的串口
                                    {
                                        //跳过已经使用的串口
                                        if (string.IsNullOrEmpty(portName) || usedPorts.Contains(portName)) continue;

                                        m.SetSerialPort(ref m._portNum, ref m._baudRate, ref m._parity, ref m._dataBits, ref m._stopBits);
                                        m.userCustomName = m._portNum != -1;

                                        //自定义了串口号
                                        if (m.userCustomName)
                                        {
                                            if (portName != m.PortName) continue;
                                        }

                                        SerialPort port = new SerialPort(portName, m._baudRate, m._parity, m._dataBits, m._stopBits);
                                        if (port.DiscardNull)
                                            port.DiscardNull = false;

                                        bool portOpen = false;
                                        try
                                        {
                                            port.Open(); //打开端口
                                            portOpen = port.IsOpen;
                                            if (portOpen)
                                            {
                                                if (m.VerifySerialPort(port))
                                                {
                                                    port.DataReceived += m.Port_DataReceived;
                                                    port.ErrorReceived += m.Port_ErrorReceived;
                                                    port.PinChanged += m.Port_PinChanged;

                                                    m.currentPort = port;
                                                    if (!m.userCustomName)
                                                    {
                                                        m._portName = portName;
                                                    }
                                                    m.Connected = true;
                                                    if (!usedPorts.Contains(portName))
                                                        usedPorts.Add(portName);

                                                    if (managers.All(s => s.Connected))
                                                    {
                                                        stopScan = true;
                                                    }

                                                    m.InvokeStateChanged(3, "捕获设备...");
                                                    break;
                                                }
                                                else
                                                    port.Close();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            if (!portOpen)
                                                m.InvokeStateChanged(4, "串口被占用！");
                                            System.Diagnostics.Debug.WriteLine(ex);
                                            continue;
                                        }
                                    }
                                }
                            }
                            Thread.Sleep(1000);
                        }
                        scanPortTh = null;
                    });
                    scanPortTh.Name = "scanPortTh";
                    scanPortTh.IsBackground = true;
                }

                if (managers.Any(s => !s.Connected))
                {
                    stopScan = false;
                    if (!scanPortTh.IsAlive)
                        scanPortTh.Start();
                }

                if (checkTask == null)
                {
                    checkTask = new Task(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                foreach (SerialPortManager serialPort in managers)
                                {
                                    if (serialPort.Connected == false) continue;
                                    if (serialPort.Released) continue;

                                    if ((DateTimeHelper.Now - serialPort.lastRecTime).TotalSeconds > 30)
                                    {
                                        serialPort.InvokeStateChanged(-1, "设备断开");
                                        serialPort.Close();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                LogHelper.WriteError(e, "检查任务失败");
                            }
                            Thread.Sleep(1000);
                        }
                    }, TaskCreationOptions.LongRunning);
                    checkTask.Start();
                }

                if (dealRecDataTh == null)
                {
                    dealRecDataTh = new Thread(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                recQueue.EnqueueEvent.WaitOne(1000);
                                if (recQueue.Count == 0) continue;

                                if (LineReceived != null)
                                {
                                    while (true)
                                    {
                                        int rl = recQueue.Find((byte)'\n'); //查找行尾
                                        if (rl == -1)
                                        {
                                            rl = recQueue.Find((byte)'$');  //特殊情况
                                            if (rl <= 0)
                                                break;
                                        }
                                        else
                                            rl++; //包含\n

                                        byte[] tmp = new byte[rl];
                                        int c = recQueue.DequeueRange(tmp, 0, rl);
                                        OnLineReceived(Encoding.GetString(tmp, 0, c), tmp);
                                    }
                                }
                                else
                                {
                                    recQueue.Clear();
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteError(ex, "处理接收到的串口数据失败");
                            }
                        }
                    });
                    dealRecDataTh.IsBackground = true;
                    dealRecDataTh.Name = nameof(dealRecDataTh);
                    dealRecDataTh.Start();
                }
            }
        }

        /// <summary>
        /// 获取计算机上的串口号
        /// </summary>
        /// <returns></returns>
        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }
        private void Port_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            bool b = false;
            switch (e.EventType)
            {
                case System.IO.Ports.SerialPinChange.CDChanged:
                    b = currentPort.CDHolding;
                    break;
                case System.IO.Ports.SerialPinChange.CtsChanged:
                    b = currentPort.CtsHolding;
                    break;
                case System.IO.Ports.SerialPinChange.DsrChanged:
                    b = currentPort.DsrHolding;
                    break;

                case System.IO.Ports.SerialPinChange.Ring:
                    // 如果是由 Ring 引发 PinChanged
                    // 那就意味着RI信号此时有效！
                    // 作为示例，仅仅是输出字符串"振铃...",
                    // 你可以在这里写你自己的处理操作
                    Console.WriteLine("振铃...");
                    break;

                case SerialPinChange.Break:
                    Console.WriteLine("中断...");
                    break;
                default:
                    break;
            }
        }

        private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        protected void OnBytesReceived(byte[] data)
        {
            recQueue.EnqueueRange(data, 0, data.Length);
            BytesReceived?.BeginInvoke(data, null, null);
        }

        protected void OnLineReceived(string lineStr, byte[] lineBytes)
        {
            LineReceived?.BeginInvoke(lineStr, lineBytes, null, null);
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                lastRecTime = DateTimeHelper.Now;
                int len = currentPort.BytesToRead;
                byte[] buffer = new byte[len];
                int count = currentPort.Read(buffer, 0, len);
                OnBytesReceived(buffer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 设置当前串口参数
        /// </summary>
        /// <param name="portNum">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="dataBits">数据位值</param>
        /// <param name="stopBits">停止位</param>
        protected virtual void SetSerialPort(ref int portNum, ref int baudRate, ref Parity parity, ref int dataBits, ref StopBits stopBits)
        {
            portNum = this.PortNum;
            baudRate = this.BaudRate;
            parity = this.Parity;
            dataBits = this.DataBits;
            stopBits = this.StopBits;
        }

        /// <summary>
        /// 验证当前串口是是目标串口
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        protected virtual bool VerifySerialPort(SerialPort port)
        {
            return true;
        }

        public bool Write(string text)
        {
            if (currentPort == null) return false;
            if (!currentPort.IsOpen) return false;
            if (!Connected) return false;

            try
            {
                currentPort.Write(text);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Write(byte[] buffer, int offset, int count)
        {
            if (currentPort == null) return false;
            if (!currentPort.IsOpen) return false;
            if (!Connected) return false;

            try
            {
                currentPort.Write(buffer, offset, count);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Write(char[] buffer, int offset, int count)
        {
            if (currentPort == null) return false;
            if (!currentPort.IsOpen) return false;
            if (!Connected) return false;

            try
            {
                currentPort.Write(buffer, offset, count);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool WriteLine(string text)
        {
            if (currentPort == null) return false;
            if (!currentPort.IsOpen) return false;
            if (!Connected) return false;

            try
            {
                currentPort.WriteLine(text);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 释放串口
        /// </summary>
        public void ReleasePort(bool closePort = true)
        {
            if (currentPort != null)
            {
                currentPort.DataReceived -= Port_DataReceived;
                currentPort.ErrorReceived -= Port_ErrorReceived;
                currentPort.PinChanged -= Port_PinChanged;
                if (closePort && currentPort.IsOpen)
                {
                    currentPort.Close();
                }
            }
            Released = true;
        }

        /// <summary>
        /// 关闭串口相关数据
        /// </summary>
        public virtual void Close(bool reconn = true)
        {
            lock (this)
            {
                if (currentPort != null)
                {
                    currentPort.DataReceived -= Port_DataReceived;
                    currentPort.ErrorReceived -= Port_ErrorReceived;
                    currentPort.PinChanged -= Port_PinChanged;
                    if (currentPort.IsOpen)
                    {
                        currentPort.Close();
                    }
                    currentPort = null;
                }

                Connected = false;

                if (usedPorts.Contains(_portName))
                    usedPorts.Remove(_portName);

                if (!userCustomName)
                {
                    _portName = null;
                    _portNum = -1;
                }

                if (reconn)
                {
                    Scan();
                }
            }
        }
    }

    public delegate void StateChangedHandler(int no, string msg);

    public delegate void BytesReceivedHandler(byte[] data);

    public delegate void LineReceivedHandler(string lineStr, byte[] lineBytes);
}
