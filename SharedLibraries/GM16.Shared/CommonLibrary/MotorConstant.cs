using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    public static class MotorConstant
    {
        public const int ELAPSED_TIME_OUT = 30000;//电机运动超时时间
        public const int SIMPLE_CMD_TIME_OUT = 5000; //简单命令执行超时时间
        public const int RESET_TIME_OUT = 10000;//电机复位超时时间

        //电机控制命令
        public const byte CMD_ENABLED = 0x0F; //电机使能
        public const byte CMD_DISABLED = 0x08; //关闭电机使能
        public const byte CMD_RELATIVE_MOVE = 0xB6; //电机相对运动
        public const byte CMD_ABS_MOVE = 0xB7; //电机相对运动
        public const byte CMD_SET_STEP_ERROR = 0xAC; //电机相对运动
        public const byte CMD_SET_SPEED = 0x42;//设置速度
        public const byte CMD_SET_CURRENT = 0x43; //设置电流
        public const byte CMD_RESET = 0x52; //电机复位
        public const byte CMD_DETECT = 0x53; //液位探测
        public const byte CMD_SET_DETECT_DEPTH = 0x55;//设置探液面最大深度
        public const byte CMD_QUERY_POS = 0x57; //询问电机当前位置
        public const byte CMD_CHECK_OC_STATE = 0x56;//查询光耦状态
        public const byte CMD_SET_LOAD_TIP_FLAG = 0x14;//设置取Tip头标记
        public const byte CMD_FB_RELATIVE_MOVE = 0xbb;//开始有接收命令反馈的相对运动
        public const byte SIGN_FB_MOVE = 0xbc;//接收到0xbb后反馈
        public const byte CMD_GET_CURRENT_POS = 0xba;//获取当前位置步数
        public const byte CMD_FB_RESET = 0x5b; //电机有反馈复位
        public const byte SIGN_FB_RESET = 0x5c; //接收到0x5b后反馈
        public const byte SIGN_ON = 0x01; //IO控制开标识
        public const byte SIGN_OFF = 0x00; //IO控制关标识

        public const byte S_OK = 0x00; //命令执行后状态——正常
        public const byte S_ERR = 0x01;//命令执行后状态——出错

        public const byte DIR_N = 0x00; //反向运动
        public const byte DIR_P = 0x01; //正向运动


        public const byte MAX_CURRENT = 0x1F;//电机最大电流
        public const byte MIN_CURRENT = 0x00;//电机最小电流

        public const byte CMD_SET_LOW_SPEED_FLAG = 0x05;

        public const byte CMD_SET_ERROR = 0xAC;

        public const int X_MOTOR_N_SPEED = 3000; //X轴正常运行速度
        public const int X_MOTOR_R_SPEED = 1000; //X轴复位运行速度
        public const int Y_MOTOR_N_SPEED = 3; //Y轴正常运行速度
        public const int Y_MOTOR_SHORT_SPEED = 12;//Y轴短距离运行速度
        public const int Z_MOTOR_N_SPEED = 3; //Z轴正常运行速度
        public const int Z_MOTOR_T_SPEED = 16; //Z轴取Tip头运行速度
        public const int MOTOR_DEFAULT_CURRENT = 31; //电机默认电流
        public const int ASPIRATE_DEPTH = 1000;//吸液下降深度
        public const int RUN_HEIGHT = 2000;//XY轴运行的Z方向安全位置
    }
}
