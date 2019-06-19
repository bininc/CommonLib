using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommLiby;
using Timer = System.Threading.Timer;

namespace CommonLib.SerialPorts
{
    public class Ymode
    {
        enum YmodeSendState
        {
            WAIT_START,  //默认
            SEND_FILENAME,
            SEND_DATA,
            SEND_EOT,
            SEND_LASTPACK,
            SEND_CA      //取消发送
        };

        private SerialPort comn = null;
        private Task tskConnect = null;
        private bool start = false;

        private const byte PACKET_HEADER = 3;
        private const byte PACKET_TRAILER = 2;
        private const byte PACKET_OVERHEAD = PACKET_HEADER + PACKET_TRAILER;
        private const byte PACKET_SIZE = 128;
        private const UInt16 PACKET_1K_SIZE = 1024;

        private const byte SOH = 0x01;  /* start of 128-byte data packet */
        private const byte STX = 0x02;  /* start of 1024-byte data packet */
        private const byte EOT = 0x04;  /* end of transmission */
        private const byte ACK = 0x06;  /* acknowledge */
        private const byte NAK = 0x15;  /* negative acknowledge */
        private const byte CA = 0x18;  /* two of these in succession aborts transfer */

        private const byte CRC16 = 0x43;  /* 'C' == 0x43, request 16-bit CRC */
        private const byte ABORT1 = 0x41;  /* 'A' == 0x41, abort by user */
        private const byte ABORT2 = 0x61;  /* 'a' == 0x61, abort by user */

        private YmodeSendState transmitState = YmodeSendState.WAIT_START;
        private bool transmittingYmodem = false;


        private byte[] packet_data = new byte[PACKET_1K_SIZE + PACKET_OVERHEAD];
        private UInt16 pktSize = 0;
        private UInt32 resSize = 0;  //文件未发送部分的大小
        private byte blkNumber = 0;
        private bool ackReceived = false;
        private byte errors = 0;
        private byte lastRec = 0;
        private YmodeSendState lastState = YmodeSendState.WAIT_START;
        private bool fsFWSent = false;

        private byte packCheckSum;
        private UInt16 packCRC;
        private byte[] fileBytes;
        private string fileName;
        private int fileIndex;
        private TQueue<byte> recData = new TQueue<byte>();

        public delegate void YmodeResponseGotHandler(string msg);
        public event YmodeResponseGotHandler YmodeResponseGot;

        public delegate void YmodeProgressUpdatedHandler(int blkSentCnt);
        public event YmodeProgressUpdatedHandler YmodeProgressUpdated;

        public delegate void YmodeProgressFinishedHandler(string result);
        public event YmodeProgressFinishedHandler YmodeProgressFinished;

        public delegate void DataSendFailedHandler();
        public event DataSendFailedHandler DataSendFailed;

        public delegate void ResponseTimeoutHandler();
        public event ResponseTimeoutHandler ResponseTimeout;

        public event StateChangedHandler YmodeStateChanged;

        public Ymode()
        {
            tskConnect = new Task(timConnect_Tick, null, TaskCreationOptions.LongRunning);
            tskConnect.Start();
        }

        public void Init(SerialPort port, byte[] fileBytes, string fileName = null)
        {
            if (port == null || fileBytes == null || fileBytes.Length == 0) return;
            if (start) return;

            comn = port;
            comn.DataReceived += this.comn_DataReceived;   //注册事件。
            this.fileBytes = fileBytes;
            if (string.IsNullOrWhiteSpace(fileName))
                this.fileName = "UpdateImg";
            else
                this.fileName = fileName;
            this.fileIndex = 0;
            start = true;
        }

        public void Close()
        {
            if (comn != null)
                comn.DataReceived -= this.comn_DataReceived;
            start = false;

            transmitState = YmodeSendState.WAIT_START;
            transmittingYmodem = false;
            pktSize = 0;
            resSize = 0;
            blkNumber = 0;
            ackReceived = false;
            errors = 0;
            fsFWSent = false;
            fileBytes = null;
            fileName = null;
            fileIndex = 0;
            lastRec = 0;
        }

        private int tickNum = 1;
        private void timConnect_Tick(object state)
        {
            while (true)
            {
                Thread.Sleep(200);
                if (start)
                {
                    if (!transmittingYmodem)
                    {
                        if (tickNum++ > 15)
                        {
                            transmitState = YmodeSendState.WAIT_START;
                            pktSize = 0;
                            resSize = 0;
                            blkNumber = 0;
                            ackReceived = false;
                            errors = 0;
                            fsFWSent = false;
                            fileIndex = 0;
                            lastRec = 0;
                            YmodeStateChanged?.Invoke(11, "请断电，重连设备...");
                            tickNum = 1;
                        }
                    }
                    else
                    {
                        tickNum = 1;

                        bool next = recData.Count > 0;
                        while (next)
                        {
                            next = Ymodem_Transmit() == 0 && recData.Count > 0;
                        }
                    }
                }
            }
        }


        private UInt16 CalcFileCRC16(ref FileInfo fi)
        {
            UInt16 crc = 0;
            FileStream fs = fi.Open(FileMode.Open, FileAccess.Read);

            fs.Seek(0, SeekOrigin.Begin); //定位到文件流开头位置

            for (UInt32 i = 0; i < fs.Length; i++)
            {
                crc = UpdateCRC16(crc, (byte)fs.ReadByte());
            }
            fs.Close();

            crc = UpdateCRC16(crc, 0);
            crc = UpdateCRC16(crc, 0);
            return (UInt16)(crc & 0xffffu);

        }


        /**
          * @brief  Prepare the first block
          * @param  
          *     
          */
        private void Ymodem_PrepareIntialPacket(ref byte[] data, string fileName, UInt32 length)
        {
            UInt16 i, j;
            byte[] file_ptr = new byte[11]; //UInt32类型数据最多10位十进制，加串尾一个字节

            /* Make first three packet */
            data[0] = SOH;
            data[1] = 0x00;
            data[2] = 0xFF;

            /* Filename packet has valid data */
            for (i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '\0')
                {
                    break;
                }
                data[i + PACKET_HEADER] = (byte)fileName[i];
            }

            data[i + PACKET_HEADER] = 0x00;

            Int2Str(ref file_ptr, length);
            for (j = 0, i = (UInt16)(i + PACKET_HEADER + 1); file_ptr[j] != '\0';)
            {
                data[i++] = file_ptr[j++];
            }

            for (j = i; j < PACKET_SIZE + PACKET_HEADER; j++)
            {
                data[j] = 0;
            }
        }

        /**
          * @brief  Prepare the data packet
          * @param  
          *     
          */
        private void Ymodem_PreparePacket(ref byte[] data, byte pktNo, UInt32 sizeBlk)
        {
            UInt16 i, size, packetSize;

            /* Make first three packet */
            packetSize = (sizeBlk > PACKET_SIZE) ? PACKET_1K_SIZE : PACKET_SIZE;   //超过128字节，则补全1024字节
            size = (UInt16)((sizeBlk < packetSize) ? sizeBlk : packetSize);        //计算待读取的字节数
            if (packetSize == PACKET_1K_SIZE)
            {
                data[0] = STX;
            }
            else
            {
                data[0] = SOH;
            }
            data[1] = pktNo;
            data[2] = (byte)(~pktNo);

            /* Data packet has valid data */
            for (i = PACKET_HEADER; i < size + PACKET_HEADER; i++)
            {
                data[i] = fileBytes[fileIndex++];
            }
            if (size <= packetSize) //补足数据包空位
            {
                for (i = (UInt16)(size + PACKET_HEADER); i < packetSize + PACKET_HEADER; i++)
                {
                    data[i] = 0x1A; /* EOF (0x1A) or 0x00 */
                }
            }
        }

        /**
          * @brief  Convert an Integer to a string
          * @param  str: The string
          * @param  intnum: The intger to be converted
          * @retval None
          */
        private void Int2Str(ref byte[] str, UInt32 intnum)
        {
            UInt32 i, Div = 1000000000, j = 0, Status = 0; //UInt32类型数据最多10位十进制

            for (i = 0; i < 10; i++)
            {
                str[j++] = (byte)((intnum / Div) + 48);//‘0’的ASCII值是48

                intnum = intnum % Div;
                Div /= 10;
                if ((str[j - 1] == '0') & (Status == 0)) //判断非零的最高位
                {
                    j = 0;
                }
                else
                {
                    Status++;
                }
            }
            //字符串尾加入结束标志
            str[j] = (byte)'\0';
        }


        /**
          * @brief  Update CRC16 for input byte
          * @param  CRC input value 
          * @param  input byte
           * @retval None
          */
        private UInt16 UpdateCRC16(UInt16 crcIn, byte data)
        {
            UInt32 crc = crcIn;
            UInt32 input = (UInt32)(data | 0x100);
            do
            {
                crc <<= 1;
                input <<= 1;
                if ((input & 0x100) == 0x100)
                    ++crc;
                if ((crc & 0x10000) == 0x10000)
                    crc ^= 0x1021;
            } while ((input & 0x10000) == 0);
            return (UInt16)(crc & 0xffffu);
        }

        /**
          * @brief  Cal CRC16 for YModem Packet
          * @param  data
          * @param  offset 偏移字节数
          * @param  size
           *@retval UInt16
          */
        private UInt16 Cal_CRC16(ref byte[] data, UInt32 offset, UInt32 size)
        {
            UInt16 crc = 0;
            UInt32 index = offset; //前三个数据不需要计入
            while (index < (size + offset))  //总计size个数据
            {
                crc = UpdateCRC16(crc, data[index++]);
            }

            crc = UpdateCRC16(crc, 0);
            crc = UpdateCRC16(crc, 0);
            return (UInt16)(crc & 0xffffu);
        }

        /**
          * @brief  Cal Checksum for YModem Packet
          * @param  data
          * @param  offset 偏移字节数
          * @param  length
          * @retval  byte
          */
        private byte CalChecksum(ref byte[] data, UInt32 offset, UInt32 size)
        {
            UInt32 sum = 0;
            UInt32 index = offset;
            while (index < (size + offset))
            {
                sum += data[index++];
            }
            return (byte)(sum & 0xffu);
        }


        /**
          * @brief  Transmit a file using the ymodem protocol
          * @param  
          * @retval 
          */
        private byte Ymodem_Transmit()
        {
            byte i;
            bool CRC16_F = true;
            byte c;

            ackReceived = false;  //默认
            c = ReadNextByte();

            //先判断是否结束传输或改变状态
            switch (transmitState)
            {
                case YmodeSendState.WAIT_START:
                    if (c == CRC16)
                    {
                        ackReceived = true;

                        YmodeResponseGot?.Invoke("\n\r The file is being sent ...\n\r");
                        YmodeStateChanged?.Invoke(13, "发送文件信息");
                        lastState = transmitState;
                        transmitState = YmodeSendState.SEND_FILENAME; //开始发送文件名及其大小                        
                    }
                    break;
                case YmodeSendState.SEND_FILENAME:
                    if (c == ACK)
                    {
                        YmodeStateChanged?.Invoke(13, "文件信息确认");
                        lastState = transmitState;
                    }
                    else
                    {
                        if (c == CRC16)
                        {
                            if (lastState == YmodeSendState.SEND_FILENAME && lastRec == ACK)
                            {
                                ackReceived = true;
                                YmodeStateChanged?.Invoke(13, "开始发送文件");
                                transmitState = YmodeSendState.SEND_DATA; //开始发送文件内容
                            }
                            else
                            {
                                errors++;
                                if (errors > 10)
                                    ackReceived = true;
                            }
                        }
                    }
                    break;
                case YmodeSendState.SEND_DATA:
                    if (c == ACK)
                    {
                        ackReceived = true;
                        YmodeProgressUpdated?.Invoke(fileIndex);
                        if (fsFWSent)
                        {
                            transmitState = YmodeSendState.SEND_EOT; //当前文件内容发送完毕
                        }
                    }
                    break;
                case YmodeSendState.SEND_EOT:
                    if (c == ACK)
                    {
                        lastState = transmitState;
                        transmitState = YmodeSendState.SEND_LASTPACK;
                    }
                    break;
                case YmodeSendState.SEND_LASTPACK:
                    if (c == CRC16 && lastState == YmodeSendState.SEND_EOT && lastRec == ACK)
                    {
                        ackReceived = true;
                    }
                    else if (c == ACK)
                    {
                        start = false;
                        transmittingYmodem = false; //结束Ymodem发送
                                                    //通知“固件升级”子窗口固件升级成功
                                                    //rMainform.ReportFwUpdateResult("Succeed");
                        YmodeProgressFinished?.Invoke("Succeed");
                        YmodeStateChanged?.Invoke(14, "文件发送成功");
                        return 1; //直接返回，然后可以发送APP代码的CRC16值
                    }
                    break;
                case YmodeSendState.SEND_CA:
                    if (c == ACK)
                    {
                        ackReceived = true;
                        transmittingYmodem = false; //结束Ymodem发送
                        return 0;
                    }
                    break;
                default:
                    break;
            }

            lastRec = c;

            if (c == CA)
            {
                if (ReadNextByte() == CA) //连续两次读到CA 
                {
                    transmittingYmodem = false;
                    YmodeStateChanged?.Invoke(11, "发送失败，请重试...");
                    return 0xFF; /*  error */
                }
            }

            //根据新状态来判断是否要准备新数据包
            switch (transmitState)
            {
                case YmodeSendState.SEND_FILENAME:
                    if (ackReceived) //下位机等待数据包下位机接收成功
                    {
                        errors = 0; //清零出错计数
                                    // Here 128 bytes package is used
                        pktSize = PACKET_SIZE;

                        // Prepare first block，对应数据包序号为0
                        Ymodem_PrepareIntialPacket(ref packet_data, fileName, (uint)fileBytes.Length);
                        fsFWSent = false;
                        resSize = (uint)fileBytes.Length;
                        fileIndex = 0;
                        blkNumber = 1; //接下来发送第1个数据包
                    }
                    else
                    {
                        errors++;
                    }
                    break;

                case YmodeSendState.SEND_DATA:
                    if (ackReceived) //上次数据包下位机接收成功
                    {
                        errors = 0; //清零出错计数
                                    // Prepare next packet
                        Ymodem_PreparePacket(ref packet_data, blkNumber, resSize);

                        /*
                        //修改第一个数据包，加入CRC值和文件长度
                        if (blkNumber == 1)
                        {
                            UInt16 crc = CalcFileCRC16(ref fiFW);  
                        }
                        */

                        if (resSize >= PACKET_1K_SIZE) //剩余字节数大于等于1024
                        {
                            // Here 1024 bytes package is used to send the packets
                            pktSize = PACKET_1K_SIZE;
                        }
                        else if (resSize > PACKET_SIZE) //剩余字节数大于128且小于1024
                        {
                            pktSize = PACKET_1K_SIZE;
                        }
                        else //剩余字节数小于等于128,且大于0
                        {
                            pktSize = PACKET_SIZE;
                        }
                        if (resSize > PACKET_1K_SIZE) //判断是否还有数据包需要发送
                        {
                            resSize -= PACKET_1K_SIZE;
                            if (blkNumber > (fileBytes.Length / 1024)) //发送的数据包数量不能超出数据包总数量
                            {
                                transmittingYmodem = false;
                                return 0xFF; //  error 
                            }
                            else
                            {
                                blkNumber++;
                            }
                        }
                        else
                        {
                            blkNumber++;
                            resSize = 0;
                            fsFWSent = true; //文件数据发送完毕，准备进入下一状态
                        }
                    }
                    else
                    {
                        errors++;
                    }
                    break;

                case YmodeSendState.SEND_LASTPACK: //发送序号为0的空数据包，结束文件传输
                    if (ackReceived) //上次数据包发送成功
                    {
                        errors = 0;
                        // Last packet preparation 
                        packet_data[0] = SOH;
                        packet_data[1] = 0;
                        packet_data[2] = 0xFF;
                        for (i = PACKET_HEADER; i < (PACKET_SIZE + PACKET_HEADER); i++)
                        {
                            packet_data[i] = 0x00;
                        }
                        pktSize = PACKET_SIZE;
                    }
                    break;
                case YmodeSendState.SEND_EOT:
                case YmodeSendState.SEND_CA:
                    if (ackReceived) //上次数据包发送成功
                    {
                        errors = 0;
                    }
                    else
                    {
                        errors++;
                    }
                    break;
                default:
                    break;
            }

            if (errors >= 0xFF)
            {
                transmittingYmodem = false;
                //通知“固件升级”子窗口固件升级失败
                //rMainform.ReportFwUpdateResult("Fail");
                YmodeProgressFinished?.Invoke("Fail");
                YmodeStateChanged?.Invoke(11, "发送失败...");
                return errors;
            }
            else //发送上次的数据包或新数据包
            {
                if ((transmitState == YmodeSendState.SEND_FILENAME) ||
                    (transmitState == YmodeSendState.SEND_DATA) ||
                    (transmitState == YmodeSendState.SEND_LASTPACK))
                {
                    bool send = false;
                    if (transmitState == YmodeSendState.SEND_FILENAME && ackReceived)
                    {
                        send = true;
                        System.Diagnostics.Debug.WriteLine("发送文件名大小");
                    }
                    else if (transmitState == YmodeSendState.SEND_DATA && ackReceived)
                    {
                        send = true;
                        System.Diagnostics.Debug.WriteLine("发送文件内容:" + (blkNumber - 1));
                    }
                    else if (transmitState == YmodeSendState.SEND_LASTPACK && ackReceived)
                    {
                        send = true;
                        System.Diagnostics.Debug.WriteLine("发送最后空数据");
                    }

                    if (send) //新数据包待发送，重新计算校验值
                    {
                        // Send CRC or CheckSum based on CRC16_F 
                        if (CRC16_F)
                        {
                            packCRC = Cal_CRC16(ref packet_data, 3, pktSize); //前3个数据不包含在CRC计算范围内
                        }
                        else
                        {
                            packCheckSum = CalChecksum(ref packet_data, 3, pktSize); //前3个数据不包含在CRC计算范围内
                        }

                        //send packet
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("CRC16:" + packCRC);
                            int len = pktSize + PACKET_HEADER;
                            // Send CRC or CheckSum based on CRC16_F 
                            if (CRC16_F)
                            {
                                packet_data[len] = (byte)(packCRC >> 8);
                                packet_data[len + 1] = (byte)(packCRC & 0xFF);
                                len += 2;
                            }
                            else
                            {
                                packet_data[len] = packCheckSum;
                                len += 1;
                            }

                            comn.Write(packet_data, 0, len);
                            ackReceived = false; //在接收事件处理函数中判断是否发送成功

                        }
                        catch (Exception e)
                        {
                            transmittingYmodem = false;
                            DataSendFailed?.Invoke();
                            YmodeStateChanged?.Invoke(11, "串口通信错误");
                            return 0xFF; /*  error */
                        }
                    }
                }

                else if (transmitState == YmodeSendState.SEND_EOT) //send EOT
                {
                    try
                    {
                        if (ackReceived)
                        {
                            comn.Write(new[] { EOT }, 0, 1);
                            ackReceived = false; //在接收事件处理函数中判断是否发送成功
                            System.Diagnostics.Debug.WriteLine("发送EOT");
                        }
                    }
                    catch (Exception e)
                    {
                        transmittingYmodem = false;
                        YmodeStateChanged?.Invoke(11, "串口通信错误");
                        return 0xFF; //  error 
                    }
                }
            }

            return 0; /* packet trasmitted successfully */
        }

        void comn_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!start) return;
            try
            {
                if (!transmittingYmodem)
                {
                    string msg = string.Empty;
                    if (transmitState == YmodeSendState.WAIT_START)
                    {
                        char c = (char)comn.ReadChar();
                        msg += c;
                        //接收到'C'--CRC16
                        if (c == 'C')
                        {
                            //rMainform.ShowMCUMsg("\n\r Waiting for the file to be sent ...\n\r");
                            transmittingYmodem = true;
                            YmodeStateChanged?.Invoke(12, "进入Ymode模式");
                            recData.Clear();
                        }
                        else
                        {
                            msg += comn.ReadExisting(); //读取串口缓冲区数据
                            comn.Write(new[] { (byte)0x31 }, 0, 1);
                        }
                    }
                    else  //文件发送完毕后的信息
                    {
                        msg += comn.ReadExisting(); //读取串口缓冲区数据
                    }
                    YmodeResponseGot?.Invoke(msg);
                }
                else
                {
                    byte[] temp = new byte[128];
                    int count = comn.Read(temp, 0, temp.Length);
                    if (count > 0)
                    {
                        recData.EnqueueRange(temp, 0, count);
                    }
                }
            }
            catch (Exception Err)
            {
                YmodeStateChanged?.Invoke(11, "串口通信错误");
            }
        }

        private byte ReadNextByte()
        {
            if (recData.Count > 0)
            {
                byte c = recData.Dequeue();
                System.Diagnostics.Debug.WriteLine(c);
                YmodeResponseGot?.Invoke((char)c + " ");
                return c;
            }
            else
            {
                return 0xFF;   //  error 
            }
        }
    }
}
