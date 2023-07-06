using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GM16.Shared.DeviceLibrary
{
    public struct FrameId
    {
        public byte Dir;
        public byte Group;
        public byte DevId;
        public byte FrameType;
    }

    public static class AdpConst
    {
        public const byte MOD_ADTHREAD = 0x00; //AD阈值
        public const byte MOD_DURATION = 0x01;
        public const byte MOD_ABS_POS = 0x02;
        public const byte MOD_PLLD = 0x03; //设定以增量标识的活塞速度
        public const byte MOD_MAX_R_STEP = 0x04;
        public const byte MOD_PERFORM = 0x05;
        public const byte MOD_INTERVAL = 0x06;
        public const byte MOD_SMP_NO = 0x07;
        public const byte MOD_MAX_DURATION = 0x08;

        public const int TIMEOUT_CMD = 10000; //执行简单命令超时时间

        public const UInt32 ID_GUIDE = 0x80;//引导请求ID
        public const UInt32 ID_ACTION = 0x101;//动作帧ID（帧类型1）
        public const UInt32 ID_REPORT = 0x106; //报告帧ID（帧类型6）


        public const byte DIR_UP = 0x01; //消息从辅机到主机
        public const byte DIR_DOWN = 0x00; //消息从主机到辅机
        public const byte MODE_INC = 0x00;//增量模式
        public const byte MODE_UL = 0x01; //微升模式

        public const byte GROUP_GUIDE = 0x01; //预留为引导请求程序组
        public const byte GROUP_ADP = 0x02; //ADP组

        public const byte FRAME_GUIDE_ACK = 0x00; //答复引导帧
        public const byte FRAME_ACTION = 0x01; //动作帧或多帧结束帧 类型1
        public const byte FRAME_REPORT = 0x06; //报告帧 类型6
        public const byte FRAME_MUTIL_START = 0x03; //多帧应答启动帧类型3
        public const byte FRAME_MUTIL_MID = 0x04; //多帧应答中间帧类型4
        public const byte FRAME_MUTIL_END = 0x06; //多帧应答结束帧类型6

        public static byte[] ACK_GUIDE = { 0x20, 0x20 };//主机应答消息
        public static byte[] ACK_STATUS = { 0x20, 0x60 }; //设备返回的答复帧（包含的两个状态字节）

        public const byte CMD_EXECUTE = 0x52; //执行命令‘R’

        //以下命令使用帧类型1发送
        public const byte CMD_INIT = 0x57; //初始化命令‘W’
        public const byte CMD_EJECT = 0x45; //顶出枪头 ‘E’
        public const byte CMD_ABS = 0x41; //将活塞移动到以增量或微升指定的绝对位置 ‘A’
        public const byte CMD_PULL = 0x50; //将活塞上移（抽吸）输入参数中指定的增量或微升数 ‘P’
        public const byte CMD_DETACH = 0x44; //将活塞上移（抽吸）输入参数中指定的增量或微升数 ‘D’
        public const byte CMD_MAX_SPEED = 0x56; //设置最高速度‘V’
        public const byte CMD_START_SPEED = 0x76; //设置启动速度‘v’
        public const byte CMD_CUTOFF_SPEED = 0x63; //设置断流速度‘c’
        public const byte CMD_GRADE = 0x4C; //设置坡度‘L’
        public const byte CMD_BACKLASH = 0x4B; //设置后冲增量‘K’
        public const byte CMD_TERMINATE = 0x54; //终止指令 ‘T’
        public const byte CMD_RESET = 0x21; //复位指令 ‘！’
        public const byte CMD_BEGIN_DETECTION = 0x42; //开始液位检测 ‘B’
        public const byte CMD_PLLD_PARAM = 0x70; //设置LLD和压力流参数
        public const byte CMD_DIAGNOSE = 0x64; //执行设备诊断
        public const byte CMD_DEVICE_CONFIG = 0x55;//将设备信息写入NVRAM 'U'
        public const byte CMD_SET_HEARTBEAT_PERIOD = 0x62;//设置心跳周期'b'

        //以下命令使用帧类型6发送,不要求执行指令
        public const byte CMD_QUERY = 0x51; //查询设备状态 ‘Q’
        public const byte CMD_REPORT = 0x3F; //报告设备状态、参数、配置和诊断数据 ‘？’
        public const byte CMD_REPORT_PRESURE = 0x23; //报告压力数据 ‘#’


        public const byte S_REPORT_TIP = 31;//报告枪头状态
        public const byte S_REPORT_PRESSURE = 56;//以ADC计数报告当前压力读数

        public const byte TIP_EXIST = 1; //Tip头存在
        public const byte TIP_NONE_EXIST = 0; //Tip头不存在

        public const byte FLAG_HEARTBEAT = 0x40;

        /// <summary>
        /// 生成ID共11位
        /// </summary>
        /// <param name="dir">方向（第11位）</param>
        /// <param name="group">组（第8-10）</param>
        /// <param name="devId">设备地址（第4-7位）</param>
        /// <param name="frameType">帧类型（第1-3位）</param>
        /// <returns></returns>
        public static UInt32 GenerateFramID(byte dir, byte group, byte devId, byte frameType)
        {

            return (UInt32)(frameType + (devId << 3) + (group << 7) + (dir << 10));
        }

        /// <summary>
        /// 生成帧ID
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns></returns>
        public static UInt32 GenerateFramID(FrameId frameId)
        {
            return (UInt32)(frameId.FrameType + (frameId.DevId << 3) + (frameId.Group << 7) + (frameId.Dir << 10));

        }

        /// <summary>
        /// 解析帧ID
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="dir">方向（第11位）</param>
        /// <param name="group">组（第8-10）</param>
        /// <param name="devId">设备地址（第4-7位）</param>
        /// <param name="frameType">帧类型（第1-3位）</param>
        public static void AnalysisFramID(UInt32 frameId, ref byte dir, ref byte group, ref byte devId, ref byte frameType)
        {
            dir = (byte)(frameId >> 10);
            group = (byte)((frameId - (dir << 10)) >> 7);
            devId = (byte)((frameId - (dir << 10) - (group << 7)) >> 3);
            frameType = (byte)(frameId - (dir << 10) - (group << 7) - (devId << 3));
        }

        /// <summary>
        /// 解析帧ID
        /// </summary>
        /// <param name="frameId"></param>
        /// <param name="anyFrameId"></param>
        public static void AnalysisFramID(UInt32 frameId, ref FrameId anyFrameId)
        {
            anyFrameId.Dir = (byte)(frameId >> 10);
            anyFrameId.Group = (byte)((frameId - (anyFrameId.Dir << 10)) >> 7);
            anyFrameId.DevId = (byte)((frameId - (anyFrameId.Dir << 10) - (anyFrameId.Group << 7)) >> 3);
            anyFrameId.FrameType = (byte)(frameId - (anyFrameId.Dir << 10) - (anyFrameId.Group << 7) - (anyFrameId.DevId << 3));
        }
    }
}
