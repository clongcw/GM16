using GM16.Shared.CommonLibrary;
using GM16.Shared.EntityModel;
using GM16.Shared.EntityModel.DBContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GM16.Shared.DeviceLibrary
{
    public abstract class Motor
    {

        protected int _currentSpeed;
        protected int _currentCurrent;

        /// <summary>
        /// 电机信息
        /// </summary>
        public MotorInfo MotorInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 电机是否正在运动
        /// </summary>
        [XmlIgnore]
        public bool IsRunning
        {
            get;
            protected set;
        }

        /// <summary>
        /// 当前位置，是指针对坐标原点的位置
        /// </summary>
        [XmlIgnore]
        public int CurrentPostion
        {
            get;
            set;
        }

        public Motor()
        {

        }

        public Motor(string name)
        {
            MotorInfo = new MotorInfo();
            MotorInfo.Name = name;
            Load();
        }
        public void Load()
        {
            var context = new DatabaseContext();
            MotorInfo info = context.MotorInfos.GetSingle(m => m.Name == MotorInfo.Name);
            if (info != null)
            {
                MotorInfo = info;
            }
        }

        public void Load(string fileName)
        {
            using (FileStream fsRead = new FileStream(fileName, FileMode.Open))
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(Motor));
                Motor motor = (Motor)xmlSer.Deserialize(fsRead);
                this.MotorInfo.MotorId = motor.MotorInfo.MotorId;
                this.MotorInfo.Name = motor.MotorInfo.Name;
                this.MotorInfo.DeviceId = motor.MotorInfo.DeviceId;
                this.MotorInfo.MaxStep = motor.MotorInfo.MaxStep;
            }
        }

        public void Save(string fileName)
        {
            using (Stream writer = new FileStream(fileName, FileMode.Create))
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(Motor));
                xmlSer.Serialize(writer, this);
            }
        }


        /// <summary>
        /// 电机使能
        /// </summary>
        public abstract int MotorEnabled();

        /// <summary>
        /// 关闭电机使能
        /// </summary>
        public abstract int MotorDisabled();

        /// <summary>
        /// 电机相对移动至指定位置
        /// </summary>
        /// <param name="dir">方向</param>
        /// <param name="step">运行步数</param>
        public abstract int RelativeMove(byte dir, int step);

        /// <summary>
        /// 电机移动至绝对位置
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <returns></returns>
        public abstract int AbsMove(int position);

        /// <summary>
        /// 设置电机电流
        /// </summary>
        /// <param name="current"></param>
        public abstract int SetCurrent(int current);

        /// <summary>
        /// 设置电机电流
        /// </summary>
        /// <param name="speed"></param>
        public abstract int SetSpeed(int speed);

        /// <summary>
        /// 电机复位
        /// </summary>
        public abstract int Reset();

        /// <summary>
        /// 电机复位
        /// </summary>
        public abstract int Reset(int timeout);

        /// <summary>
        /// 查询电机当前位置
        /// </summary>
        public abstract int QueryPosition();

        /// <summary>
        /// 开始液面探测
        /// </summary>
        /// <param name="step">电机下降步数</param>
        /// <returns>错误码（0表示成功）</returns>
        public abstract int LiquidDetection(byte adpAddress, ref int step);

        /// <summary>
        /// 开始液面探测
        /// </summary>
        /// <param name="adpAddress"></param>
        /// <param name="speed">Z轴电机速度</param>
        /// <param name="step"></param>
        /// <returns></returns>
        public abstract int LiquidDetection(byte adpAddress, byte speed, ref int step);

        /// <summary>
        /// 设置探液面Z轴下降最大深度
        /// </summary>
        /// <param name="depth">最大深度对应步数</param>
        /// <returns></returns>
        public abstract int SetDetectDepth(int depth);

        /// <summary>
        /// 查询光耦状态（true-挡住，false-未挡住）
        /// </summary>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public abstract int CheckOptocouplerState(ref bool isOn);

        /// <summary>
        /// 电机相对移动至指定位置(不需要复位)
        /// </summary>
        /// <param name="dir">方向</param>
        /// <param name="step">运行步数</param>
        public abstract int RelativeMoveWithUnreset(byte dir, int step);

        /// <summary>
        /// 设置慢速标记
        /// </summary>
        /// <returns></returns>
        public abstract int SetSlowSpeedFlag(byte flag = 1);

        public abstract int StartFeedBackAbsMove(int step);

        public abstract int WaitForMoveEnd();

        public abstract int GetCurrentPos(ref int step);

        public abstract int StartFeedBackReset();

        public abstract void SetResetDir(byte dir);

        public abstract int GetVersion(ref string version);

        public abstract int SetError(int errValue);

        /// <summary>
        /// 电机初始化
        /// </summary>
        /// <returns></returns>
        public int Initialize(int timeout = MotorConstant.RESET_TIME_OUT)
        {
            //int res = MotorEnabled();
            //res = SetCurrent(MotorInfo.Current);
            //res = SetSpeed(MotorInfo.ResetSpeed);
            var res = Reset(timeout);
            if (res == ResultCode.S_SUCCESSED)
            {
                res = SetSpeed(MotorInfo.NormalSpeed);
            }

            return res;
        }

    }
}
