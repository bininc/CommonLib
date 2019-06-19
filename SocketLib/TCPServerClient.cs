﻿using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CommLiby;
using CommLiby.SocketLib;

namespace CommonLib.SocketLib
{
    public delegate void DataReceivedHandler(TCPServerClient client, int head, byte[] buffer, string strdata = null);
    public delegate void DisplayMsgHandler(TCPServerClient client, string msg, DateTime time);
    public class TCPServerClient : INotifyPropertyChanged
    {
        public delegate void CloseHandler(TCPServerClient client);
        /// <summary>
        /// 客户端连接关闭事件
        /// </summary>
        public event CloseHandler ClientClosed;
        /// <summary>
        /// 收到数据事件
        /// </summary>
        public event DataReceivedHandler DataReceived;
        /// <summary>
        /// 消息显示事件
        /// </summary>
        public event DisplayMsgHandler DisplayMsg;
        /// <summary>
        /// 字段改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 调用字段改变事件
        /// </summary>
        /// <param name="propertyName"></param>
        private void InvokePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public TCPServerClient(Socket sokClient, bool _trustClientValidate)
        {
            SokClient = sokClient;
            id = SokClient.RemoteEndPoint.ToString();
            StateObject state = new StateObject(SokClient);
            SokClient.BeginReceive(state.Buffer, 0, state.BufferSize, SocketFlags.None, ReceiveCallback, state);
            connTime = dataTime = DateTimeHelper.Now;
            countTimer = new Timer(CountTimerCallBack, null, 1000, 3000);
            DocID = "未知";
            this.trustClientValidate = _trustClientValidate;
        }

        private Timer countTimer;   //倒计时Timer
        private bool trustedClient; //可信连接
        private DateTime connTime;  //连接时的时间
        private DateTime dataTime;  //收到数据时间
        private bool trustClientValidate = false;   //可信连接验证
        private void CountTimerCallBack(object state)
        {
            if (trustClientValidate)
            {
                if (!trustedClient && (DateTimeHelper.Now - connTime).TotalSeconds >= 120)
                {
                    CloseConnection("验证超时");    //超过2分钟未检测到握手包 关闭连接
                    countTimer.Dispose();   //销毁倒计时
                }
                else
                {
                    if (trustedClient && (DateTimeHelper.Now - connTime).TotalSeconds >= 60)
                    {
                        CloseConnection("客户端掉线");    //超过1分钟未检测到心跳包 关闭连接 
                        countTimer.Dispose();   //销毁倒计时
                    }
                }
            }
            else
            {
                if ((DateTimeHelper.Now - dataTime).TotalSeconds >= 60)  
                {
                    CloseConnection("超时", false);     //超过1分钟未收到数据 关闭连接
                    countTimer.Dispose();   //销毁倒计时
                }
            }
        }

        #region 字段
        /// <summary>
        /// 与客户端通信Socket
        /// </summary>
        public Socket SokClient;

        private string id;
        /// <summary>
        /// 识别ID
        /// </summary>
        public string ID
        {
            get
            {
                if (SokClient != null)
                {
                    id = SokClient.RemoteEndPoint.ToString();
                    return id;
                }
                if (!string.IsNullOrWhiteSpace(id))
                    return id;

                return null;
            }
        }
        /// <summary>
        /// 客户端版本
        /// </summary>
        public string ClientVer { get; private set; }
        /// <summary>
        /// 健康师ID
        /// </summary>
        public string DocID { get; private set; }
        /// <summary>
        /// 健康师姓名
        /// </summary>
        public string DocName { get; private set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name
        {
            get { return string.Format("[{0}] {1}_{2}", ID, DocName, DocID); }
        }
        /// <summary>
        /// 是否显示所有数据
        /// </summary>
        public bool ShowAllData { get; set; }
        #endregion

        #region 向客户端发送数据
        /// <summary>
        /// 向客户端发送字节
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public bool SendBS(byte[] bs)
        {
            try
            {
                if (SokClient == null) return false;
                int count = SokClient.Send(bs);
                return count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return false;
            }
        }
        /// <summary>
        /// 向客户端发送字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SendString(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return false;
            try
            {
                int head = 0;   //0表示发送字符串
                byte[] headBS = BitConverter.GetBytes(head);
                byte[] msgBS = Encoding.UTF8.GetBytes(msg);
                byte[] sendBytes = new byte[headBS.Length + msgBS.Length];
                headBS.CopyTo(sendBytes, 0);
                msgBS.CopyTo(sendBytes, 4);
                if (DisplayMsg != null)
                    DisplayMsg(this, "向客户端发送文字：" + msg, DateTimeHelper.Now);
                return SendBS(sendBytes);
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 向客户端发送命令
        /// </summary>
        /// <param name="head">命令ID</param>
        /// <param name="buffer">命令数据</param>
        /// <returns></returns>
        public bool SendCommand(int head, byte[] buffer)
        {
            //系统已占用命令码
            if (head == 7777 || head == 8888 || head == 9999) return false;
            try
            {
                byte[] headBS = BitConverter.GetBytes(head);
                byte[] sendBytes = new byte[headBS.Length + buffer.Length];
                headBS.CopyTo(sendBytes, 0);
                buffer.CopyTo(sendBytes, 4);
                if (DisplayMsg != null)
                    DisplayMsg(this, "向客户端发送命令：" + head, DateTimeHelper.Now);
                return SendBS(sendBytes);
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 向客户端发送命令
        /// </summary>
        /// <param name="head"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SendCommand(int head, string msg)
        {
            //系统已占用命令码
            if (head == 7777 || head == 8888 || head == 9999) return false;
            if (string.IsNullOrWhiteSpace(msg)) return false;
            try
            {
                byte[] headBS = BitConverter.GetBytes(head);
                byte[] buffer = Encoding.UTF8.GetBytes(msg);
                byte[] sendBytes = new byte[headBS.Length + buffer.Length];
                headBS.CopyTo(sendBytes, 0);
                buffer.CopyTo(sendBytes, 4);
                if (DisplayMsg != null)
                    DisplayMsg(this, "向客户端发送命令：" + head + "->" + msg, DateTimeHelper.Now);
                return SendBS(sendBytes);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 接收客户端发送过来的数据
        void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            bool disposed = false;  //是否停止数据接收
            int length = 0; //接收到的数据长度
            try
            {
                //接收 对应 客户端发来的消息
                length = ((Socket)state.State).EndReceive(ar);
                byte[] receiveBytes = new byte[length];
                Array.Copy(state.Buffer, 0, receiveBytes, 0, length);

                dataTime = DateTimeHelper.Now;    //更新收到数据时间

                int head = -1;  //消息头码
                string strData = null;  //String类型消息
                if (length >= 4)
                {
                    //消息头码
                    head = BitConverter.ToInt32(new[] { receiveBytes[0], receiveBytes[1], receiveBytes[2], receiveBytes[3] }, 0);

                    if (head == 8888) //握手头码
                    {
                        trustedClient = true;
                        SendHandShake();
                        if (ShowAllData && DisplayMsg != null)
                            DisplayMsg(this, "收到握手包，客户端确认", DateTimeHelper.Now);
                    }
                    else if (head == 7777) //心跳信息
                    {
                        if (trustedClient)
                            connTime = DateTimeHelper.Now;
                        strData = ParserString(receiveBytes);
                        string[] infos = strData.Split(';');
                        ClientVer = infos[0];
                        DocID = string.IsNullOrWhiteSpace(infos[1]) ? "未登录" : infos[1];
                        DocName = infos[2];
                        if (ShowAllData && DisplayMsg != null)
                            DisplayMsg(this, "收到心跳包：" + strData, DateTimeHelper.Now);
                        InvokePropertyChanged("Name");
                    }
                    else if (head == 9999) //请求断开连接
                    {
                        CloseConnection("客户端退出");
                    }
                    else if (head == 0 && trustedClient) //发送来的是消息
                    {
                        strData = ParserString(receiveBytes);
                        if (DisplayMsg != null)
                            DisplayMsg(this, "收到文字消息：" + strData, DateTimeHelper.Now);
                    }
                    else
                    {
                        if (ShowAllData && DisplayMsg != null)
                            DisplayMsg(this, "收到未知消息", DateTimeHelper.Now);
                    }
                }
                if (DataReceived != null)
                {
                    try
                    {
                        DataReceived(this, head, receiveBytes, strData);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    disposed = true;
                    return; //停止接收数据
                }
                if (ex is SocketException)
                {
                    CloseConnection("客户端掉线");
                    return;
                }
                LogHelper.WriteError(ex, "TCP");
                if (DisplayMsg != null)
                    DisplayMsg(this, "客户端消息处理失败", DateTimeHelper.Now);
            }
            finally
            {
                try
                {
                    if (!disposed && length != 0)
                        //继续接收消息
                        ((Socket)state.State).BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
                    if (length == 0)
                        CloseConnection("客户端关闭连接", false);
                }
                catch
                {
                    CloseConnection("接收消息异常");
                }
            }
        }
        #endregion

        #region 其他连接处理
        /// <summary>
        /// 关闭与客户端连接
        /// </summary>
        public void CloseConnection(string errMsg = null, bool sendClose = true)
        {
            try
            {
                if (SokClient != null)
                {
                    if (sendClose)
                        SendBS(BitConverter.GetBytes(9999));
                    SokClient.Shutdown(System.Net.Sockets.SocketShutdown.Receive);
                    SokClient.Disconnect(true);
                    SokClient.Close();
                    SokClient = null;
                    if (DisplayMsg != null)
                        DisplayMsg(this, "断开连接：" + (string.IsNullOrWhiteSpace(errMsg) ? "" : errMsg), DateTimeHelper.Now);
                    if (ClientClosed != null)
                        ClientClosed(this);
                }
            }
            catch { }
            GC.Collect();
        }
        /// <summary>
        /// 发送握手包
        /// </summary>
        /// <returns></returns>
        public bool SendHandShake()
        {
            byte[] handBytes = BitConverter.GetBytes(8888);
            return SendBS(handBytes);
        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 转换字符串方法
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ParserString(byte[] buffer)
        {
            byte[] arrMsg = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, arrMsg, 0, arrMsg.Length);
            string strMsg = Encoding.UTF8.GetString(arrMsg, 0, arrMsg.Length);
            return strMsg;
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        public void Dispose()
        {
            if (countTimer != null)
                countTimer.Dispose();
            if (SokClient != null)
                SokClient.Dispose();
        }
    }
}
