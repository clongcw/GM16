using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    public static class ResultCode
    {
        //public const int ERR_OK = 0x00; //执行成功
        //public const int ERR_TIMEOUT = 0x01; //执行超时
        //public const int ERR_FAILURE = 0x02;//执行失败

        public const int S_OK = 0x01; //通讯接口成功代码
        public const int S_SUCCESSED = 0x00; //自定义接口成功代码
        public const int S_STOP = 0x5F;//执行了停止操作
        public const int ERR_DISCONNECTED = 0x63;//断开连接
        public const int ERR_DETECT_FAILURE = 0x4F;//探测液面失败
        public const int ERR_MOTOR_RESET = 0x1E;//电机复位出错
        public const int ERR_TIMEOUT = 0x1F; //超时
        public const int ERR_BUSY = 0x10;//忙
        public const int ERR_UNRESET = 0x11;//没有复位
        public const int ERR_TIP_EXIST = 0x12;//已经有Tip头
        public const int ERR_TIP_NONE_EXIST = 0x13; //Tip不存在
        public const int ERR_TIP_LOST = 0x14; //Tip头丢失
        public const int ERR_MOTOR_UNREACH = 0x15;//电机未到位
        public const int ERR_UNREACH_LIMIT = 0x16;//Maxon电机复位未到达限位位置
        //public const int ERR_MAX_DISENABLED = 0x17;//Maxon电机失控
        public const int ERR_PLATE_NONE_EXIST = 0x18;//板不存在??
        public const int ERR_PLATE_CARRY_FAILURE = 0x19; //搬板失败
        public const int ERR_LIQUID_NOTENOUGH = 0x1A;//液体不足
        public const int ERR_LIQUID_BLOCKING = 0x1B;//Adp堵塞
        public const int ERR_BARREL_WARING = 0x1C;//桶报警未处理错误??
        public const int ERR_OVER_RANG = 0x1D;//电机移动超出范围
        public const int ERR_CHECK_BARCODE = 0x30;//条码验证错误
        public const int ERR_TOTAL_LIQUID_NOTENOUGH = 0x31;//液体总量不足
        //public const int ERR_LUM_TAKEOUT_FAILURE = 0x32; //荧光仪出仓失败
        //public const int ERR_LUM_LOCK_TRAY_FAILURE = 0x33; //荧光仪锁住托盘失败
        //public const int ERR_LUM_PUTIN_FAILURE = 0x34; //荧光仪入仓失败
        //public const int ERR_LUM_UNINIT = 0x35; //荧光仪未初始化，不能操作
        //public const int ERR_LUM_OPEN_DOOR_FAILURE = 0x36; //荧光仪开门失败
        //public const int ERR_LUM_CLOSE_DOOR_FAILURE = 0x37; //荧光仪关门失败
        //public const int ERR_LUM__FAILURE_DISABLED = 0x38; //荧光仪故障失效
        //public const int ERR_WASHER_FALURE = 0x40;//洗板机故障，不能洗板，转入另一洗板机重新洗板
        public const int ERR_SUSPICIOUS_LIQUIDLEVEL = 0x41;//液面可疑
        public const int ERR_NONE_ASPIRATE = 0x42;//疑似空吸
        public const int ERR_TEMP_EXP = 0x50;//振荡器温控异常
        public const int ERR_EXIST_CHIP = 0x51;//搬运位置存在芯片
        public const int ERR_LOAD_CHIP = 0x52;//吸取芯片失败
        public const int ERR_UNLOAD_CHIP = 0x53;//放置芯片失败
        public const int ERR_DOOR_UNCLOSED = 0x61;//门未关闭


        public const int FLAG_DEVICE = 10000;//设备代码
        public const int FLAG_LIQHA = 20000;//Adp移液系统
        public const int FLAG_MULTIPLE_PIP = 30000;//排枪
        public const int FLAG_EXTRACT_CAHNNEL = 40000;//提取通道
        public const int FLAG_TIP_CAHNNEL = 50000;//Tip头通道
        public const int FLAG_PCR_CHANNEL = 60000;//PCR通道
        public const int FLAG_SHAKER = 70000;//振荡器代码
                                             //public const int FLAG_CHIP_HANDLING_X = 80000;//振荡器代码
                                             //public const int FLAG_CHIP_HANDLING_Z = 90000;//振荡器代码


        #region 温度块超时
        public const int ERR_TEMP_TIMEOUT = 0x71;//升温超时超时
        #endregion


        public const int ERR_ADP_NO_ERROR = 0x00;//无错误
        public const int ERR_ADP_INITIALIZATION_ERROR = 0x01;//初始化错误
        public const int ERR_ADP_INVALID_COMMAND = 0x02;//无效指令
        public const int ERR_ADP_INVALID_OPERAND = 0x03;//无效操作数
        public const int ERR_ADP_PRESSURE_SENSOR_MODULE_NOT_CONFIGURED_OR_NOT_WORKING = 0x04;//压力传感器模块未配置或不工作
        public const int ERR_ADP_OVER_PRESSURE = 0x05;//过度压力
        public const int ERR_ADP_LIQUID_LEVEL_DETECT_FAILURE = 0x06;//液位检测故障
        public const int ERR_ADP_DEVICE_NOT_INITIALIZED = 0x07;//设备未被初始化
        public const int ERR_ADP_TIP_EJECT_FAILURE = 0x08;//枪头顶出故障
        public const int ERR_ADP_PLUNGER_OVERLOAD = 0x09;//活塞过载
        public const int ERR_ADP_TIP_LOST_OR_NOT_PRESENT = 0x0A;//枪头丢失或不存在
        public const int ERR_ADP_NOT_USED = 0x0B;//未使用
        public const int ERR_ADP_EXTENDED_ERROR = 0x0C;//扩展错误
        public const int ERR_ADP_NVMEM_ACCESS_FAILURE = 0x0D;//NVMEM访问故障??
        public const int ERR_ADP_COMMAND_BUFFER_EMPTY_OR_EXECUTED_OR_NOT_READY_FOR_REPEAT = 0x0E;//指令缓冲区是空的或者被执行（不能使用R）或未做好重复的准备（不能使用X）
        public const int ERR_ADP_COMMAND_BUFFER_OVERFLOW = 0x0F;//指令缓冲区溢出



    }
}
