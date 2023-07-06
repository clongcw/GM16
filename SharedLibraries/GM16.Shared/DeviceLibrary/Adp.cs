using GM16.Shared.CommonLibrary;
using GM16.Shared.CommunicationLibrary;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Logger = GM16.Shared.Services.Logger;

namespace GM16.Shared.DeviceLibrary
{
    public class Adp : CommonAdp
    {
        private const float MAX_HIGH_SPEED = 1000.0f;
        private const float MIN_HIGH_SPEED = 2.5f;
        private const float MAX_CUTOFF_SPEED = 200.0f;
        private const float MIN_CUTOFF_SPEED = 2.5f;
        private Port _port;
        private Logger _log = Logger.Instance;
        private AutoResetEvent _ackEvent;
        private int _errCode = 0;
        private bool _bSended;
        private bool _reportState = false;
        private int _currentPressure = 0;


        public Adp(Port port, int sequence)
        {
            AdpInfo = new AdpInfo();
            AdpInfo.Name = "Adp" + sequence.ToString();
            AdpInfo.NodeId = sequence;
            AdpInfo.Enabled = true;
            _ackEvent = new AutoResetEvent(false);
            _port = port;
            _port.DataReceived += _port_DataReceived;
            //Load();
            PressureInitial = 0;
        }

        void _port_DataReceived(object sender, CanDataReceivedEventArgs e)
        {
            if ((e.CanData.FrameFlag == FrameFlag.Stanard))
            {
                CanData recObj = e.CanData.Clone();
                UInt16 deviceId = Common.ParseDeviceId(recObj.ID);
                if ((deviceId < Common.MIN_DEVICE_ID) || (deviceId > Common.MAX_DEVICE_ID))
                {
                    //_log.DebugFormat("ADP反馈信息，ID为：{0}", deviceId);
                    FrameId frameId = new FrameId();
                    AdpConst.AnalysisFramID(recObj.ID, ref frameId);
                    if (frameId.DevId == AdpInfo.NodeId)
                    {

                        if (frameId.Group == AdpConst.GROUP_GUIDE)
                        {
                            byte msgGuide = byte.Parse(AdpConst.GROUP_ADP.ToString() + frameId.DevId.ToString(), NumberStyles.HexNumber);
                            _port.Send(AdpConst.ID_GUIDE, 2, new byte[] { msgGuide, msgGuide });
                        }
                        if ((recObj.Length == 0))
                        {
                            //_log.Debug("确认");
                            //_ackEvents.Set();
                        }
                        if (recObj.Length > 1)
                        {
                            if (_bSended)
                            {
                                _bSended = false;
                                if (frameId.FrameType == AdpConst.FRAME_ACTION)
                                {
                                    if (recObj.Data[1] == 0x60)
                                    {
                                        if (OnReceiveStatus != null)
                                        {
                                            OnReceiveStatus(recObj.Data[0]);
                                        }
                                        //_log.DebugFormat("返回命令执行状态信息ADP{0}:{1}", frameId.DevId, e.CanData.Data[0]); 
                                        _errCode = recObj.Data[0] - 0x20;
                                        _ackEvent.Set();

                                    }
                                }
                                else if (frameId.FrameType == AdpConst.FRAME_REPORT)
                                {
                                    _errCode = recObj.Data[0] - 0x20;
                                    if (recObj.Length > 2)
                                    {
                                        if (_bReportTipState)
                                        {
                                            byte tipState = (byte)(recObj.Data[2] - 0x30);
                                            if (tipState == AdpConst.TIP_EXIST)
                                            {
                                                _reportState = true;
                                            }
                                            else
                                            {
                                                _reportState = false;
                                            }
                                        }
                                        else if (_bReportPressure)
                                        {
                                            byte[] bytPrs = new byte[recObj.Length - 2];
                                            Array.Copy(recObj.Data, 2, bytPrs, 0, bytPrs.Length);
                                            _currentPressure = int.Parse(ASCIIEncoding.ASCII.GetString(bytPrs));
                                        }
                                    }
                                    _ackEvent.Set();
                                }
                            }
                        }
                    }

                }

            }
        }

        public delegate void DelegateRefreshReceiveMsg(CanData msg);
        public DelegateRefreshReceiveMsg OnReceiveMsg = null;
        public delegate void DelegateReceiveStatus(byte status);
        public DelegateReceiveStatus OnReceiveStatus = null;

        private int SendMessage(UInt32 frameId, byte[] msg, int timeOut)
        {
            _errCode = ResultCode.S_SUCCESSED;
            _bSended = true;
            int res = _port.Send(frameId, (byte)msg.Length, msg);
            if (res == ResultCode.S_OK)
            {
                if (timeOut > 0)
                {
                    bool ackRes = _ackEvent.WaitOne(timeOut);
                    //_log.DebugFormat("Adp{0}执行命令{1}结果{2}",AdpInfo.Sequence,msg[0], ackRes);
                    if (!ackRes)
                    {
                        _bSended = false;
                        _errCode = ResultCode.ERR_TIMEOUT;
                    }
                }
            }
            else
            {
                _bSended = false;
                _errCode = ResultCode.ERR_DISCONNECTED;
            }
            return _errCode;
        }

        private int SendMessage(UInt32 frameId, byte msg)
        {
            return SendMessage(frameId, new byte[] { msg }, AdpConst.TIMEOUT_CMD);
        }


        /// <summary>
        /// 以帧类型1执行命令
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="data">命令参数</param>
        /// <returns>命令执行结果，超时返回false</returns>
        private int ExecuteCommandByFrame1(byte cmd, byte[] data)
        {
            return ExecuteCommand(cmd, data, AdpConst.FRAME_ACTION, true);
        }

        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        /// <param name="frameType"></param>
        /// <param name="isAddR"></param>
        /// <returns>resultCode 0成功，其他失败</returns>
        private int ExecuteCommand(byte cmd, byte[] data, byte frameType, bool isAddR)
        {
            UInt32 sendId = AdpConst.GenerateFramID(AdpConst.DIR_DOWN, AdpConst.GROUP_ADP, (byte)AdpInfo.NodeId, frameType);
            byte[] msg;
            if (data != null)
            {
                int length = 2 + data.Length;
                if (!isAddR)
                {
                    length = 1 + data.Length;
                }

                msg = new byte[length];
                msg[0] = cmd;
                data.CopyTo(msg, 1);
                if (isAddR)
                {
                    msg[1 + data.Length] = AdpConst.CMD_EXECUTE;
                }
            }
            else
            {
                if (isAddR)
                {
                    msg = new byte[] { cmd, AdpConst.CMD_EXECUTE };
                }
                else
                {
                    msg = new byte[] { cmd };
                }
            }
            int res = SendMessage(sendId, msg, AdpConst.TIMEOUT_CMD);
            return res;
        }

        private int OnInit()
        {
            int res = ResultCode.S_SUCCESSED;
            if (AdpInfo.Enabled)
            {
                res = ExecuteCommandByFrame1(AdpConst.CMD_INIT, null);
            }
            AdpInfo.IsInitialized = res == ResultCode.S_SUCCESSED ? true : false;
            _log.Debug("ADP{0}初始化{1}！", AdpInfo.NodeId, res);
            _log.Error("ADP{0}初始化{1}！", AdpInfo.NodeId, res);

            //读取Adp初始压力值，用于判断堵塞
            if (res == ResultCode.S_SUCCESSED)
            {
                int pressure = 0;
                res = ReportPressure(ref pressure);
                if (res == ResultCode.S_SUCCESSED)
                {
                    PressureInitial = pressure;
                    _log.Debug("ADP{0}初始压力为{1}！", AdpInfo.NodeId, PressureInitial);
                    _log.Error("ADP{0}初始压力为{1}！", AdpInfo.NodeId, PressureInitial);
                }
            }
            return res;
        }

        /// <summary>
        /// 初始化指定ADP
        /// </summary>
        public override int Init()
        {
            return OnInit();
        }

        public delegate void DelegateInitState(bool res);
        public DelegateInitState OnInitCompleted = null;


        /// <summary>
        /// 打掉Tip头
        /// </summary>
        /// <param name="mode">模式（0-1）</param>
        /// <returns></returns>
        public override int EjectTip(char mode)
        {
            _log.Debug("ADP{0}打掉Tip头", AdpInfo.NodeId);
            return ExecuteCommandByFrame1(AdpConst.CMD_EJECT, new byte[] { (byte)mode });
        }


        /// <summary>
        /// ADP复位
        /// </summary>
        /// <returns></returns>
        public override int Reset()
        {
            _log.Debug("ADP{0}复位", AdpInfo.NodeId);
            return ExecuteCommandByFrame1(AdpConst.CMD_RESET, null);
        }

        private byte[] GetBytes(string strData)
        {
            if (!string.IsNullOrEmpty(strData))
            {
                byte[] res = new byte[strData.Length];
                for (int i = 0; i < strData.Length; i++)
                {
                    res[i] = (byte)strData[i];
                }
                return res;
            }
            else
            {
                return null;
            }
        }

        private int SetParameter(byte cmd, float n1, float n2)
        {
            return ExecuteCommandByMultipleFrames(cmd, n1, n2);
        }

        private int ExecuteCommandByMultipleFrames(byte cmd, float n1, float n2)
        {
            int res = ResultCode.S_SUCCESSED;
            string strData = n1.ToString() + "," + n2.ToString();
            byte[] bytData = GetBytes(strData);
            List<byte> bytList = new List<byte>();
            bytList.Add(cmd);
            bytList.AddRange(bytData);
            bytList.Add(AdpConst.CMD_EXECUTE);
            int frameCount = bytList.Count / 8;
            int remain = bytList.Count % 8;
            if (remain > 0)
            {
                frameCount += 1;
            }

            if (frameCount > 1)
            {
                int startId = 0;
                for (int i = 0; i < frameCount; i++)
                {
                    if (i == 0)//起始帧
                    {
                        UInt32 sendId = AdpConst.GenerateFramID(AdpConst.DIR_DOWN, AdpConst.GROUP_ADP, (byte)AdpInfo.NodeId, AdpConst.FRAME_MUTIL_START);
                        res = SendMessage(sendId, bytList.GetRange(startId, 8).ToArray(), 0);
                        startId += 8;
                    }
                    else if (i == frameCount - 1)//结束帧
                    {
                        UInt32 sendId = AdpConst.GenerateFramID(AdpConst.DIR_DOWN, AdpConst.GROUP_ADP, (byte)AdpInfo.NodeId, AdpConst.FRAME_ACTION);
                        res = SendMessage(sendId, bytList.GetRange(bytList.Count - remain, remain).ToArray(), AdpConst.TIMEOUT_CMD);
                    }
                    else //中间帧
                    {
                        UInt32 sendId = AdpConst.GenerateFramID(AdpConst.DIR_DOWN, AdpConst.GROUP_ADP, (byte)AdpInfo.NodeId, AdpConst.FRAME_MUTIL_MID);
                        res = SendMessage(sendId, bytList.GetRange(startId, 8).ToArray(), 0);
                        startId += 8;
                    }
                }
            }
            else
            {
                res = ExecuteCommandByFrame1(cmd, bytData);
            }

            return res;
        }


        /// <summary>
        /// 设置ADP最高速度
        /// </summary>
        /// <param name="speed">速度值</param>
        /// <param name="mode">参数模式（0-1）</param>
        /// <returns></returns>
        public override int SetMaxSpeed(float speed, byte mode)
        {
            if (speed < MIN_HIGH_SPEED)
            {
                speed = MIN_HIGH_SPEED;
            }
            else if (speed > MAX_HIGH_SPEED)
            {
                speed = MAX_HIGH_SPEED;
            }
            int res = SetParameter(AdpConst.CMD_MAX_SPEED, speed, mode);
            return res;
        }


        /// <summary>
        /// 设置启动速度
        /// </summary>
        /// <param name="speed">速度值</param>
        /// <param name="mode">参数模式（0-1）</param>
        /// <returns></returns>
        public override int SetStartSpeed(float speed, byte mode)
        {
            //_log.DebugFormat("设置ADP{0}启动速度", AdpInfo.NodeId);
            return SetParameter(AdpConst.CMD_START_SPEED, speed, mode);
        }


        /// <summary>
        /// 设置ADP断流速度
        /// </summary>
        /// <param name="speed">速度值</param>
        /// <param name="mode">参数模式（0-1）</param>
        /// <returns></returns>
        public override int SetCutOffSpeed(float speed, byte mode)
        {
            //_log.DebugFormat("设置ADP{0}断流速度", AdpInfo.NodeId);
            if (speed < MIN_CUTOFF_SPEED)
            {
                speed = MIN_CUTOFF_SPEED;
            }
            else if (speed > MAX_CUTOFF_SPEED)
            {
                speed = MAX_CUTOFF_SPEED;
            }
            return SetParameter(AdpConst.CMD_CUTOFF_SPEED, speed, mode);
        }


        /// <summary>
        /// 设置坡度
        /// </summary>
        /// <param name="upGrade"></param>
        /// <param name="downGrade"></param>
        /// <returns></returns>
        public int SetGradient(UInt32 upGrade, UInt32 downGrade)
        {
            //_log.DebugFormat("设置ADP{0}坡度", AdpInfo.NodeId);
            return SetParameter(AdpConst.CMD_GRADE, upGrade, downGrade);
        }


        /// <summary>
        /// 将ADP活塞移至指定绝对位置，排空液体
        /// </summary>
        /// <param name="pos">体积</param>
        /// <param name="mode">模式（0-1）</param>
        /// <returns></returns>
        public override int AbsMove(double pos, byte mode)
        {
            //_log.DebugFormat("ADP{0}绝对移动", AdpInfo.NodeId);
            float tmpPos = (float)pos;
            if (mode == 0)
            {
                tmpPos = (int)pos;
            }
            return SetParameter(AdpConst.CMD_ABS, tmpPos, mode);
        }


        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="pos">体积</param>
        /// <param name="mode">模式（0-1）</param>
        /// <returns></returns>
        public override int Aspirate(double pos, byte mode)
        {
            //_log.DebugFormat("ADP{0}吸液", AdpInfo.NodeId);
            float tmpPos = (float)Math.Round(pos, 3);
            if (mode == 0)
            {
                tmpPos = (int)pos;
            }
            return SetParameter(AdpConst.CMD_PULL, tmpPos, mode);
        }


        /// <summary>
        /// 分配液体
        /// </summary>
        /// <param name="pos">体积</param>
        /// <param name="mode">模式（0-1）</param>
        /// <returns></returns>
        public override int Dispense(double pos, byte mode)
        {
            //_log.DebugFormat("ADP{0}排液", AdpInfo.NodeId);
            float tmpPos = (float)pos;
            if (mode == 0)
            {
                tmpPos = (int)pos;
            }
            return SetParameter(AdpConst.CMD_DETACH, tmpPos, mode);
        }

        /// <summary>
        /// 开始液位检测
        /// </summary>
        /// <param name="mode">模式（0-1）</param>
        public override int LiquidDetection(char mode)
        {
            //_log.DebugFormat("ADP{0}开始液位检测", AdpInfo.NodeId);
            return ExecuteCommandByFrame1(AdpConst.CMD_BEGIN_DETECTION, new byte[] { (byte)mode });
        }

        /// <summary>
        /// 设置LLD和压力流参数
        /// </summary>
        /// <param name="mode">参数模式（0-8）</param>
        /// <param name="paramValue">对应参数值</param>
        public override int SetPLLD(byte mode, UInt32 paramValue)
        {
            //_log.DebugFormat("ADP{0}设置LLD和压力流参数", AdpInfo.NodeId);
            return SetParameter(AdpConst.CMD_PLLD_PARAM, mode, paramValue);
        }

        private bool _bReportTipState = false;

        /// <summary>
        /// 报告Tip头状态
        /// </summary>
        /// <param name="state">false：不存在，true：存在</param>
        /// <returns></returns>
        public override int ReportTipState(ref bool state)
        {
            _bReportTipState = true;
            _reportState = false;
            string strData = AdpConst.S_REPORT_TIP.ToString();
            byte[] bytdata = GetBytes(strData);
            int res = ExecuteCommand(AdpConst.CMD_REPORT, bytdata, AdpConst.FRAME_REPORT, false);
            if (res == ResultCode.S_SUCCESSED)
            {
                state = _reportState;
            }
            _bReportTipState = false;
            return res;
        }

        private bool _bReportPressure = false;

        /// <summary>
        /// 以ADC计数报告当前压力
        /// </summary>
        /// <param name="pressure"></param>
        /// <returns></returns>
        public override int ReportPressure(ref int pressure)
        {
            _bReportPressure = true;
            string strData = AdpConst.S_REPORT_PRESSURE.ToString();
            byte[] bytdata = GetBytes(strData);
            int res = ExecuteCommand(AdpConst.CMD_REPORT, bytdata, AdpConst.FRAME_REPORT, false);
            if (res == ResultCode.S_SUCCESSED)
            {
                pressure = _currentPressure;
            }
            _bReportPressure = false;
            return res;
        }
    }
}
