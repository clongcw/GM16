using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    /// <summary>
    /// CanOpen通用预定义，PDO（传输数据对象），SDO（服务数据对象）
    /// </summary>
    public static class Common
    {
        public const UInt16 TX_PDO1 = 0x200;
        public const UInt16 RX_PDO1 = 0x180;
        public const UInt16 TX_PDO2 = 0x300;
        public const UInt16 RX_PDO2 = 0x280;
        public const UInt16 TX_SDO1 = 0x600;
        public const UInt16 RX_SDO1 = 0x580;

        public const UInt16 MIN_DEVICE_ID = 0x20; //自开发设备最小设备地址
        public const UInt16 MAX_DEVICE_ID = 0x2f; //自开发设备最大设备地址
        public const int SIMPLE_CMD_TIME_OUT = 10000; //简单命令执行超时时间
        public const int ELAPSED_TIME_OUT = 20000; //耗时命令执行超时时间
        public const int ELAPSED_RESET_TIME_OUT = 30000; //耗时命令执行超时时间

        public const int CODE_DEFAULT = 0;
        public const int CODE_X = 100000; //X轴代码
        public const int CODE_Y = 200000;//Y轴代码
        public const int CODE_Z = 300000;//Z轴代码
        public const int CODE_T = 400000;//T轴
        public const int CODE_E = 500000;//E
        public const int CODE_ADP = 600000;//ADP代码
        public const int CODE_Temp = 700000;//温控代码

        public const int CODE_CHANNEL = 1000000;
        public const int CODE_DOOR = 2000000;
        public const int CODE_LED = 3000000;
        public const int CODE_UV = 4000000;
        public const int CODE_FAN = 5000000;
        public const int CODE_ALARM = 6000000;
        public const int CODE_FAN2 = 7000000;
        public const int CODE_MODULE_CH = 10000000;
        public const int CODE_LH = 20000000;
        public const int CODE_HANDLE_Z = 30000000;
        public const int CODE_HANDLE_X = 40000000;
        public const int CODE_MODULE_PCR = 50000000;
        public const int FLAG_SCANNER = 60000000;
        public const int CODE_AUX = 70000000;
        public const byte CMD_CHIP = 0x17;


        public const byte CMD_CONNECT = 0x00;
        public const byte CMD_VERSION = 0x02;//查询版本

        public const byte SIGN_OC_ON = 0x01;//光耦挡住
        public const byte SIGN_OC_OFF = 0x00; //光耦未挡住

        //public const byte DefaultDetectSpeed = 32;
        //public const byte DeepDetectSpeed = 90;

        public const byte FRAME_START = 0x91; //起始帧
        public const byte FRAME_MID = 0x92; //中间帧
        public const byte FRAME_END = 0x93; //结束帧
        public const int GROUP_HS = 800;
        public const int GROUP_HS_X = 900;
        /// <summary>
        /// 通过功能码和设备ID生成帧ID（4位功能码+7位设备ID）
        /// </summary>
        /// <param name="functionCode">功能码</param>
        /// <param name="deviceId">设备ID</param>
        /// <returns>帧ID</returns>
        public static UInt32 GeneralFrameID(UInt16 functionCode, UInt16 deviceId)
        {
            return (UInt32)(functionCode + deviceId);
        }

        /// <summary>
        /// 从帧ID中解析出设备ID（帧ID的后7位为设备ID）
        /// </summary>
        /// <param name="frameId"></param>
        /// <returns>设备ID</returns>
        public static UInt16 ParseDeviceId(UInt32 frameId)
        {
            return (UInt16)(frameId & 0x7F);
        }

        /// <summary>
        /// 根据序号计算行列数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="countPerCol"></param>
        /// <param name="countPerRack"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static int GetPosById(int id, int countPerCol, int countPerRack, ref int row, ref int col)
        {
            int res = -1;
            if ((id >= 0) && (id < countPerRack))
            {
                col = id / countPerCol;
                row = id % countPerCol;
                res = (int)Math.Ceiling((id + 0.5) / countPerRack) - 1;
            }
            else
            {
                row = 0;
                col = 0;
            }
            return res;
        }

        public static int GetTipPos(int id, ref int row, ref int col)
        {
            return GetPosById(id, 8, 96, ref row, ref col);
        }

        private static string _sepator = "-";
        public static string EncodeLocaName(string name, int index)
        {
            return string.Format("{0}{1}{2}", name, _sepator, index);
        }

        public static string EncodeLocaName(string name, string pos)
        {
            return string.Format("{0}{1}{2}", name, _sepator, pos);
        }

        public static bool DecodeLocaName(string name, out string deName, out int index)
        {
            deName = string.Empty;
            index = 0;
            var names = name.Split(new[] { _sepator }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 2)
            {
                deName = names[0];
                return int.TryParse(names[1], out index);
            }
            return false;
        }

        public static int GenerateAdpGroupId(Channels ch)
        {
            return GenerateGroupId((int)ch);
        }

        public static int GenerateAdpGroupId(Channels ch, Tips tip)
        {
            var t = ((int)ch).ToString();
            var t2 = ((int)tip).ToString();
            var t3 = ((int)Module.Adp).ToString();
            return Convert.ToInt16(((int)ch).ToString() + ((int)tip).ToString() + ((int)Module.Adp).ToString());
        }

        public static int GeneratePcrGroupId(Channels ch)
        {
            return 20 + (int)ch;
        }

        public static int GenerateGroupId(int ch, bool isAdp = true)
        {
            if (isAdp)
            {
                return 10 + ch;
            }
            else
            {
                return ch;
            }
        }

        public static int GenerateScannerGroupId(int ch)
        {
            return 30 + ch;
        }

        public static int GenerateReagentScannerGroupId(int ch)
        {
            return 40 + ch;
        }

        public static int GenerateChipSealingGroupId(int ch)
        {
            return 50 + ch;
        }

        public static int GetChannelIndex(int groupId)
        {
            return groupId % 10;
        }

        /// <summary>
        /// 写ini文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 读ini文件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="refVal"></param>
        /// <param name="size"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder refVal, int size, string filePath);

        public delegate void DelegateRetryCommand(RetryModule module, int errCode, bool isIgnore);
    }


    #region 协议格式 FunctionCode+CommonDevice+CMD
    public static class FunctionCode
    {
        public const UInt16 SENDING_CODE = 0x300;
        public const UInt16 RECEIVE_CODE = 0x2C0;

        //public const UInt16 RX_SEALING_Z = 0x300;
        //public const UInt16 TX_SEALING_Z = 0x2C0;
    }
    public static class CommonDevice
    {
        public const UInt16 HEARTBEAT_PACKET = 0x00;//热封设置温度
        #region 风扇
        public const UInt16 FAN = 0x25;//上排风口
        public const UInt16 FAN2 = 0x3F;//风扇0x3f
        #endregion

        public const UInt16 LED = 0x21;//LED灯
        public const UInt16 UV_LAMP = 0x22;//紫外灯灯

        public const UInt16 DOOR = 0x3F;//门控制
        public const UInt16 ALARM = 0x3F;//三色灯警报       
        #region 热封
        public const UInt16 CHIP_SEALING_SET_TEMP = 0x4C;//热封设置温度
        public const UInt16 CHIP_SEALING_Y_MOVE = 0x4D;//热封y移动
        public const UInt16 CHIP_SEALING_Z_MOVE = 0x4C;//热封z移动
        #endregion


        #region 搬运
        public const UInt16 CHIP_HANDLING_SYSX = 0x2A;//芯片X搬运
        public const UInt16 CHIP_HANDLING_SYS = 0x28;//芯片Z搬运

        public const UInt16 LIQUID_HANDLING_SYS = 0x20;//液滴搬运
        #endregion
    }

    public static class CMD
    {
        public const byte CMD_HEARTBEAT_PACKET = 0x00;//心跳包
        #region 热封
        public const byte CMD_SET_TEMP = 0x82;//设置温度
        public const byte CMD_QUERY_TEMP = 0x83;//查询温度
        #endregion

        public const byte CALIBRATION_PARAM_A = 0x0A;//温度校准A参数
        public const byte CALIBRATION_PARAM_B = 0x0B;//温度校准B参数
        public const byte CALIBRATION_PARAM_C = 0x0C;//温度校准C参数
    }
    #endregion
}
