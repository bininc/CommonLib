using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CommonLib.WinAPI
{
    public class ShareMemoryManager : IDisposable
    {
        private Semaphore m_Write;  //可写的信号
        private Semaphore m_Read;  //可读的信号
        public IntPtr handle;     //文件句柄
        public IntPtr addr;       //共享内存地址
        public readonly string Name; //名字
        public readonly uint Length;            //共享内存长
        private readonly string m_w_name;   //写信号量名字
        private readonly string m_r_name;    //度信号量名字
        private readonly string share_name; //共享内存名称
        private Thread threadRed;
        /// <summary>
        /// 收到数据事件
        /// </summary>
        public event Action<byte[], string> DataReceived;

        /// <summary>
        /// 创建一个共享内存管理器
        /// </summary>
        /// <param name="name">名称(相互通信的标识)</param>
        /// <param name="length">共享内存长度</param>
        public ShareMemoryManager(string name, uint length = 102400)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "名称不能为空");

            Name = name;
            m_w_name = $"WriteMap_{name}";
            m_r_name = $"ReadMap_{name}";
            share_name = $"shareMemory_{name}";
            Length = length;
        }

        ///<summary>
        /// 初始化共享内存数据 开始读取
        ///</summary>
        public void BeginReceiveData()
        {
            lock (this)
            {
                if (threadRed != null) return;

                m_Write = new Semaphore(1, 1, m_w_name);//开始的时候有一个可以写
                m_Read = new Semaphore(0, 1, m_r_name);//没有数据可读
                IntPtr hFile = new IntPtr(Kernel32.INVALID_HANDLE_VALUE);
                handle = Kernel32.CreateFileMapping(hFile, 0, Kernel32.PAGE_READWRITE, 0, Length, share_name);
                addr = Kernel32.MapViewOfFile(handle, Kernel32.FILE_MAP_ALL_ACCESS, 0, 0, 0);

                threadRed = new Thread(ThreadReceive);
                threadRed.IsBackground = true;
                threadRed.Start();
            }
        }

        /// <summary>
        /// 线程启动从共享内存中获取数据信息 
        /// </summary>
        private void ThreadReceive()
        {
            while (true)
            {
                try
                {
                    //读取共享内存中的数据：
                    //是否有数据写过来
                    m_Read.WaitOne();
                    byte[] byteBuffer = new byte[Length];
                    Copy2Byte(byteBuffer, addr);
                    m_Write.Release();

                    int len = byteBuffer[0] | byteBuffer[1] << 8 | byteBuffer[2] << 16 | byteBuffer[3] << 24;
                    byte[] data = new byte[len];
                    Array.Copy(byteBuffer, 4, data, 0, len);
                    string str = Encoding.ASCII.GetString(data);
                    DataReceived?.Invoke(data, str);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    continue;
                    //Thread.Sleep(0);
                }
            }
        }

        public byte[] ReceiveData(TimeSpan? span = null)
        {
            lock (this)
            {
                try
                {
                    if (m_Write == null)
                        m_Write = new Semaphore(1, 1, m_w_name);
                    if (m_Read == null)
                        m_Read = new Semaphore(0, 1, m_r_name);
                    if (handle == IntPtr.Zero)
                    {
                        IntPtr hFile = new IntPtr(Kernel32.INVALID_HANDLE_VALUE);
                        handle = Kernel32.CreateFileMapping(hFile, 0, Kernel32.PAGE_READWRITE, 0, Length, share_name);
                    }
                    if (addr == IntPtr.Zero)
                        addr = Kernel32.MapViewOfFile(handle, Kernel32.FILE_MAP_ALL_ACCESS, 0, 0, 0);

                    //读取共享内存中的数据 ：
                    bool read = false;
                    //是否有数据写过来
                    if (span != null)
                        read = m_Read.WaitOne(span.Value);
                    else
                        read = m_Read.WaitOne();

                    if (read)
                    {
                        byte[] byteBuffer = new byte[Length];
                        Copy2Byte(byteBuffer, addr);
                        m_Write.Release();

                        int len = byteBuffer[0] | byteBuffer[1] << 8 | byteBuffer[2] << 16 | byteBuffer[3] << 24;
                        byte[] data = new byte[len];
                        Array.Copy(byteBuffer, 4, data, 0, len);
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
                return null;
            }
        }

        public string ReceiveString(TimeSpan? span = null)
        {
            byte[] byteStr = ReceiveData(span);
            if (byteStr == null) return null;
            return Encoding.ASCII.GetString(byteStr);
        }

        public bool WriteBytes(byte[] bs)
        {
            if (bs == null || bs.Length == 0) return false;

            lock (this)
            {
                try
                {
                    m_Write = Semaphore.OpenExisting(m_w_name);
                    m_Read = Semaphore.OpenExisting(m_r_name);
                    handle = Kernel32.OpenFileMapping(Kernel32.FILE_MAP_WRITE, 0, share_name);
                    addr = Kernel32.MapViewOfFile(handle, Kernel32.FILE_MAP_ALL_ACCESS, 0, 0, 0);

                    m_Write.WaitOne();
                    int len = bs.Length;
                    byte[] sendBs = new byte[len + 4];
                    sendBs[0] = (byte)len;
                    sendBs[1] = (byte)(len >> 8);
                    sendBs[2] = (byte)(len >> 16);
                    sendBs[3] = (byte)(len >> 24);
                    Array.Copy(bs, 0, sendBs, 4, bs.Length);

                    //如果要是超长的话，应另外处理，最好是分配足够的内存
                    if (sendBs.Length <= Length)
                        Copy2Ptr(sendBs, addr);

                    m_Read.Release();

                    bool suc = m_Write.WaitOne(new TimeSpan(0, 0, 10));
                    if (suc)
                        m_Write.Release(); 
                    return suc;
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    //MessageBox.Show("不存在系统信号量!");
                }
                return false;
            }
        }

        public bool WriteString(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            byte[] strBytes = Encoding.ASCII.GetBytes(str);
            return WriteBytes(strBytes);
        }

        private void Copy2Byte(byte[] dst, IntPtr src)
        {
            for (var i = 0; i < dst.Length; i++)
            {
                byte b = Marshal.ReadByte(src, i);
                dst[i] = b;
            }
        }

        private void Copy2Ptr(byte[] byteSrc, IntPtr dst)
        {
            for (int i = 0; i < byteSrc.Length; i++)
            {
                Marshal.WriteByte(dst, i, byteSrc[i]);
            }
        }

        public void Dispose()
        {
            threadRed?.Abort();
            threadRed = null;
            m_Write?.Close();
            m_Write = null;
            m_Read?.Close();
            m_Read = null;
            Kernel32.CloseHandle(handle);
            handle = IntPtr.Zero;
            Kernel32.UnmapViewOfFile(addr);
            addr = IntPtr.Zero;
        }
    }
}
