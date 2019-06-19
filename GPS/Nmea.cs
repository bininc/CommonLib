using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommLiby;

namespace CommonLib.GPS
{
    public delegate void NewMessageEventHandler(NmeaMsg msg);
    public delegate void SateNumChangeEventHandler(int totalNum, int useNum, int bdNum, int gpsNum);

    public class Nmea
    {
        public event NewMessageEventHandler NewMessage;
        public event SateNumChangeEventHandler SateNumChanged;
        public event Action<bool> LocatedChanged;

        private TQueue<string> source = new TQueue<string>();

        private int gpSateNum = 0;
        private int bdSateNum = 0;
        private int useSateNum = 0;
        private bool _isLocated;

        /// <summary>
        /// 是否定位
        /// </summary>
        public bool IsLocated
        {
            get { return _isLocated; }
            set
            {
                if (_isLocated != value)
                {
                    _isLocated = value;
                    LocatedChanged?.Invoke(_isLocated);
                }
            }
        }

        public static bool AutoSyncTime
        {
            get
            {
                return ConfigHelper.GetConfigBool("Nmea_AutoSyncTime", true);
            }
            set
            {
                ConfigHelper.UpdateConfig("Nmea_AutoSyncTime", value, true);
            }
        }

        public DateTime GnssTime { get; private set; }

        public double Altitude { get; private set; }
        public int Mileage { get; private set; }

        private enum SpecialChars
        {
            CR = 13, LF = 10
        }

        public Nmea()
        {
            NmeaMsg.InitDefaults();
        }

        private void FireSateNumChanged(int useNum)
        {
            if (SateNumChanged == null) return;
            try
            {
                SateNumChanged(bdSateNum + gpSateNum, useNum, bdSateNum, gpSateNum);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void FireNewMessage(NmeaMsg msg)
        {
            if (msg == null) return;

            if (msg.id == NmeaMsg.MsgType.GPGSV)
            {
                gpSateNum = msg.GetFieldByName("NoSV").GetInt(0);
            }
            else if (msg.id == NmeaMsg.MsgType.BDGSV)
            {
                bdSateNum = msg.GetFieldByName("NoSV").GetInt(0);
            }
            else if (msg.id == NmeaMsg.MsgType.GGA)
            {
                int useNum = msg.GetFieldByName("NoSv").GetInt(0);

                if (useNum != useSateNum || useNum == 0)
                {
                    if ((DateTimeHelper.Now - NmeaMsg.GetMsgHandle(NmeaMsg.MsgType.BDGSV).LastNmeaTime).TotalSeconds > 5)
                    {//大于5秒认为模式切换
                        bdSateNum = 0;
                    }
                    if ((DateTimeHelper.Now - NmeaMsg.GetMsgHandle(NmeaMsg.MsgType.GPGSV).LastNmeaTime).TotalSeconds > 5)
                    { //大于5秒认为模式切换
                        gpSateNum = 0;
                    }
                    useSateNum = useNum;
                }
                FireSateNumChanged(useNum);

                if (IsLocated)
                {
                    Altitude = msg.GetFieldByName("msl").GetDouble(0);
                }

            }
            else if (msg.id == NmeaMsg.MsgType.RMC)
            {
                char status = msg.GetFieldByName("status").GetChar('V');
                IsLocated = status == 'A';

                TimeSpan time = msg.GetFieldByName("time").GetTime(DateTime.UtcNow.TimeOfDay);
                DateTime date = msg.GetFieldByName("date").GetDate(DateTime.UtcNow.Date);
                GnssTime = date.Add(time).ToLocalTime();
                if (AutoSyncTime)
                {
                    if (Math.Abs((DateTime.Now - GnssTime).TotalSeconds) > 30)
                    {
                        //系统时间与GPS时间相差 30秒校准系统时间
                        DateTimeHelperEx.SetSystemTime(GnssTime);
                        DateTimeHelper.Now = GnssTime;
                    }
                }
            }
            else if (msg.id == NmeaMsg.MsgType.GPRS)
            {
                Mileage = msg.GetFieldByName("mileage").GetInt(0);
            }

            if (NewMessage != null)
            {
                try
                {
                    NewMessage(msg);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        public TQueue<string> Source
        {
            get
            {
                return source;
            }
        }

        public bool HasSource
        {

            get { return source.Count > 0; }
        }

        private Task workThread = null;

        public void Start()
        {
            Stop();
            workThread = new Task(DoWork, TaskCreationOptions.LongRunning);
            ShouldStop = false;
            workThread.Start();
        }

        public void Stop()
        {
            if (IsRunning)
            {
                ShouldStop = true;
            }
        }

        public bool IsRunning
        {
            get
            {
                return (workThread != null);
            }
        }


        protected bool ShouldStop { get; set; }

        private void DoWork()
        {
            while (!ShouldStop) //循环读取
            {
                source.EnqueueEvent.WaitOne(1000);
                if (source.Count > 0)
                {
                    try
                    {
                        for (string line = ReadLine(); line != null; line = ReadLine())
                        {
                            if (ShouldStop)
                            {
                                break;
                            }
                            if (line.Length <= 0)
                            {
                                continue;
                            }
                            ParseLine(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Error e = new Error(null, ex);
                        FireNewMessage(e);
                    }
                }
            }

            //workThread.Dispose();
            workThread = null;
            FireNewMessage(new NmeaMsg(NmeaMsg.MsgType.Done));
        }

        private void ParseLine(string line)
        {
            if (line == null || line.Length <= 0)
            {
                return;
            }

            NmeaMsg outMsg = null;
            try
            {
                int csIndex = line.LastIndexOf('*');
                if (csIndex <= 0)
                {
                    return;
                }
                csIndex++;
                if (csIndex >= line.Length)
                {
                    return;
                }
                string cs = line.Substring(csIndex, line.Length - csIndex);
                string tempLine = line.Substring(0, csIndex - 1);
                string[] parts = tempLine.ToUpper().Split(',');
                if (parts.Length <= 0)
                {
                    return;
                }
                if (!ValidateChecksum(tempLine, cs))
                {
                    return;
                }
                outMsg = NmeaMsg.Parse(parts);
            }
            catch (Exception ex)
            {
                outMsg = new Error(line, ex);
            }
            FireNewMessage(outMsg);
        }

        private bool ValidateChecksum(string line, string cs)
        {
            int crc = Convert.ToInt32(cs, 16);
            int checksum = 0;
            for (int i = 1; i < line.Length; i++)
            {
                if (ShouldStop)
                {
                    break;
                }
                checksum ^= Convert.ToByte(line[i]);
            }
            return (checksum == crc);
        }

        private string ReadLine()
        {
            if (!HasSource) return null;
            try
            {
                string line = source.Dequeue();
                line = line.TrimEnd((char)SpecialChars.CR, (char)SpecialChars.LF);
                if (line.Contains("\r\n"))
                    return ReadLine();
                return line;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return null;
            }
            /*
            StringBuilder line = new StringBuilder();
            int c = -1;
            while (true)
            {
                if (ShouldStop)
                {
                    break;
                }
                c = source.ReadByte();
                if (c < 0)
                {

                    if (line.Length <= 0)
                    {

                        return null;

                    }
                    break;

                }
                if ((c == (int)SpecialChars.LF)
                    || (c == (int)SpecialChars.CR))
                {

                    if (line.Length <= 0)
                    {

                        continue;

                    }
                    break;

                }
                line.Append((char)c);

            }
            //System.Diagnostics.Debug.WriteLine(count.ToString() + ": " + line.ToString());
            return line.ToString();*/
        }

        /// <summary>
        /// 输入数据行
        /// </summary>
        public void InputLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            source.Enqueue(line);
        }

    }//EOC
}
