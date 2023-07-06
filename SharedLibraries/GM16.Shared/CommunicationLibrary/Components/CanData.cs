using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommunicationLibrary
{
    /// <summary>
    /// 波特率
    /// </summary>
    public enum BaudRate
    {
        BAUD_10K,
        BAUD_20K,
        BAUD_50K,
        BAUD_100K,
        BAUD_125K,
        BAUD_250K,
        BAUD_500K,
        BAUD_800K,
        BAUD_1000K
    }

    /// <summary>
    /// 帧类型
    /// </summary>
    public enum FrameFlag
    {
        Stanard, //标准帧
        Extern   //扩展帧
    }

    /// <summary>
    /// 帧格式
    /// </summary>
    public enum FrameMode
    {
        Data,  //数据帧
        Remote //远程帧
    }




    /// <summary>
    /// 
    /// </summary>
    public struct CanData
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public UInt32 TimeStamp;

        /// <summary>
        /// 帧类型
        /// </summary>
        public FrameFlag FrameFlag;

        /// <summary>
        /// 帧格式
        /// </summary>
        public FrameMode FrameMode;

        /// <summary>
        /// 帧ID
        /// </summary>
        public UInt32 ID;

        /// <summary>
        /// 帧数据长度
        /// </summary>
        public byte Length;

        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data;

        public void Init()
        {
            Data = new byte[8];
        }

        public CanData Clone()
        {
            CanData cloneData = new CanData();
            cloneData.Init();
            cloneData.TimeStamp = TimeStamp;
            cloneData.FrameFlag = FrameFlag;
            cloneData.FrameMode = FrameMode;
            cloneData.ID = ID;
            cloneData.Length = Length;
            Data.CopyTo(cloneData.Data, 0);
            return cloneData;
        }
    }
}
