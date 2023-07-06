using GM16.Shared.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GM16.Shared.CommunicationLibrary
{
    public abstract class Port
    {
        private VCI_ERR_INFO pErrInfo;
        public const int MAX_COUNT = 50; //单次接收的最大帧数
        protected byte _canId = 0;
        protected bool IsOpened = false;
        public bool IsConnected = true;
        private static readonly Logger _log = Logger.Instance;
        public virtual void SetPortId(byte id)
        {
            _canId = id;
        }
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns></returns>
        public abstract int Open();

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        public abstract int Close();

        /// <summary>
        /// 设置AccCode、AccMode
        /// </summary>
        /// <param name="accCode"></param>
        /// <param name="accMask"></param>
        /// <returns></returns>
        public abstract int SetAccCodeMask(UInt32 accCode, UInt32 accMask);

        /// <summary>
        /// 设置波特率
        /// </summary>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        public abstract int SetBaudRate(BaudRate baudRate);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        public abstract int Init(BaudRate baudRate);

        /// <summary>
        /// 启动设备
        /// </summary>
        /// <returns></returns>
        public abstract int Start();

        /// <summary>
        /// 复位设备
        /// </summary>
        /// <returns></returns>
        public abstract int Reset();
        //public abstract int ReadBoardInfo();
        //public abstract int ReadErrInfo();
        //public abstract int ReadStatus(byte canId);

        /// <summary>
        /// 获取接收缓冲区的消息个数
        /// </summary>
        /// <returns></returns>
        public abstract int GetReceiveNum();

        /// <summary>
        /// 多帧发送
        /// </summary>
        /// <param name="count">一次发送的帧数</param>
        /// <param name="datas"></param>
        /// <returns></returns>
        public abstract int Send(UInt32 count, CanData[] datas);

        /// <summary>
        /// 单帧发送
        /// </summary>
        /// <param name="id"></param>
        /// <param name="len"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        public abstract int Send(UInt32 id, byte len, byte[] datas);

        /// <summary>
        /// 多帧接收
        /// </summary>
        /// <param name="count">帧数</param>
        /// <param name="datas">实际接收数据</param>
        /// <returns>实际接收帧数</returns>
        public abstract int Receive(UInt16 count, ref CanData[] datas);

        /// <summary>
        /// 单帧接收
        /// </summary>
        /// <param name="data">实际接收数据</param>
        /// <returns>实际接收帧数</returns>
        public abstract int Receive(ref CanData data);

        /// <summary>
        /// 读取错误信息
        /// </summary>
        /// <param name="pErrInfo"></param>
        /// <returns></returns>
        public abstract int ReadErrInfo(ref VCI_ERR_INFO pErrInfo);

        /// <summary>
        /// 设备是否运行
        /// </summary>
        /// <returns></returns>
        public abstract bool IsRunning();

        /// <summary>
        /// clear buffer 
        /// </summary>
        /// <returns></returns>
        public abstract int Clear();

        /// <summary>
        /// 开始接收端口数据
        /// </summary>
        public void StartReceiveData()
        {
            Thread threadReceiveData = new Thread(ReceiveData);
            threadReceiveData.IsBackground = true;
            //threadReceiveData.Priority = ThreadPriority.AboveNormal;
            threadReceiveData.Name = "Recevie Thread";
            threadReceiveData.Start();
        }

        private void ReceiveData()
        {
            while (IsOpened)
            {
                if (IsRunning())
                {
                    //int res = _port.Receive(1, ref objRec);
                    //CanData objRec = new CanData();
                    //objRec.Init();
                    //int res = GetReceiveNum();
                    //if (res>0)
                    //{
                    //    res = Receive(ref objRec);
                    //    if (res > 0)
                    //    {
                    //        if (objRec.FrameFlag== FrameFlag.Stanard)
                    //        {
                    //            CanDataReceivedEventArgs e = new CanDataReceivedEventArgs(objRec);
                    //            OnDataReceived(e);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        res = ReadErrInfo(ref pErrInfo);
                    //        Thread.Sleep(10);
                    //    }
                    //}
                    //else
                    //{
                    //    Thread.Sleep(10);
                    //}

                    //改为多帧读取 
                    int res = 0;
                    int count = GetReceiveNum();
                    if (count > 0)
                    {
                        CanData[] recDatas = new CanData[count];
                        for (int i = 0; i < count; i++)
                        {
                            recDatas[i].Init();
                        }
                        res = Receive((UInt16)count, ref recDatas);
                        if (res > 0)
                        {
                            for (int i = 0; i < res; i++)
                            {
                                if (recDatas[i].FrameFlag == FrameFlag.Stanard)
                                {
                                    CanDataReceivedEventArgs e = new CanDataReceivedEventArgs(recDatas[i]);
                                    OnDataReceived(e);
                                }
                            }
                        }
                        else
                        {
                            res = ReadErrInfo(ref pErrInfo);
                            Clear();
                            Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }

        }

        /// <summary>
        /// 接收数据事件
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataReceived(CanDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, e);
            }
        }

        public delegate void DataReceivedEventHandler(object sender, CanDataReceivedEventArgs e);
        public event DataReceivedEventHandler DataReceived = null; //接收数据


    }
}
