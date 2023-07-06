using GM16.Shared.CommonLibrary;
using GM16.Shared.CommunicationLibrary;
using GM16.Shared.EntityModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logger = GM16.Shared.Services.Logger;

namespace GM16.Shared.DeviceLibrary
{
    /// <summary>
    /// 电机基本操作类
    /// </summary>
    public class CommonMotor : Motor
    {
        private Port _port; //通信接口
        private UInt32 _sendFrameId; //发送帧ID（电路板ID）
        private Logger _log = Logger.Instance;
        private AutoResetEvent _ackEvent;
        private AutoResetEvent _ackEventMoveEnd;
        private int _detectStep = 0;//探测液面，要移动的步数
        private int _errCode = 0;
        private bool _bReset = false;
        private bool _bWait = false;
        private bool _bWaitEnd = false;
        private byte _ocstate = 0;
        private AutoResetEvent _ackQueryVersion;
        private bool _bWaitQueryVersion = false;
        private string _strVersion = string.Empty;

        public CommonMotor(Port port, UInt16 deviceId, byte motorId, string name, int maxStep) : base(name)
        {
            MotorInfo = new MotorInfo
            {
                NormalSpeed = 3,
                ResetSpeed = 16,
                Current = 31
            };
            _port = port;
            MotorInfo.DeviceId = (short)deviceId;
            MotorInfo.MotorId = motorId;
            _ackEvent = new AutoResetEvent(false);
            _ackEventMoveEnd = new AutoResetEvent(false);

            _sendFrameId = Common.GeneralFrameID(Common.TX_PDO2, (ushort)MotorInfo.DeviceId);


            _port.DataReceived += _port_DataReceived; //注册接收事件
            MotorInfo.Name = name;
            MotorInfo.MaxStep = maxStep;
            _currentCurrent = 0;
            _currentSpeed = 0;
            _ackQueryVersion = new AutoResetEvent(false);
            //Load();
            //Save();

        }

        void _port_DataReceived(object sender, CanDataReceivedEventArgs e)
        {
            ushort deviceId = Common.ParseDeviceId(e.CanData.ID);
            if (deviceId == MotorInfo.DeviceId)
            {
                CanData recObj = e.CanData.Clone();
                if (recObj.Length > 1)
                {
                    byte cmd = recObj.Data[0];
                    _log.Debug("cmd:{0},{1},{2}", cmd, recObj.Data[1], MotorInfo.Name);


                    #region 接收debug
                    string dataReceived = string.Empty;

                    for (int i = 1; i < recObj.Data.Length; i++)
                    {
                        dataReceived += recObj.Data[i].ToString("X");
                    }
                    _log.Debug($"ID:{recObj.ID.ToString("X")},Data:{cmd.ToString("X")},{dataReceived}");
                    #endregion

                    if (cmd != MotorConstant.CMD_DETECT && cmd != Common.CMD_VERSION)
                    {
                        int recMotorId = recObj.Data[1];
                        if (recMotorId == MotorInfo.MotorId)
                        {
                            switch (recObj.Data[0])
                            {
                                case MotorConstant.CMD_RELATIVE_MOVE:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        IsRunning = false;
                                        int step = (int)recObj.Data[4] + (int)(recObj.Data[5] << 8) + (int)(recObj.Data[6] << 16) + (int)(recObj.Data[7] << 24);
                                        _log.Information("电机{0}相对运行{1}步", MotorInfo.Name, step);
                                        //_errCode = recObj.Data[2];
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_ABS_MOVE:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        IsRunning = false;
                                        int step = (int)recObj.Data[3] + (int)(recObj.Data[4] << 8) + (int)(recObj.Data[5] << 16) + (int)(recObj.Data[6] << 24);
                                        _log.Information("电机{0}绝对运行{1}步", MotorInfo.Name, step);
                                        //_errCode = recObj.Data[2];
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.SIGN_FB_MOVE:
                                    _log.Debug("电机{0}接收到有反馈运行命令{1},{2}", MotorInfo.Name, recObj.Data[0], _bWait);
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        _bWaitEnd = true;
                                        //IsRunning = false;
                                        //int step = (int)recObj.Data[4] + (int)(recObj.Data[5] << 8) + (int)(recObj.Data[6] << 16) + (int)(recObj.Data[7] << 24);
                                        // _log.Information("电机{0}相对运行{1}步", MotorInfo.Name, step);
                                        _errCode = ResultCode.S_SUCCESSED;
                                        _log.Debug("电机{0}运行接收到反馈", MotorInfo.Name);
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_FB_RELATIVE_MOVE:
                                    if (_bWaitEnd)
                                    {
                                        _bWaitEnd = false;
                                        IsRunning = false;
                                        int step = (int)recObj.Data[4] + (int)(recObj.Data[5] << 8) + (int)(recObj.Data[6] << 16) + (int)(recObj.Data[7] << 24);
                                        _log.Debug("电机{0}有反馈相对运行{1}步结束", MotorInfo.Name, step);
                                        _errCode = recObj.Data[2];
                                        _ackEventMoveEnd.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_QUERY_POS:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        CurrentPostion = (int)recObj.Data[2] + (int)(recObj.Data[3] << 8) + (int)(recObj.Data[4] << 16) + (int)(recObj.Data[5] << 24);
                                        _errCode = ResultCode.S_SUCCESSED;
                                        _ackEvent.Set();
                                    }
                                    break;
                                //case MotorConstant.CMD_DETECT:
                                //    _detectStep = (int)recObj.Data[2] + (int)(recObj.Data[3] << 8) + (int)(recObj.Data[4] << 16) + (int)(recObj.Data[5] << 24);
                                //    _errCode = recObj.Data[1];
                                //    _ackEvent.Set();
                                //    break;
                                case MotorConstant.CMD_RESET:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        CurrentPostion = 0;
                                        _bReset = true;
                                        _errCode = recObj.Data[2];
                                        if (_errCode != ResultCode.S_SUCCESSED)
                                        {
                                            _errCode = ResultCode.ERR_MOTOR_RESET;
                                        }
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.SIGN_FB_RESET:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        _bWaitEnd = true;
                                        //IsRunning = false;
                                        //int step = (int)recObj.Data[4] + (int)(recObj.Data[5] << 8) + (int)(recObj.Data[6] << 16) + (int)(recObj.Data[7] << 24);
                                        // _log.Information("电机{0}相对运行{1}步", MotorInfo.Name, step);
                                        _errCode = ResultCode.S_SUCCESSED;
                                        _log.Debug("电机{0}复位接收到反馈", MotorInfo.Name);
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_FB_RESET:
                                    if (_bWaitEnd)
                                    {
                                        _bWaitEnd = false;
                                        IsRunning = false;
                                        CurrentPostion = 0;
                                        _bReset = true;
                                        _errCode = recObj.Data[2];
                                        if (_errCode != ResultCode.S_SUCCESSED)
                                        {
                                            _errCode = ResultCode.ERR_MOTOR_RESET;
                                        }
                                        _ackEventMoveEnd.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_GET_CURRENT_POS:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        _errCode = recObj.Data[2];
                                        if (_errCode == ResultCode.S_SUCCESSED)
                                        {
                                            CurrentPostion = 0;
                                            _currentAbsStep = (int)recObj.Data[3] + (int)(recObj.Data[4] << 8) + (int)(recObj.Data[5] << 16) + (int)(recObj.Data[6] << 24);
                                        }
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_CHECK_OC_STATE:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        _errCode = recObj.Data[2];
                                        _ocstate = recObj.Data[3];
                                        _ackEvent.Set();
                                    }
                                    break;
                                case MotorConstant.CMD_ENABLED:
                                case MotorConstant.CMD_DISABLED:
                                case MotorConstant.CMD_SET_CURRENT:
                                case MotorConstant.CMD_SET_SPEED:
                                case MotorConstant.CMD_SET_DETECT_DEPTH:
                                case MotorConstant.CMD_SET_LOAD_TIP_FLAG:
                                case MotorConstant.CMD_SET_LOW_SPEED_FLAG:
                                    if (_bWait)
                                    {
                                        _bWait = false;
                                        _errCode = ResultCode.S_SUCCESSED;
                                        _ackEvent.Set();
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (cmd)
                        {
                            case MotorConstant.CMD_DETECT:
                                if (_bWait)
                                {
                                    _bWait = false;
                                    _detectStep = (int)recObj.Data[2] + (int)(recObj.Data[3] << 8) + (int)(recObj.Data[4] << 16) + (int)(recObj.Data[5] << 24);
                                    _errCode = recObj.Data[1];
                                    if (_errCode > 0x20 && _errCode <= 0x2f)
                                    {
                                        _errCode -= 0x20;
                                    }
                                    _ackEvent.Set();
                                }
                                break;
                            case Common.CMD_VERSION:
                                if (_bWaitQueryVersion)
                                {
                                    _bWaitQueryVersion = false;
                                    _strVersion = string.Format("{0}.{1}.{2}", ASCIIEncoding.ASCII.GetString(recObj.Data, 3, 1), ASCIIEncoding.ASCII.GetString(recObj.Data, 4, 2), ASCIIEncoding.ASCII.GetString(recObj.Data, 6, 2));
                                    _ackQueryVersion.Set();
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发送单命令
        /// </summary>
        /// <param name="cmd">指令ID</param>
        /// <param name="parameters">参数</param>
        /// <param name="timeOut">超时时间</param>
        /// <returns></returns>
        private int SendCommand(byte cmd, byte[] parameters, int timeOut)
        {
            _errCode = ResultCode.S_SUCCESSED;
            List<byte> bytSendList = new List<byte>();
            bytSendList.Add(cmd);//指令ID
            bytSendList.Add(MotorInfo.MotorId);//电机ID
            if (parameters != null)
            {
                bytSendList.AddRange(parameters);
            }

            _bWait = true;
            int resSend = _port.Send(_sendFrameId, (byte)bytSendList.Count, bytSendList.ToArray());

            #region 发送debug
            string data = string.Empty;
            for (int i = 1; i < bytSendList.Count; i++)
            {
                data += bytSendList[i].ToString("X");
            }
            #endregion

            if (resSend == ResultCode.S_OK)
            {
                if (cmd == MotorConstant.CMD_FB_RELATIVE_MOVE)
                {
                    _log.Debug("电机{0}发送有反馈运行命令{1}成功", MotorInfo.Name, cmd);
                }
                _log.Debug($"DataSend:::::::::::::::::命令{_sendFrameId.ToString("X") + "_" + cmd.ToString("X") + "_" + data}电机{MotorInfo.Name}执行命令{cmd}");
                bool res = _ackEvent.WaitOne(timeOut);
                if (!res)
                {
                    _bWait = false;
                    _errCode = ResultCode.ERR_TIMEOUT;
                    _log.Debug("电机{0}执行命令{1}超时", MotorInfo.Name, cmd);
                }
                else
                {
                    //_log.Debug("电机{0}执行命令{1}结束", MotorInfo.MotorId, cmd);
                }
            }
            else
            {
                _bWait = false;
                _errCode = ResultCode.ERR_DISCONNECTED;
            }

            return _errCode;
        }



        /// <summary>
        /// 电机使能
        /// </summary>
        /// <returns></returns>
        public override int MotorEnabled()
        {
            return SendCommand(MotorConstant.CMD_ENABLED, null, MotorConstant.SIMPLE_CMD_TIME_OUT);
        }

        /// <summary>
        /// 关闭电机使能
        /// </summary>
        /// <returns></returns>
        public override int MotorDisabled()
        {
            return SendCommand(MotorConstant.CMD_DISABLED, null, MotorConstant.SIMPLE_CMD_TIME_OUT);
        }

        /// <summary>
        /// 电机相对移动至指定位置
        /// </summary>
        /// <param name="dir">方向</param>
        /// <param name="step">运行步数</param>
        /// <returns></returns>
        public override int RelativeMove(byte dir, int step)
        {
            int res = ResultCode.S_SUCCESSED;
            if (_bReset)
            {
                if (step >= 0 && step <= MotorInfo.MaxStep)
                {
                    int tmpStep = step;
                    if (dir == MotorConstant.DIR_P)
                    {
                        if (step + CurrentPostion >= MotorInfo.MaxStep)
                        {
                            tmpStep = MotorInfo.MaxStep - CurrentPostion;
                        }
                    }
                    else
                    {
                        if (step > CurrentPostion)
                        {
                            tmpStep = CurrentPostion;
                        }
                    }

                    List<byte> bytParams = new List<byte>();
                    bytParams.Add(dir);
                    bytParams.AddRange(BitConverter.GetBytes(tmpStep));
                    if (dir == MotorConstant.DIR_N)
                    {
                        CurrentPostion -= tmpStep;
                    }
                    else
                    {
                        CurrentPostion += tmpStep;
                    }
                    IsRunning = true;
                    res = SendCommand(MotorConstant.CMD_RELATIVE_MOVE, bytParams.ToArray(), MotorConstant.ELAPSED_TIME_OUT);
                    if (res != ResultCode.S_SUCCESSED)
                    {
                        _log.Error("Timeout", MotorInfo.Name);
                    }
                }
                else
                {
                    res = ResultCode.ERR_OVER_RANG;
                }

            }
            else
            {
                res = ResultCode.ERR_UNRESET;
            }
            return res;
        }

        /// <summary>
        /// 绝对移动至指定位置
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override int AbsMove(int position)
        {
            if (position < 0)
            {
                position = 0;
            }

            //int step = position - CurrentPostion;
            //if (step == 0)
            //{
            //    return ResultCode.S_SUCCESSED;
            //}
            List<byte> bytParams = new List<byte>();
            bytParams.AddRange(BitConverter.GetBytes(position));
            var res = SendCommand(MotorConstant.CMD_ABS_MOVE, bytParams.ToArray(), MotorConstant.ELAPSED_TIME_OUT);
            if (res == ResultCode.S_SUCCESSED)
            {
                CurrentPostion = position;
            }
            return res;
        }

        /// <summary>
        /// 设置电机电流
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override int SetCurrent(int current)
        {
            int res = ResultCode.S_SUCCESSED;

            return res;
        }

        /// <summary>
        /// 设置电机速度
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public override int SetSpeed(int speed)
        {
            int res = ResultCode.S_SUCCESSED;

            return res;
        }

        /// <summary>
        /// 电机复位
        /// </summary>
        /// <returns></returns>
        public override int Reset()
        {
            return Reset(MotorConstant.RESET_TIME_OUT);
        }

        /// <summary>
        /// 电机复位
        /// </summary>
        /// <returns></returns>
        public override int Reset(int timeout)
        {
            CurrentPostion = 0;
            _log.Error("开始：电机{0}执行命令{1}", MotorInfo.Name, MotorConstant.CMD_RESET);
            int res = SendCommand(MotorConstant.CMD_RESET, BitConverter.GetBytes(MotorInfo.ResetStep), timeout);
            if (res != ResultCode.S_SUCCESSED)
            {
                res = SendCommand(MotorConstant.CMD_RESET, BitConverter.GetBytes(MotorInfo.ResetStep), timeout);
            }
            _log.Error("结束：电机{0}执行命令{1}", MotorInfo.Name, MotorConstant.CMD_RESET);
            return res;
        }

        /// <summary>
        /// 查询电机当前位置
        /// </summary>
        public override int QueryPosition()
        {
            return SendCommand(MotorConstant.CMD_QUERY_POS, null, MotorConstant.SIMPLE_CMD_TIME_OUT);
        }

        private bool SendDetectCommand(byte adpAddress, byte[] parameters)
        {
            List<byte> bytSendList = new List<byte>();
            bytSendList.Add(MotorConstant.CMD_DETECT);
            bytSendList.Add(adpAddress);
            if (parameters != null)
            {
                bytSendList.AddRange(parameters);
            }
            _bWait = true;
            int resSend = _port.Send(_sendFrameId, (byte)bytSendList.Count, bytSendList.ToArray());
            bool res = true;
            if (resSend == ResultCode.S_OK)
            {
                res = _ackEvent.WaitOne(MotorConstant.ELAPSED_TIME_OUT);
                if (!res)
                {
                    _log.Debug("电机{0}执行命令{1}超时", MotorInfo.Name, MotorConstant.CMD_DETECT);
                }
            }
            else
            {
                _bWait = false;
                res = false;
                _errCode = ResultCode.ERR_DISCONNECTED;
            }
            return res;
        }
        /// <summary>
        /// 开始液面探测
        /// </summary>
        /// <param name="step">电机下降步数，是针对原点的坐标</param>
        /// <returns>错误码（0表示成功）</returns>
        public override int LiquidDetection(byte adpAddress, ref int step)
        {
            bool res = SendDetectCommand(adpAddress, null);//探测液面指令
            step = _detectStep;//待移动步数
            int code = ResultCode.S_SUCCESSED;
            if (res)
            {
                code = _errCode;
            }
            else
            {
                _bWait = false;
                code = ResultCode.ERR_TIMEOUT; //超时
            }
            return code;
        }

        public override int LiquidDetection(byte adpAddress, byte speed, ref int step)
        {
            bool res = SendDetectCommand(adpAddress, new byte[] { speed });
            step = _detectStep;
            int code = ResultCode.S_SUCCESSED;
            if (res)
            {
                code = _errCode;
            }
            else
            {
                _bWait = false;
                code = ResultCode.ERR_TIMEOUT; //超时
            }
            return code;
        }

        /// <summary>
        /// 设置探液面Z轴下降最大深度
        /// </summary>
        /// <param name="depth">最大深度对应步数</param>
        /// <returns></returns>
        public override int SetDetectDepth(int depth)
        {
            int res = SendCommand(MotorConstant.CMD_SET_DETECT_DEPTH, BitConverter.GetBytes(depth), Common.SIMPLE_CMD_TIME_OUT);
            _log.Debug("地址为{0}电机{1}设置探测液面步数为{2}，执行结果为{3:X}", MotorInfo.DeviceId, MotorInfo.MotorId, depth, res);
            return res;
        }

        public override int CheckOptocouplerState(ref bool isOn)
        {
            int res = SendCommand(MotorConstant.CMD_CHECK_OC_STATE, null, Common.SIMPLE_CMD_TIME_OUT);
            isOn = (_ocstate == Common.SIGN_OC_ON);
            return res;
        }

        public override int RelativeMoveWithUnreset(byte dir, int step)
        {
            List<byte> bytParams = new List<byte>();
            bytParams.Add(dir);
            bytParams.AddRange(BitConverter.GetBytes(step));
            int res = SendCommand(MotorConstant.CMD_RELATIVE_MOVE, bytParams.ToArray(), Common.ELAPSED_TIME_OUT);
            return res;
        }

        /// <summary>
        /// 设置慢速标记
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public override int SetSlowSpeedFlag(byte flag = 1)
        {
            int tmp = 0;
            int res = SendCommand(MotorConstant.CMD_SET_LOAD_TIP_FLAG, new byte[] { flag }, Common.SIMPLE_CMD_TIME_OUT);
            return res;
        }

        private int StartFeedBackRelativeMove(byte dir, int step)
        {
            int res = ResultCode.S_SUCCESSED;
            if (_bReset)
            {
                int tmpStep = step;
                if (dir == MotorConstant.DIR_P)
                {
                    if (step + CurrentPostion >= MotorInfo.MaxStep)
                    {
                        tmpStep = MotorInfo.MaxStep - CurrentPostion;
                    }
                }
                else
                {
                    if (step > CurrentPostion)
                    {
                        tmpStep = CurrentPostion;
                    }
                }

                List<byte> bytParams = new List<byte>();
                bytParams.Add(dir);
                bytParams.AddRange(BitConverter.GetBytes(tmpStep));
                if (dir == MotorConstant.DIR_N)
                {
                    CurrentPostion -= tmpStep;
                }
                else
                {
                    CurrentPostion += tmpStep;
                }
                IsRunning = true;
                _bWaitEnd = false;
                res = SendCommand(MotorConstant.CMD_FB_RELATIVE_MOVE, bytParams.ToArray(), MotorConstant.ELAPSED_TIME_OUT);
                if (res != ResultCode.S_SUCCESSED)
                {
                    _log.Error("Timeout", MotorInfo.Name);
                }

            }
            else
            {
                res = ResultCode.ERR_UNRESET;
            }
            return res;
        }

        private bool _bMove = true;
        public override int StartFeedBackAbsMove(int position)
        {
            int step = position - CurrentPostion;
            if (step == 0)
            {
                _bMove = false;
                return ResultCode.S_SUCCESSED;
            }
            else
            {
                _bMove = true;
                byte dir = MotorConstant.DIR_P;
                if (step < 0)
                {
                    dir = MotorConstant.DIR_N;
                }
                return StartFeedBackRelativeMove(dir, Math.Abs(step));
            }
        }

        /// <summary>
        /// 等待相对运行结束，此函数必须在StartFeedBackMove函数后调用
        /// </summary>
        /// <returns></returns>
        public override int WaitForMoveEnd()
        {
            _errCode = ResultCode.S_SUCCESSED;
            if (_bMove)
            {
                bool res = _ackEventMoveEnd.WaitOne(MotorConstant.ELAPSED_TIME_OUT);
                if (res)
                {
                    return _errCode;
                }
                else
                {
                    return ResultCode.ERR_TIMEOUT;
                }
            }
            else
            {
                return ResultCode.S_SUCCESSED;
            }
        }

        private int _currentAbsStep = 0;

        public override int GetCurrentPos(ref int step)
        {
            int res = SendCommand(MotorConstant.CMD_GET_CURRENT_POS, null, Common.ELAPSED_TIME_OUT);
            if (res == ResultCode.S_SUCCESSED)
            {
                step = _currentAbsStep;
            }
            return res;
        }

        public override int StartFeedBackReset()
        {
            _bMove = true;
            CurrentPostion = 0;
            return SendCommand(MotorConstant.CMD_FB_RESET, BitConverter.GetBytes(MotorInfo.ResetStep), MotorConstant.RESET_TIME_OUT);

        }

        public override void SetResetDir(byte dir)
        {

        }

        public override int GetVersion(ref string version)
        {
            _strVersion = string.Empty;
            int resCode = ResultCode.S_SUCCESSED;
            int resSend = _port.Send(_sendFrameId, 1, new byte[] { Common.CMD_VERSION });
            if (resSend == ResultCode.S_OK)
            {
                _bWaitQueryVersion = true;
                bool res = _ackQueryVersion.WaitOne(Common.SIMPLE_CMD_TIME_OUT);
                if (!res)
                {
                    _bWaitQueryVersion = false;
                    resCode = ResultCode.ERR_TIMEOUT;
                }
                if (resCode == ResultCode.S_SUCCESSED)
                {
                    version = _strVersion;
                }
            }
            else
            {
                resCode = ResultCode.ERR_DISCONNECTED;
            }
            return resCode;
        }

        public override int SetError(int errValue)
        {
            return SendCommand(MotorConstant.CMD_SET_ERROR, null, Common.SIMPLE_CMD_TIME_OUT);

        }
    }
}
