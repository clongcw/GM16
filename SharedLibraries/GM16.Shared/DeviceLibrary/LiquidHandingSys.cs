using GM16.Shared.CommunicationLibrary;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;
using GM16.Shared.Services;
using GM16.Shared.CommonLibrary;
using GM16.Shared.EntityModel;
using GM16.Shared.CommonLibrary.Enums;
using GM16.Shared.EntityModel.DBContext;

namespace GM16.Shared.DeviceLibrary
{
    /// <summary>
    /// 液体移动系统
    /// </summary>
    public class LiquidHandingSys
    {

        #region 声明实例
        private Logger _log = Logger.Instance;

        private const int MAX_STEP_X = 75000;
        private const int MAX_STEP_Y = 100000;
        private const int MAX_STEP_Z = 13800;
        private AutoResetEvent _retryEvent;

        public Dictionary<Channels, Motor> MotorYDictionary { get; } = new Dictionary<Channels, Motor>();



        public Motor MotorX { get; }
        public Motor MotorY { get; }
        public Motor MotorZ { get; }
        public CommonAdp Adp { get; }

        #region 委托事件
        private bool _isActuralLoadTip = false;

        public event Action<int, int> TipUsedAct;
        public Func<string, int, int, Location> GetSpecLoc;

        public event Action<int> OnTipTypeChanged;

        public event Action<int, int> TipCurrentState;

        //public event Action<int, bool> OnRefreshPcrChipStatus;//更新PCR加样孔使用状态
        #endregion
        #endregion

        #region 构造函数
        public LiquidHandingSys(Port port, ushort deviceId)
        {
            Adp = new Adp(port, 0);
            MotorX = new CommonMotor(port, deviceId, 1, "MotorX", MAX_STEP_X);
            MotorY = new CommonMotor(port, 0x27, 0, "MotorY", MAX_STEP_Y);
            MotorZ = new CommonMotor(port, deviceId, 0, "MotorZ", MAX_STEP_Z);
            DevStateManager = new DeviceStateManager();
            DevStateManager.DeviceStateChanged += _devStateManager_DeviceStateChanged;
            _retryEvent = new AutoResetEvent(false);
            RunningHeight = 100;
            LoadTipStep = 1150;
            LoadTipSpeed = 16;
            LeaveLiquidSpeed = 16;
            AspirateStep = 100;
            TipVolume = 1000;
            DetectMaxStep = 8000;
            _isNeedWarningLevel = true;
            MinLiquidLevelStep = 500;
            MinPressureDifference = 100;
            IsWarningLiquidLevel = false;
            LiquidLevelErrStep = 85;
            IsAgingTest = false;
            LeaveLiquidStep = 500;
            DisInnerLiquidStep = 500;
            OverLiquidLevelStep = 50;
            DefaultDetectSpeed = 32;
            DeepDetectSpeed = 90;
            DetectOffsetStep = 0;
            MotorYDictionary.Add(Channels.Tip, MotorY);
            TipManager = new TipManager();
        }
        #endregion

        #region 方法

        #endregion


        /// <summary>
        /// X、Y轴安全运动的Z轴高度
        /// </summary>
        public int RunningHeight
        {
            get;
            set;
        }

        /// <summary>
        /// 慢速取Tip步数
        /// </summary>
        public int LoadTipStep
        {
            get;
            set;
        }

        /// <summary>
        /// Z取Tip头速度
        /// </summary>
        public int LoadTipSpeed
        {
            get;
            set;
        }


        /// <summary>
        /// Adp离开液面速度
        /// </summary>
        public int LeaveLiquidSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 吸液时ADP单次扎进液面步数，吸液量较大时分多次扎入
        /// </summary>
        public int AspirateStep
        {
            get;
            set;
        }

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public bool IsInitialized
        {
            get;
            set;
        }

        private bool _isNeedAccReset; //初始化之后的复位动作分两步执行

        /// <summary>
        /// 当前使用的Tip头容量
        /// </summary>
        public int TipVolume
        {
            get;
            set;
        }

        /// <summary>
        /// 探液面最大步数
        /// </summary>
        public int DetectMaxStep
        {
            get;
            private set;
        }

        /// <summary>
        /// 单位步数高度容量
        /// </summary>
        public double UnitStepVolume
        {
            get
            {
                return UnitmmVolume;
            }
        }

        public double UnitmmVolume { get; set; }

        /// <summary>
        /// Tip头管理
        /// </summary>
        public TipManager TipManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否加样校正
        /// </summary>
        public bool IsCalibration
        {
            get;
            set;
        }


        /// <summary>
        /// 加样时，进入步数
        /// </summary>
        public int DisInnerLiquidStep
        {
            get;
            set;
        }
        /// <summary>
        /// 探测液面误差
        /// </summary>
        public int LiquidLevelErrStep
        {
            get;
            set;
        }

        /// <summary>
        /// 是否是系统老化测试
        /// </summary>
        public bool IsAgingTest
        {
            get;
            set;
        }

        /// <summary>
        /// Adp探测到液面后离开液面步数
        /// </summary>
        public int LeaveLiquidStep
        {
            get;
            set;
        }


        /// <summary>
        /// 吸液时Z轴下降超过液位步数
        /// </summary>
        public int OverLiquidLevelStep
        {
            get;
            set;
        }

        public bool IsWarningLiquidLevel
        {
            get;
            set;
        }

        public int MinLiquidLevelStep
        {
            get;
            set;
        }

        public int MinPressureDifference
        {
            get;
            set;
        }

        public byte DefaultDetectSpeed { get; set; }

        public byte DeepDetectSpeed { get; set; }

        public double ConeHeight { get; set; }

        public double ConeVol { get; set; }

        public int DetectOffsetStep { get; set; }

        private bool _isNeedWarningLevel = true;

        private DeviceState _deviceState = DeviceState.Idle;

        private int _aspirateDepthStep = 0;
        private int _dispenseDepthStep = 0;//排液转化步数

        public bool IsIgnoreLiquildEnough { get; set; }

        public int DisEmptyStep { get; set; }

        public int PrickStep { get; set; } = 200;

        public int CurrentErrCode { get; set; }

        private int GetResultCode(int signCode, int res)
        {
            if (res != ResultCode.S_SUCCESSED)
            {
                return Common.CODE_LH + signCode + res;
            }
            else
            {
                return res;
            }
        }

        private int GetChannelResultCode(Channels ch, int signCode, int res)
        {
            if (res != ResultCode.S_SUCCESSED)
            {
                return Common.CODE_MODULE_CH + Common.CODE_CHANNEL * (int)ch + signCode + res;
            }
            else
            {
                return res;
            }
        }

        /// <summary>
        /// 初始化电机
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public int InitMotor(Channels ch)
        {
            var res = MotorZ.Initialize();
            var signCode = Common.CODE_Z;
            if (res == ResultCode.S_SUCCESSED)
            {
                var taskX = Task.Run(() => { return MotorX.Initialize(); });
                var taskY = Task.Run(() => { return MotorYDictionary[ch].Initialize(); });
                Task.WaitAll(taskX, taskY);
                signCode = Common.CODE_X;
                if (taskX.Result == ResultCode.S_SUCCESSED)
                {
                    signCode = Common.CODE_Y;
                    res = GetChannelResultCode(ch, signCode, taskY.Result);
                }
                else
                {
                    res = GetResultCode(signCode, res);
                }
            }
            else
            {
                res = GetResultCode(signCode, res);
            }

            return res;
        }
        /// <summary>
        /// 初始化ADP
        /// </summary>
        /// <returns></returns>
        public int InitAdp()
        {
            return GetResultCode(Common.CODE_ADP, Adp.Init());
        }

        /// <summary>
        /// 移液系统初始化
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public int InitAll(Channels ch)
        {
            var res = ResultCode.S_SUCCESSED;
            var taskMotor = Task.Run(() => { return InitMotor(ch); });//电机初始化
            var taskAdp = Task.Run(() => { return InitAdp(); });//初始化ADP
            if (taskMotor.Result == ResultCode.S_SUCCESSED)
            {
                res = taskAdp.Result;
                if (res != ResultCode.S_SUCCESSED)
                {
                    var tmpRes = res % 100;
                    if (tmpRes == ResultCode.ERR_ADP_INITIALIZATION_ERROR || tmpRes == ResultCode.ERR_ADP_NOT_USED)
                    {
                        _log.Error($"通道{ch.ToString()}开始排掉tip头");
                        tmpRes = EjectTip();
                        _log.Error($"通道{ch.ToString()}结束排掉tip头");
                        if (tmpRes == ResultCode.S_SUCCESSED)
                        {
                            _log.Error($"通道{ch.ToString()}开始初始化");
                            res = Init();
                            _log.Error($"通道{ch.ToString()}结束初始化");
                        }
                    }
                }
            }
            else
            {
                res = taskMotor.Result;
            }

            return res;
        }

        public int InitXZ()
        {
            var res = MotorZ.Initialize();
            var signCode = Common.CODE_Z;
            if (res == ResultCode.S_SUCCESSED)
            {
                res = MotorX.Initialize();
                signCode = Common.CODE_X;
            }

            return GetResultCode(signCode, res);
        }

        public int Init()
        {
            var res = ResultCode.S_SUCCESSED;
            var taskMotor = Task.Run(() => { return InitXZ(); });
            var taskAdp = Task.Run(() => { return InitAdp(); });
            if (taskMotor.Result == ResultCode.S_SUCCESSED)
            {
                res = taskAdp.Result;
                if (res != ResultCode.S_SUCCESSED)
                {
                    var tmpRes = res % 100;
                    if (tmpRes == ResultCode.ERR_ADP_INITIALIZATION_ERROR || tmpRes == ResultCode.ERR_ADP_NOT_USED)
                    {
                        tmpRes = EjectTip();
                        if (tmpRes == ResultCode.S_SUCCESSED)
                        {
                            res = Init();
                        }
                    }
                }
            }
            else
            {
                res = taskMotor.Result;
            }

            return res;
        }

        /// <summary>
        /// 移动坐标轴
        /// </summary>
        /// <param name="signCode">坐标轴</param>
        /// <param name="step">步数</param>
        /// <returns></returns>
        private int MotorMove(int signCode, int step)
        {
            var res = ResultCode.S_SUCCESSED;
            switch (signCode)
            {
                case Common.CODE_X:
                    res = GetResultCode(signCode, MotorX.AbsMove(step));
                    break;
                case Common.CODE_Y:
                    res = GetChannelResultCode(Channels.Tip, signCode, MotorY.AbsMove(step));
                    break;
                case Common.CODE_Z:
                    res = GetResultCode(signCode, MotorZ.AbsMove(step));
                    break;
            }

            return GetResultCode(signCode, res);
        }

        public int MoveToXY(int x, int y, Channels ch = Channels.Tip)
        {
            var res = ResultCode.S_SUCCESSED;
            //var taskX = Task.Run(() => { return MotorMove(Common.CODE_X,x); });
            //var taskY = Task.Run(() => { return MoveToY(y,ch); });
            //res = taskX.Result;
            //if (taskX.Result == ResultCode.S_SUCCESSED)
            //{
            //    res = taskY.Result;
            //}
            #region 同步等反馈
            res = MotorMove(Common.CODE_X, x);
            if (res == ResultCode.S_SUCCESSED)
            {
                res = MoveToY(y, ch);
            }
            #endregion
            return res;
        }

        public int MoveTo(int x, int y, int z, Channels ch = Channels.Tip)
        {
            var res = MoveToXY(x, y, ch);
            if (CheckDeviceState(res, ch) == ResultCode.S_SUCCESSED)
            {
                res = MotorMove(Common.CODE_Z, z);
            }

            res = CheckDeviceState(res, ch);

            return res;
        }

        public int MoveTo(Location loc, Channels ch = Channels.Tip)
        {
            return MoveTo(loc.X, loc.Y, loc.Z, ch);
        }

        public int MoveToX(int x)
        {
            return MotorMove(Common.CODE_X, x);
        }

        public int MoveToY(int y, Channels ch = Channels.Tip)
        {
            if (ch < Channels.Pcr1)
            {
                return GetChannelResultCode(ch, Common.CODE_Y, MotorYDictionary[ch].AbsMove(y));
            }
            else
            {
                return ResultCode.S_SUCCESSED;
            }
        }

        public int MoveToZ(int z)
        {
            return MotorMove(Common.CODE_Z, z);
        }

        public int MoveToXZ(int x, int z)
        {
            var res = MoveToSafeZ();
            if (res == ResultCode.S_SUCCESSED)
            {
                if (res == ResultCode.S_SUCCESSED)
                {
                    res = MoveToX(x);
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        res = MoveToZ(z);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// 移动Z到安全位置
        /// </summary>
        /// <returns></returns>
        public int MoveToSafeZ()
        {
            return MotorMove(Common.CODE_Z, RunningHeight);
        }

        //public int ChkDevState()
        //{
        //    while (_devStateManager.CurrentDevState == DeviceState.Pause)
        //        Thread.Sleep(10);
        //    return _devStateManager.CurrentDevState != DeviceState.Stop ? ResultCode.S_SUCCESSED : ResultCode.S_STOP;
        //}

        /// <summary>
        /// 反馈TIP头状态
        /// </summary>
        /// <param name="bExist"></param>
        /// <returns></returns>
        private int ReportTipState(bool bExist)
        {
            int res = ResultCode.S_SUCCESSED;
            int tmpRes = ResultCode.S_SUCCESSED;
            bool tipState = false;
            tmpRes = Adp.ReportTipState(ref tipState);//false不存在，true存在
            if (tmpRes == ResultCode.S_SUCCESSED)//指令发送成功
            {
                if (bExist && !tipState)//Tip头不存在
                {
                    res = GetResultCode(Common.CODE_ADP, ResultCode.ERR_TIP_NONE_EXIST);
                    CurrentErrCode = res;
                }
                else if (!bExist && tipState)//存在
                {
                    res = GetResultCode(Common.CODE_ADP, ResultCode.ERR_TIP_EXIST);
                    CurrentErrCode = res;
                }
            }
            else//失败
            {
                res = GetResultCode(Common.CODE_ADP, tmpRes);
            }
            return res;
        }

        public int MotorZReset()
        {
            return GetResultCode(Common.CODE_Z, MotorZ.Initialize());
        }

        private bool _autoReTry = false;

        public delegate void DelegateRetryCommand(int errCode, bool isIgnore);

        public Common.DelegateRetryCommand OnRetryCommand = null;

        private bool DoRetry(int errCode, bool isIgnore)
        {
            if (OnRetryCommand != null)
            {
                OnRetryCommand(RetryModule.LiqHa, errCode, isIgnore);
                _retryEvent.WaitOne();
                return true;
            }
            return false;
        }

        public RetryFlag RetryFlag { get; private set; }

        /// <summary>
        /// 出错重试
        /// </summary>
        /// <param name="flag">1-Abort,2-Retry,3-Ignore</param>
        public void Retry(RetryFlag flag)
        {
            RetryFlag = flag;
            _retryEvent.Set();
        }

        private int LoadTipByZ(int z)
        {
            int res = ResultCode.S_SUCCESSED;
            res = MoveToZ(z + LoadTipStep);
            _isActuralLoadTip = true;
            if (res == ResultCode.S_SUCCESSED)
            {
                res = MoveToSafeZ();
                if (res == ResultCode.S_SUCCESSED)
                {
                    res = MotorZReset();
                }
            }
            return res;
        }

        /// <summary>
        /// 加载通道Tip头
        /// </summary>
        /// <param name="isReportTipState">是否报告Tip头状态</param>
        /// <param name="loc">位置信息</param>
        /// <param name="tipType">tip头类型</param>
        /// <param name="ch">通道</param>
        /// <returns></returns>
        private int LoadTipInChannel(bool isReportTipState, Location loc, int tipType, Channels ch)
        {
            int res = CheckDeviceState(ResultCode.S_SUCCESSED, ch);
            if (res != ResultCode.S_SUCCESSED)
                return res;
            //包含检测Tip状态
            CurrentErrCode = ResultCode.S_SUCCESSED;
            var tipExistErrCodes = new List<int>() { ResultCode.ERR_TIP_EXIST };
            var retryErrCodes = new List<int>() { ResultCode.ERR_TIP_NONE_EXIST };
            if (isReportTipState)//tip头状态检测
            {
                res = ReportTipState(false);//tip头状态
                if (tipExistErrCodes.Any(c => res % 100 == c))//tip头存在
                {
                    res = MotorZReset();//Z轴初始化
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        res = EjectTip(loc, ch);//打掉tip头
                        if (res == ResultCode.S_SUCCESSED)
                        {
                            res = LoadTipInChannel(false, loc, tipType, ch);//重新加载tip头
                        }
                    }
                }
                else//不存在
                {
                    res = LoadTipInChannel(false, loc, tipType, ch);
                }

                if (res == ResultCode.S_SUCCESSED)
                {
                    res = ReportTipState(true);
                    res = ReLoadTipInChannel(res, ch, loc, tipType);//重新加载TIP头
                }
            }
            else//不做检测，加载TIP头
            {
                res = MoveTo(loc, ch);
                if (res == ResultCode.S_SUCCESSED)
                {
                    res = SetLoadTipFlag();
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        res = LoadTipByZ(loc.Z);
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// 加载tip头
        /// </summary>
        /// <param name="isReportTipState"></param>
        /// <param name="loc">tip位置</param>
        /// <param name="tipBoxId"></param>
        /// <param name="tipType">tip头类型</param>
        /// <param name="ch">通道</param>
        /// <returns></returns>
        private int LoadTip(bool isReportTipState, Location loc, ref int tipBoxId, int tipType, Channels ch = Channels.Tip)
        {
            int res = CheckDeviceState(ResultCode.S_SUCCESSED, ch);
            if (res != ResultCode.S_SUCCESSED)
                return res;
            //包含检测Tip状态
            CurrentErrCode = ResultCode.S_SUCCESSED;
            var tipExistErrCodes = new List<int>() { ResultCode.ERR_TIP_EXIST };
            var retryErrCodes = new List<int>() { ResultCode.ERR_TIP_NONE_EXIST };
            if (isReportTipState)
            {
                res = ReportTipState(false);
                if (tipExistErrCodes.Any(c => res % 100 == c))
                {
                    res = MotorZReset();
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        res = EjectTip(loc, ch);
                        if (res == ResultCode.S_SUCCESSED)
                        {
                            res = LoadTip(false, loc, ref tipBoxId, tipType, ch);
                        }
                    }
                }
                else
                {
                    res = LoadTip(false, loc, ref tipBoxId, tipType, ch);
                }

                if (res == ResultCode.S_SUCCESSED)
                {
                    res = ReportTipState(true);
                    res = ReLoadTipInProcess(res, ref tipBoxId, tipType);
                }
            }
            else
            {
                res = MoveTo(loc, ch);//移动到tip头位置
                if (res == ResultCode.S_SUCCESSED)
                {
                    res = SetLoadTipFlag();
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        res = LoadTipByZ(loc.Z);//移动Z轴加载tip头
                    }
                }
            }
            return res;
        }

        private int _reLoadTimes = 0;

        private int ReLoadTipInProcess(int resCode, ref int tipBoxId, int tipType)
        {
            int res = resCode;
            bool isAutoRetry = false;
            if (res != ResultCode.S_SUCCESSED && res != ResultCode.S_STOP)
            {
                _reLoadTimes++;
                if (OnRetryCommand != null)
                {
                    //取Tip头失败后添加自动重试一次
                    if (_reLoadTimes > 1)
                    {
                        OnRetryCommand(RetryModule.LiqHa, res, true);
                        _retryEvent.WaitOne();
                    }
                    else
                    {
                        RetryFlag = RetryFlag.Retry;//自动重试一次
                        isAutoRetry = true;
                    }
                    if (RetryFlag == RetryFlag.Retry)//重试
                    {
                        int nextTipIndex = -1;
                        for (int i = TipManager[tipBoxId].CurrentTipIndex; i < 96; i++)
                        {
                            if (TipManager[tipBoxId][i].State)
                            {
                                nextTipIndex = i;
                                break;
                            }
                        }
                        if (nextTipIndex != TipManager[tipBoxId].CurrentTipIndex)
                        {
                            if (nextTipIndex >= 0)
                            {
                                TipManager[tipBoxId].CurrentTipIndex = nextTipIndex;
                            }
                            else
                            {
                                TipManager[tipBoxId].CurrentTipIndex = 0;
                                TipManager[tipBoxId].Enabled = false;
                                tipBoxId += 1;
                            }
                        }

                        res = MotorYReset();
                        if (res == ResultCode.S_SUCCESSED)
                        {
                            res = LoadTip(ref tipBoxId, tipType);
                            if (!isAutoRetry)
                            {
                                res = ReLoadTipInProcess(res, ref tipBoxId, tipType);
                            }
                        }
                    }
                    else if (RetryFlag == RetryFlag.Ignore)//忽略
                    {
                        res = ResultCode.S_SUCCESSED;
                    }
                }
            }
            else
            {
                _reLoadTimes = 0;

            }
            return res;
        }

        private int ReLoadTipInChannel(int resCode, Channels ch, Location loc, int tipType)
        {
            int res = resCode;
            bool isAutoRetry = false;
            if (res != ResultCode.S_SUCCESSED && res != ResultCode.S_STOP)
            {
                _reLoadTimes++;
                if (OnRetryCommand != null)
                {
                    //取Tip头失败后添加自动重试一次
                    if (_reLoadTimes > 1)
                    {
                        OnRetryCommand(RetryModule.LiqHa, res, true);
                        _retryEvent.WaitOne();
                    }
                    else
                    {
                        RetryFlag = RetryFlag.Retry;//自动重试一次
                        isAutoRetry = true;
                    }
                    if (RetryFlag == RetryFlag.Retry)//重试
                    {

                        res = MotorYReset();
                        if (res == ResultCode.S_SUCCESSED)
                        {
                            res = LoadTipInChannel(ch, loc, tipType);
                            if (!isAutoRetry)
                            {
                                res = ReLoadTipInChannel(res, ch, loc, tipType);
                            }
                        }
                    }
                    else if (RetryFlag == RetryFlag.Ignore)//忽略
                    {
                        res = ResultCode.S_SUCCESSED;
                    }
                }
            }
            else
            {
                _reLoadTimes = 0;
            }
            return res;
        }

        private int MotorYReset()
        {
            return GetResultCode(Common.CODE_Y, MotorY.Initialize());
        }

        private List<TipLoc> tmpTipLocs = new List<TipLoc>();

        /// <summary>
        /// 获取tip头位置
        /// </summary>
        /// <param name="tipBoxId"></param>
        /// <param name="tipType"></param>
        /// <returns></returns>
        private int GetTipLoc(ref int tipBoxId, int tipType = 0)
        {
            var res = ResultCode.S_SUCCESSED;
            tmpTipLocs.Clear();

            if (tipBoxId >= TipManager.Count)
            {
                var tipBox = TipManager.FirstOrDefault(t => t.TipType == tipType);
                if (tipBox != null)
                {
                    tipBoxId = tipBox.Index;
                }
                else
                {
                    tipBoxId = 0;
                }
            }

            if (TipManager[tipBoxId].Enabled && TipManager[tipBoxId].TipType == tipType)
            {
                var curTipIndex = TipManager[tipBoxId].CurrentTipIndex;
                var tip = TipManager[tipBoxId].FirstOrDefault(t => t.State && t.Index >= curTipIndex);
                if (tip != null)
                {
                    tmpTipLocs.Add(new TipLoc()
                    { Idx = tip.Index, TipBoxIdx = tipBoxId, LayoutIdx = TipManager[tipBoxId].LayoutIdx });
                }
            }

            if (tmpTipLocs.Count == 0)
            {
                var tipBox = TipManager.FirstOrDefault(t => t.Enabled && t.TipType == tipType);
                if (tipBox != null)
                {
                    var tip = tipBox.FirstOrDefault(t => t.State);
                    if (tip != null)
                    {
                        tmpTipLocs.Add(new TipLoc()
                        { Idx = tip.Index, TipBoxIdx = tipBox.Index, LayoutIdx = tipBox.LayoutIdx });
                        tipBoxId = tipBox.Index;
                    }
                }
            }

            if (tmpTipLocs.Count == 0)
            {
                var id = TipManager.FindIndex(t => t.TipType == tipType);
                if (id < 0)
                    id = tipType;
                TipManager[id].Enabled = true;
                TipManager.SetTipBoxState(id, 0, 95, true);
                if (OnNotifyLoadNewTip != null)
                {
                    OnNotifyLoadNewTip(id, tipType);
                    _waitLoadFinished.WaitOne();
                }

                res = GetTipLoc(ref tipBoxId, tipType);
            }

            return res;
        }



        private Location GetLoc(string locName)
        {
            Location loc = null;
            loc = GetSpecLoc?.Invoke(locName, (int)Channels.Tip, 0);
            return loc ?? new Location() { Name = locName };
        }

        private int _currentTipBoxId = 0;
        public int CurrentTipBoxId
        {
            get
            {
                return _currentTipBoxId;
            }
            set
            {
                if (value >= 0 && value < 2)
                {
                    _currentTipBoxId = value;
                }
            }
        }

        private Location GetTipLoc(int tipId)
        {
            var tipBoxA1 = Common.EncodeLocaName(LocationName.TipBoxName, "A1");
            var locTipBoxA1 = GetLoc(tipBoxA1);
            var tipBoxA12 = Common.EncodeLocaName(LocationName.TipBoxName, "A12");
            var locTipBoxA12 = GetLoc(tipBoxA12);
            var tipBoxH12 = Common.EncodeLocaName(LocationName.TipBoxName, "H12");
            var locTipBoxH12 = GetLoc(tipBoxH12);
            var locTipBox = new Location();
            var locInterval = new Location();
            //locTipBox.X = (locTipBoxA1.X + locTipBoxA12.X) / 2;
            //locTipBox.Y = locTipBoxA1.Y;
            //locTipBox.Z = (locTipBoxA1.Z + locTipBoxA12.Z + locTipBoxH12.Z) / 3;
            //locInterval.X = (locTipBoxH12.X - locTipBox.X) / 7;
            //locInterval.Y = (locTipBoxH12.Y - locTipBox.Y) / 11;
            //var col = 0;
            //var row = 0;
            //Common.GetTipPos(tipId, ref row, ref col);
            locTipBox.X = locTipBoxA1.X;
            locTipBox.Y = locTipBoxA1.Y;
            locTipBox.Z = (locTipBoxA1.Z + locTipBoxA12.Z + locTipBoxH12.Z) / 3;
            locInterval.X = ((locTipBoxH12.X + locTipBoxA12.X) / 2 - locTipBox.X) / 11;
            locInterval.Y = (locTipBoxH12.Y - (locTipBox.Y + locTipBoxA12.Y) / 2) / 7;
            var col = 0;
            var row = 0;
            Common.GetTipPos(tipId, ref row, ref col);

            var loc = new Location();
            loc.Z = locTipBox.Z;
            loc.X = locTipBox.X + col * locInterval.X;
            loc.Y = locTipBox.Y + row * locInterval.Y;
            return loc;
        }

        private int _oldTipType = -1;

        /// <summary>
        /// 加载tip头
        /// </summary>
        /// <param name="tipBoxId"></param>
        /// <param name="tipType"></param>
        /// <returns></returns>
        public int LoadTip(ref int tipBoxId, int tipType = 0)
        {
            _curTipType = tipType;
            var res = GetTipLoc(ref tipBoxId, tipType);

            if (res == ResultCode.S_SUCCESSED)
            {
                var tipLoc = tmpTipLocs[0];
                //var tipBoxName = Common.EncodeLocaName(LocationName.TipBoxName, tipLoc.TipBoxIdx);
                //var locTipBox = GetLoc(tipBoxName);
                //var tipIntervalName = Common.EncodeLocaName(LocationName.TipBoxInterval, tipLoc.TipBoxIdx);
                //var locInterval = GetLoc(tipIntervalName);
                //var col = 0;
                //var row = 0;
                //Common.GetTipPos(tipLoc.Idx, ref row, ref col);

                //var loc = new Location();
                //loc.Z = locTipBox.Z;
                //loc.X = locTipBox.X + row * locInterval.X;
                //loc.Y = locTipBox.Y - col * locInterval.Y;
                var loc = GetTipLoc(tipLoc.Idx);//获取tip头位置
                _isActuralLoadTip = false;
                TipManager[tipLoc.TipBoxIdx][tipLoc.Idx].State = false;
                res = LoadTip(true, loc, ref tipBoxId, tipType);//加载tip头
                if (_isActuralLoadTip)
                {
                    TipUsedAct?.Invoke(tipLoc.TipBoxIdx, tipLoc.Idx);
                    int nextTipIndex = -1;
                    for (int i = TipManager[tipBoxId].CurrentTipIndex; i < 96; i++)
                    {
                        if (TipManager[tipBoxId][i].State)
                        {
                            nextTipIndex = i;
                            break;
                        }
                    }
                    if (nextTipIndex != TipManager[tipBoxId].CurrentTipIndex)
                    {
                        if (nextTipIndex >= 0)
                        {
                            TipManager[tipBoxId].CurrentTipIndex = nextTipIndex;
                        }
                        else
                        {
                            TipManager[tipBoxId].CurrentTipIndex = 0;
                            //只有一盒Tip，用完重新从0开始
                            TipManager[tipBoxId].Enabled = false;
                            //tipBoxId += 1;
                        }
                    }

                    TipCurrentState?.Invoke(tipBoxId, TipManager[tipBoxId].CurrentTipIndex);
                    if (_oldTipType != _curTipType)
                    {
                        OnTipTypeChanged?.Invoke(_curTipType);
                        _oldTipType = _curTipType;
                    }
                }
            }
            _currentTipBoxId = tipBoxId;
            return res;
        }

        public int LoadTipInChannel(Channels ch, Location loc, int tipType = 0)
        {
            return LoadTipInChannel(true, loc, tipType, ch);
        }

        /// <summary>
        /// 加载通道Tip头
        /// </summary>
        /// <param name="ch">通道名</param>
        /// <param name="locName">位置名称</param>
        /// <param name="subId"></param>
        /// <param name="tipType">tip类型</param>
        /// <returns></returns>
        public int LoadTipInChannel(int ch, string locName, int subId, int tipType = 0)
        {
            var loc = GetSpecLoc(locName, ch, subId);//获取位置信息
            return LoadTipInChannel(true, loc, tipType, (Channels)ch);
        }

        private int SetLoadTipFlag()
        {
            return GetResultCode(Common.CODE_Z, MotorZ.SetSlowSpeedFlag());
        }

        public delegate void DelegateNotifyLoadNewTip(int layout, int tipType);
        public DelegateNotifyLoadNewTip OnNotifyLoadNewTip = null;

        private AutoResetEvent _waitLoadFinished = new AutoResetEvent(false);

        public void NotifyLoadNewTipFinished()
        {
            _waitLoadFinished.Set();
        }

        /// <summary>
        /// 排掉Tip头
        /// </summary>
        /// <param name="locName">tip头名称</param>
        /// <param name="ch">通道</param>
        /// <param name="subId">样本Id</param>
        /// <returns></returns>
        public int EjectTip(string locName, int ch, int subId)
        {
            Location loc = GetSpecLoc(locName, ch, subId);
            return EjectTip(loc, (Channels)ch);
        }

        /// <summary>
        /// 排掉Tip头
        /// </summary>
        /// <param name="loc">tip头名称</param>
        /// <param name="ch">通道</param>
        /// <returns></returns>
        public int EjectTip(Location loc, Channels ch)
        {
            var res = ResultCode.S_SUCCESSED;
            res = MoveTo(loc, ch);
            if (res == ResultCode.S_SUCCESSED)
            {
                res = GetResultCode(Common.CODE_ADP, Adp.EjectTip('0'));
                if (res == ResultCode.S_SUCCESSED)
                {
                    res = MoveToSafeZ();
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        res = MotorZReset();
                        if (res == ResultCode.S_SUCCESSED)
                        {
                            res = ReportTipState(false);
                        }
                    }
                }
            }
            if (res != ResultCode.S_SUCCESSED && res != ResultCode.S_STOP)
            {

                if (OnRetryCommand != null)
                {
                    OnRetryCommand(RetryModule.LiqHa, res, true);
                    _retryEvent.WaitOne();
                    if (RetryFlag == RetryFlag.Retry)//重试
                    {
                        res = EjectTip(loc, ch);
                    }
                    else if (RetryFlag == RetryFlag.Ignore)//忽略
                    {
                        res = MoveToSafeZ();
                        if (res == ResultCode.S_SUCCESSED)
                        {
                            res = MotorZReset();
                        }
                        res = ResultCode.S_SUCCESSED;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// 排掉Tip头
        /// </summary>
        /// <returns></returns>
        public int EjectTip()
        {
            var ejectLocaName = LocationName.EjectTipLoc;
            var loc = GetLoc(ejectLocaName);
            return EjectTip(loc, Channels.Tip);
        }

        public int GetStep(double millimeter)
        {
            return (int)(millimeter * 3200 / 38.1);
        }

        /// <summary>
        /// 设置检测深度
        /// </summary>
        /// <param name="aParam"></param>
        /// <returns></returns>
        private int SetDetectDepth(LiqHaParameter aParam)
        {
            int res = ResultCode.S_SUCCESSED;
            var liquidCan = aParam.LiqContainer;
            int step = GetStep(liquidCan.Height - liquidCan.ExcessVolHeight +
                       liquidCan.HeightDiff) + DetectOffsetStep;
            res = GetResultCode(Common.CODE_Z, MotorZ.SetDetectDepth(step));
            return res;
        }

        private bool WaitForBalance(int milliseconds)
        {
            AutoResetEvent waitEvent = new AutoResetEvent(false);
            return waitEvent.WaitOne(milliseconds);
        }

        public Func<int, int, AdpParameter, bool, int, AdpParameter> GetFreeDispense;

        private int _curTipType = 0;
        private bool _isLiquildEnough = true;


        /// <summary>
        /// 单次检测
        /// </summary>
        /// <param name="aParam"></param>
        /// <param name="curLiquidStep"></param>
        /// <returns></returns>
        private int SingleDetect(LiqHaParameter aParam, ref int curLiquidStep)
        {
            int res = ResultCode.S_SUCCESSED;
            int resCode = ResultCode.S_SUCCESSED;
            //var tmp = aParam.DisParam.Clone();
            var index = 0;
            var liqCon = aParam.LiqContainer;
            _isLiquildEnough = false;
            _log.Debug("Adp {0} detect liquid level in the container {1}", index, liqCon.EnglishName);
            aParam.DisParam = GetFreeDispense(_curTipType, index, aParam.DisParam, false, aParam.DisParam.Row);
            var tmp = aParam.DisParam.Clone();
            curLiquidStep = MotorZ.CurrentPostion - GetStep(liqCon.DetectLiquidLevelErr);
            int step = 0;//探测到液面需要移动的步数
            bool tipStates = false;
            res = Adp.ReportTipState(ref tipStates);//Tip头状态
            if (res != ResultCode.S_SUCCESSED)
            {
                resCode = GetResultCode(Common.CODE_ADP, res);
            }
            else
            {
                if (!tipStates)//Tip头检测，不存在Tip头
                {
                    resCode = GetResultCode(Common.CODE_ADP, ResultCode.ERR_TIP_LOST);
                }
                else
                {
                    int pressure = 0;
                    res = Adp.ReportPressure(ref pressure);
                    _log.Debug("The pressure of Adp {0} is {1} before detection", index, pressure);
                    //res = _adps[index].AbsMove(3000, 1);
                    res = MotorZ.LiquidDetection((byte)Adp.AdpInfo.NodeId, ref step);//探测液面
                    int relativeStep = step - MotorZ.CurrentPostion;//移动相对步数
                    MotorZ.CurrentPostion = step;
                    _log.Debug("The liquid level step is {0},{1:X},relative step of the top container is {2}",
                        MotorZ.CurrentPostion,
                        res,
                        relativeStep);

                    if (res != ResultCode.S_SUCCESSED)
                    {
                        resCode = GetResultCode(Common.CODE_Z, res);
                    }
                    else
                    {
                        var tmpIsLiquidEnough = true;
                        int tmpLiquildStep = step - GetStep(liqCon.DetectLiquidLevelErr);//????
                        int topStep = aParam.DesLocation.Z + DetectOffsetStep;

                        if (relativeStep < DetectOffsetStep && _isNeedWarningLevel)//tip头在液面以下？？？
                        {
                            res = MotorZ.Reset();
                            if (res != ResultCode.S_SUCCESSED)
                            {
                                resCode = GetResultCode(Common.CODE_Z, res);
                            }
                            else
                            {
                                byte mode = 1;
                                resCode = Adp.SetMaxSpeed(aParam.DisParam.DispenseSpeed, mode);
                                resCode = Adp.AbsMove(0, mode); //排空

                                res = ResultCode.ERR_SUSPICIOUS_LIQUIDLEVEL;
                                resCode = GetResultCode(Common.CODE_ADP, res);
                            }
                        }
                        else//???
                        {
                            res = MotorZ.RelativeMove(MotorConstant.DIR_N, LeaveLiquidStep);//探测到页面后，向上离开液面
                            if (res != ResultCode.S_SUCCESSED)
                            {
                                resCode = GetResultCode(Common.CODE_ADP, res);
                            }
                            else
                            {
                                byte mode = 1;
                                resCode = Adp.SetMaxSpeed(aParam.DisParam.DispenseSpeed, mode);
                                resCode = Adp.AbsMove(0, mode); //排空

                                var totalVol = aParam.DisParam.DisVolumeList.Sum();
                                if (totalVol > 0)//总移液需求量
                                {
                                    int totalAspStep = (int)Math.Ceiling(totalVol / liqCon.UnitStepVolume);
                                    int actualLiquildStep = GetStep(liqCon.Height +
                                                            liqCon.HeightDiff - liqCon.ExcessVolHeight) -
                                                            (tmpLiquildStep - topStep);//容器实际步数
                                    double actualTotalVol = actualLiquildStep * liqCon.UnitStepVolume;//容器实际容量
                                    tmpIsLiquidEnough = totalVol <=
                                                        actualTotalVol;
                                    _log.Debug("The Adp detect the actual total amount of liquid in the normal container is {0}, " +
                                        "the total demand volume is {1}, the liquid is {2}, the liquid level steps is {3}",
                                        actualTotalVol, totalVol,
                                        tmpIsLiquidEnough ? "Enough" : "Not Enough", tmpLiquildStep);

                                    if (IsIgnoreLiquildEnough)
                                    {
                                        tmpIsLiquidEnough = true;
                                    }

                                    _isLiquildEnough = tmpIsLiquidEnough;
                                    if (tmpIsLiquidEnough)
                                    {
                                        res = ResultCode.S_SUCCESSED;
                                        resCode = GetResultCode(Common.CODE_ADP, res);
                                    }
                                    else
                                    {
                                        res = MotorZ.Reset();
                                        if (res != ResultCode.S_SUCCESSED)
                                        {
                                            resCode = GetResultCode(Common.CODE_Z, res);
                                        }
                                        else
                                        {
                                            res = ResultCode.ERR_TOTAL_LIQUID_NOTENOUGH;
                                            resCode = GetResultCode(Common.CODE_ADP, res);
                                        }
                                    }
                                }
                            }
                        }
                        curLiquidStep = tmpLiquildStep;
                    }
                }
            }
            aParam.DisParam = tmp;
            return resCode;
        }

        private static int DelayBalance = 300;

        private int GetAdpPressure()
        {
            int pressure = 0;
            int res = Adp.ReportPressure(ref pressure);
            if (res == ResultCode.S_SUCCESSED)
            {
                Adp.PressureInitial = pressure;
                _log.Debug("The pressure before aspirate is {0}", pressure);
            }
            return res;
        }

        /// <summary>
        /// 配置吸液参数
        /// </summary>
        /// <param name="aParam"></param>
        /// <returns></returns>
        private int AspirePreCfg(LiqHaParameter aParam)
        {
            byte mode = 1;
            int resCode = ResultCode.S_SUCCESSED;
            int signCode = Common.CODE_ADP;
            var res = ResultCode.S_SUCCESSED;
            _log.Debug("Configuration parameters before aspirate");
            resCode = Adp.SetCutOffSpeed(aParam.DisParam.BreakOffSpeed, mode);
            resCode = Adp.SetMaxSpeed(aParam.DisParam.DispenseSpeed, mode);
            if (resCode == ResultCode.S_SUCCESSED)
            {
                signCode = Common.CODE_Z;
                resCode = MotorZ.RelativeMove(MotorConstant.DIR_P, DisEmptyStep);
                if (resCode == ResultCode.S_SUCCESSED)
                {
                    signCode = Common.CODE_ADP;
                    resCode = Adp.AbsMove(0, mode); //排空
                    if (resCode == ResultCode.S_SUCCESSED)
                    {
                        WaitForBalance(DelayBalance);
                        resCode = Adp.SetMaxSpeed(aParam.DisParam.AspiratingSpeed, mode);
                        if (resCode == ResultCode.S_SUCCESSED)
                        {
                            resCode = Adp.Aspirate(aParam.DisParam.LeadingAirgap, mode);
                        }
                        //吸完空气柱后再读取压力值
                        WaitForBalance(DelayBalance);
                        GetAdpPressure();

                    }
                }

            }
            res = GetResultCode(Common.CODE_ADP, resCode);
            return res;
        }

        /// <summary>
        /// 是否阻塞
        /// </summary>
        /// <param name="aspVol"></param>
        /// <param name="isBlock"></param>
        /// <returns></returns>
        private int CheckIsBlock(double aspVol, ref bool isBlock)
        {
            int res = ResultCode.S_SUCCESSED;
            isBlock = false;
            var tipParam = GetTipParam(_curTipType);//获取tip头参数
            if (tipParam != null)
            {
                if (tipParam.IsCheckBlock)
                {
                    int pressure = 0;
                    res = Adp.ReportPressure(ref pressure);
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        //double thresholdPrs = Slope * aspVol + Intercept;
                        double thresholdPrs = tipParam.Slope * aspVol + tipParam.Intercept; //每个Adp初始压力不一致
                        isBlock = Adp.PressureInitial - pressure > thresholdPrs;
                        //isBlock = true;
                        //if (index==2)
                        //{
                        //    isBlock = true;
                        //}
                        _log.Debug("The initial pressure before aspirate is {0}，after aspirate {1}ul, " +
                            "the pressure is {2}, calculate pressure is {3}," +
                            " judge result is {4}", Adp.PressureInitial, aspVol,
                            pressure, thresholdPrs, isBlock ? "Block" : "Not Block");

                        if (!isBlock && tipParam.IsCheckAspNull && aspVol >= 10)
                        {
                            int dValue = Math.Abs(pressure - Adp.PressureInitial);
                            if (dValue < tipParam.MinPressureDiff)
                            {
                                res = ResultCode.ERR_NONE_ASPIRATE;
                                _log.Debug("The initial pressure before aspirate is {0}，after aspirate {1}ul, the pressure is {2},，threshold value is {3}，judge as 'Aspirate empty'",
                                    Adp.PressureInitial, aspVol,
                                    pressure, tipParam.MinPressureDiff);
                            }
                        }
                    }
                }
            }
            else
            {
                _log.Debug("Can not find the block parameter of tip {0}", _curTipType);
            }
            return res;
        }

        /// <summary>
        /// 获取tip头参数
        /// </summary>
        /// <param name="curTipType"></param>
        /// <returns></returns>
        private TipParam GetTipParam(int curTipType)
        {
            TipParam tipP = null;
            var context = new DatabaseContext();
            
                tipP = context.TipParams.GetSingle(t => t.TipType == curTipType);
            

            if (tipP == null)
            {
                tipP = new TipParam() { TipType = curTipType };
            }
            return tipP;
        }

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="aParam">参数</param>
        /// <param name="isLiquidDetection"></param>
        /// <param name="aspirateStep"></param>
        /// <returns></returns>
        private int SingleAspirate(LiqHaParameter aParam, bool isLiquidDetection, ref int aspirateStep)
        {
            int res = ResultCode.S_SUCCESSED;
            AdpParameter tmpAdpParameter = aParam.DisParam.Clone();
            int index = 0;
            byte mode = 1;//0,增量，1微升
            var liqCon = aParam.LiqContainer;
            int resCode = ResultCode.S_SUCCESSED;
            int signCode = Common.CODE_Z;
            _log.Debug("Adp当前Z轴步数为{0}", MotorZ.CurrentPostion);

            var bakList = new List<double>();
            bakList.AddRange(aParam.DisParam.DisVolumeList);
            aParam.DisParam = GetFreeDispense(_curTipType, index, aParam.DisParam, true, aParam.DisParam.Row);
            aParam.DisParam.BakDisVolumeList.Clear();
            aParam.DisParam.BakDisVolumeList.AddRange(bakList);

            double aspVoulme = 0;//吸液体积
            for (int i = 0; i < aParam.DisParam.DisVolumeList.Count; i++)
            {
                aspVoulme += aParam.DisParam.DisVolumeList[i];
            }

            aspVoulme += aParam.DisParam.ConditioningVolume;

            if (aParam.DisParam.DisVolumeList.Count > 0)
            {
                aspVoulme += (aParam.DisParam.DisVolumeList[0] * aParam.DisParam.ExcessVolume);
            }

            aspVoulme += aParam.DisParam.ExtraVol;
            aspVoulme += aParam.DisParam.LeftVolume;
            int aspStep = (int)Math.Ceiling(aspVoulme / liqCon.UnitStepVolume);
            var tmpLiquidStep = aspirateStep;
            if (_isLiquildEnough)
            {
                #region

                if (resCode == ResultCode.S_SUCCESSED)
                {
                    resCode = AspirePreCfg(aParam);
                    if (resCode != ResultCode.S_SUCCESSED)
                    {
                        _log.Information("Failed to configuration parameters before aspirate {0}", resCode);
                        res = resCode;
                    }
                    else
                    {
                        bool isAspInDepth = false;
                        var tmpAspDepthStep = GetStep(aParam.AspirateDepth);//获取移液步数
                        if (tmpAspDepthStep == 0)
                        {
                            //Z轴分多次下降吸液
                            int aspTimes =
                                (int)Math.Ceiling(aspVoulme * 1.0 / (liqCon.UnitStepVolume * AspirateStep));
                            int singleAspStep = 0;
                            double singleAspVol = 0;
                            //int curStep = tmpLiquidStep;
                            int curStep = tmpLiquidStep + liqCon.OverLiquidLevel; //需多下降固定步数
                            var maxAspStep =
                                aParam.DesLocation.Z + GetStep(liqCon.Height - liqCon.ExcessVolHeight); //???
                            for (int k = 0; k < aspTimes; k++)
                            {
                                if (k < aspTimes - 1)
                                {
                                    singleAspStep = AspirateStep;
                                    singleAspVol = liqCon.UnitStepVolume * singleAspStep;
                                }
                                else
                                {
                                    singleAspStep = aspStep - (aspTimes - 1) * singleAspStep;
                                    singleAspVol = aspVoulme - singleAspVol * (aspTimes - 1);
                                }

                                curStep += singleAspStep;
                                if (resCode == ResultCode.S_SUCCESSED)
                                {
                                    var tmpAspStep = curStep;
                                    if (tmpAspStep > maxAspStep)
                                    {
                                        tmpAspStep = maxAspStep;
                                        _log.Debug("吸液位置计算值为{0}修改为最大吸液位置值{1}", curStep, maxAspStep);
                                    }

                                    signCode = Common.CODE_Z;
                                    resCode = MotorZ.AbsMove(tmpAspStep);
                                    if (resCode == ResultCode.S_SUCCESSED)
                                    {
                                        signCode = Common.CODE_ADP;
                                        resCode = Adp.Aspirate(Math.Round(singleAspVol, 3), mode); //吸液量最多能保持3位小数
                                        WaitForBalance(aParam.DisParam.DelayAsp);
                                        _log.Debug("Adp{0}吸液位置为{1}实际吸液量为{2}ul", index, tmpAspStep,
                                            singleAspVol);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                            }
                        }
                        else
                        {
                            isAspInDepth = true;
                            signCode = Common.CODE_Z;
                            int aspLocStep =
                                aParam.DesLocation.Z + tmpAspDepthStep + GetStep(liqCon.HeightDiff); //???
                            _log.Debug("Adp固定吸液位置为{0}", aspLocStep);
                            resCode = MotorZ.AbsMove(aspLocStep);
                            if (resCode == ResultCode.S_SUCCESSED)
                            {
                                signCode = Common.CODE_ADP;
                                resCode = Adp.Aspirate(Math.Round(aspVoulme, 3), mode);
                                WaitForBalance(aParam.DisParam.DelayAsp);

                                _log.Debug("Adp在固定位置{0}实际吸液量为{1} uL, Ret={2:X}", aspLocStep, aspVoulme,
                                    resCode);
                            }
                        }

                        if (resCode == ResultCode.S_SUCCESSED) //判堵塞
                        {
                            bool isBlock = false;
                            aspirateStep += aspStep;
                            _log.Debug("Adp当前液面步数为{0}，吸液量为{1} uL，吸液步数为{2},瓶口位置步数{3}",
                                tmpLiquidStep,
                                aspVoulme,
                                aspStep,
                                aParam.DesLocation.Z);

                            signCode = Common.CODE_ADP;
                            resCode = CheckIsBlock(aspVoulme, ref isBlock);
                            if (resCode == ResultCode.S_SUCCESSED)
                            {
                                int tmpLeaveLiquildStep = aspStep - 200;//留液量
                                if (tmpLeaveLiquildStep < 0)
                                {
                                    tmpLeaveLiquildStep = 0;
                                }

                                if (!isBlock)//未阻塞
                                {
                                    signCode = Common.CODE_Z;
                                    //resCode = ZUpAfterAsiprate(index, aspStep);
                                    var tmpCurrentStep = MotorZ.CurrentPostion;
                                    if (tmpLeaveLiquildStep > 0)
                                    {
                                        //resCode = MotorZ.SetLowSpeedFlag();
                                        resCode =
                                            MotorZ.AbsMove(tmpLeaveLiquildStep);
                                        _log.Debug("Adpmove up to {0},relative step is {1}",
                                            tmpLeaveLiquildStep,
                                            tmpCurrentStep - tmpLeaveLiquildStep);
                                    }

                                    if (resCode == ResultCode.S_SUCCESSED)
                                    {
                                        if (aParam.DisParam.ExtraVol > 0)//回吐量
                                        {
                                            signCode = Common.CODE_ADP;
                                            Adp.SetMaxSpeed(aParam.DisParam.DispenseSpeed, mode);
                                            resCode = Adp.Dispense(aParam.DisParam.ExtraVol, mode);
                                        }

                                        if (resCode == ResultCode.S_SUCCESSED)
                                        {
                                            signCode = Common.CODE_Z;
                                            resCode = MotorZ.AbsMove(RunningHeight);
                                            if (resCode == ResultCode.S_SUCCESSED)
                                            {
                                                Adp.SetMaxSpeed(aParam.DisParam.AspiratingSpeed, mode);
                                                resCode = Adp.Aspirate(aParam.DisParam.TrailingAirgap, mode);//吸空气柱
                                            }
                                        }
                                    }
                                }
                                else//阻塞
                                {
                                    if (tmpLeaveLiquildStep > 0)
                                    {
                                        signCode = Common.CODE_Z;
                                        //resCode = _motorZ[index].SetLowSpeedFlag();
                                        resCode =
                                            MotorZ.AbsMove(tmpLeaveLiquildStep);
                                    }

                                    if (resCode == ResultCode.S_SUCCESSED)
                                    {
                                        signCode = Common.CODE_ADP;
                                        resCode = Adp.SetMaxSpeed(aParam.DisParam.DispenseSpeed, mode);
                                        if (resCode == ResultCode.S_SUCCESSED)
                                        {
                                            resCode = Adp.AbsMove(0, mode);
                                            if (resCode == ResultCode.S_SUCCESSED)
                                            {
                                                signCode = Common.CODE_Z;
                                                resCode = MotorZ.AbsMove(RunningHeight);
                                                if (resCode == ResultCode.S_SUCCESSED)
                                                {
                                                    signCode = Common.CODE_ADP;
                                                    resCode = Adp.Aspirate(Math.Round(aspVoulme
                                                            - aParam.DisParam.ExtraVol
                                                            + aParam.DisParam
                                                                .TrailingAirgap, 3),
                                                        mode);
                                                    resCode = ResultCode.ERR_LIQUID_BLOCKING;

                                                }
                                            }
                                        }
                                    }
                                }

                                res = GetResultCode(signCode, resCode);
                            }
                            else if (resCode == ResultCode.ERR_NONE_ASPIRATE) //空吸
                            {
                                MotorZ.AbsMove(RunningHeight);
                                signCode = Common.CODE_ADP;
                                resCode = ResultCode.ERR_NONE_ASPIRATE;
                                res = GetResultCode(signCode, resCode);
                            }
                            else
                            {
                                MotorZ.Reset();
                                signCode = Common.CODE_ADP;
                                //resCode = ResultCode.ERR_SUSPICIOUS_LIQUIDLEVEL;
                                res = GetResultCode(signCode, resCode);
                            }

                        }
                        else
                        {
                            MotorZ.AbsMove(aParam.DesLocation.Z + DisEmptyStep);
                            Adp.AbsMove(0, mode);
                            Adp.Aspirate(Math.Round(aspVoulme
                                                    - aParam.DisParam.ExtraVol
                                                    + aParam.DisParam.TrailingAirgap, 3),
                                mode);
                            MotorZ.Reset();
                            res = GetResultCode(signCode, resCode);
                        }
                    }
                }

                #endregion
            }
            else
            {
                resCode = MotorZ.AbsMove(RunningHeight);
                if (resCode == ResultCode.S_SUCCESSED)
                {
                    resCode = Adp.Aspirate(aspVoulme
                                           - aParam.DisParam.ExtraVol
                                           + aParam.DisParam.TrailingAirgap, mode);
                    if (resCode == ResultCode.S_SUCCESSED)
                    {
                        resCode = ResultCode.ERR_TOTAL_LIQUID_NOTENOUGH;
                        signCode = Common.CODE_ADP;
                        res = GetResultCode(signCode, resCode);
                    }
                    else
                    {
                        res = GetResultCode(Common.CODE_ADP, resCode);
                    }
                }
                else
                {
                    res = GetResultCode(Common.CODE_Z, resCode);
                }
            }

            //if(res!=ResultCode.S_SUCCESSED)
            //{
            //    pParam.DisParam = tmpAdpParameter;
            //}
            return res;
        }

        private int _curLiquidLevelStep = 0;

        /// <summary>
        /// 检测，吸液
        /// </summary>
        /// <param name="isLiquidDetection">是否检测液体</param>
        /// <param name="aParam">液体参数</param>
        /// <returns></returns>
        private int DetectAndAspirate(bool isLiquidDetection, LiqHaParameter aParam)
        {
            var retryErrCodes = new List<int>()
            {
                ResultCode.ERR_TOTAL_LIQUID_NOTENOUGH,
                ResultCode.ERR_LIQUID_BLOCKING,
                ResultCode.ERR_SUSPICIOUS_LIQUIDLEVEL,
                ResultCode.ERR_NONE_ASPIRATE,
                ResultCode.ERR_ADP_INVALID_COMMAND,
                ResultCode.ERR_ADP_INVALID_OPERAND,
                ResultCode.ERR_ADP_OVER_PRESSURE,
                ResultCode.ERR_ADP_LIQUID_LEVEL_DETECT_FAILURE,
                ResultCode.ERR_ADP_PLUNGER_OVERLOAD,
                ResultCode.ERR_ADP_TIP_LOST_OR_NOT_PRESENT,
                ResultCode.ERR_ADP_COMMAND_BUFFER_EMPTY_OR_EXECUTED_OR_NOT_READY_FOR_REPEAT,
                ResultCode.ERR_ADP_COMMAND_BUFFER_OVERFLOW,
                ResultCode.ERR_DETECT_FAILURE,
                ResultCode.ERR_TIP_LOST,
            };
            int res = CheckDeviceState(ResultCode.S_SUCCESSED, aParam.Channel);
            ;
            if (res != ResultCode.S_SUCCESSED)
                return res;
            int tmpZ = aParam.DesLocation.Z;
            if (isLiquidDetection)
            {
                var z = aParam.DesLocation.Z - DetectOffsetStep;
                if (z < 0)
                {
                    z = 0;
                }

                aParam.DesLocation.Z = z;
            }

            res = MoveTo(aParam.DesLocation, aParam.Channel);//移动到加样孔位置

            if (res != ResultCode.S_SUCCESSED) return res;

            _log.Debug("Do SingleDetectAndAspirate.");

            if (isLiquidDetection)//检测液体
            {
                res = SetDetectDepth(aParam);//移动到检测深度
                if (res == ResultCode.S_SUCCESSED)
                {
                    var initPos = 3000;
                    var context = new DatabaseContext();
                    
                        var tipInfo = context.TipInfos.GetSingle(t => t.TipType == _curTipType);
                        if (tipInfo != null)
                        {
                            initPos = tipInfo.AbsPos;
                        }

                    
                    res = GetResultCode(Common.CODE_ADP, Adp.AbsMove(initPos, 0));//将ADP活塞移至指定绝对位置，排空液体
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        WaitForBalance(DelayBalance);
                        res = SingleDetect(aParam, ref _curLiquidLevelStep);//检测
                    }
                }

                aParam.DesLocation.Z = tmpZ;

            }
            else
            {
                _isLiquildEnough = true;
            }

            if (res == ResultCode.S_SUCCESSED)
            {
                res = SingleAspirate(aParam, isLiquidDetection, ref _curLiquidLevelStep);//吸液
            }

            if (res != ResultCode.S_SUCCESSED && retryErrCodes.Contains(res % 100))
            {

                CurrentErrCode = res;
                res = MotorZReset();//重置Z轴
                if (res == ResultCode.S_SUCCESSED)
                {
                    if (DoRetry(CurrentErrCode, true))
                    {
                        res = CurrentErrCode;
                        if (RetryFlag == RetryFlag.Retry) //重试
                        {
                            aParam.DisParam.DisVolumeList.Clear();
                            aParam.DisParam.DisVolumeList.AddRange(aParam.DisParam.BakDisVolumeList);
                            res = EjectTip();
                            if (res == ResultCode.S_SUCCESSED)
                            {
                                res = InitAdp();
                                if (res == ResultCode.S_SUCCESSED)
                                {
                                    res = LoadTip(ref _currentTipBoxId, _curTipType);
                                    if (res == ResultCode.S_SUCCESSED)
                                    {
                                        res = DetectAndAspirate(isLiquidDetection, aParam);
                                    }
                                }
                            }
                        }
                        else if (RetryFlag == RetryFlag.Ignore) //忽略
                        {
                            res = ResultCode.S_SUCCESSED;
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="isLiquidDetection">是否检测液体</param>
        /// <param name="aParam"></param>
        /// <returns></returns>
        public int Aspirate(bool isLiquidDetection, LiqHaParameter aParam)
        {
            var res = DetectAndAspirate(isLiquidDetection, aParam);

            return res;
        }

        private const int BalanceDelay = 300;

        /// <summary>
        /// 混匀处理
        /// </summary>
        /// <param name="liqHaParam"></param>
        /// <returns></returns>
        public int Mix(LiqHaParameter liqHaParam)
        {
            var res = ResultCode.S_SUCCESSED;
            var resCode = ResultCode.S_SUCCESSED;
            byte mode = 1;
            MixParameter mixParam = liqHaParam.MixParameter;
            var desZ = liqHaParam.DesLocation.Z;
            var aspStep = GetStep(mixParam.AspirateDepth);//吸液移动步数
            var disStep = GetStep(mixParam.DispenseDepth);//排液移动步数
            var signCode = Common.CODE_Z;
            for (int i = 0; i < mixParam.Times; i++)
            {
                if (resCode == ResultCode.S_SUCCESSED)
                {
                    resCode = MotorZ.AbsMove(desZ + aspStep);//移动Z轴到吸液位置
                    if (resCode == ResultCode.S_SUCCESSED)
                    {
                        signCode = Common.CODE_ADP;
                        resCode = Adp.Aspirate(mixParam.Volume, mode);//吸液
                        if (resCode == ResultCode.S_SUCCESSED)
                        {
                            signCode = Common.CODE_Z;
                            resCode = MotorZ.AbsMove(desZ + disStep);//向上移动Z轴到排液位置
                            if (resCode == ResultCode.S_SUCCESSED)
                            {
                                signCode = Common.CODE_ADP;
                                resCode = Adp.AbsMove(0, mode);//排空液体
                                WaitForBalance(BalanceDelay);
                            }
                        }
                    }
                }
            }
            res = GetResultCode(signCode, resCode);
            return res;
        }

        /// <summary>
        /// 单次分配
        /// </summary>
        /// <param name="aParam">液体参数</param>
        /// <returns></returns>
        private int SingleDispense(LiqHaParameter aParam)
        {
            int res = ResultCode.S_SUCCESSED;
            int signCode = Common.CODE_ADP;

            AutoResetEvent waitEvent = new AutoResetEvent(false);
            byte mode = 1;
            _log.Debug("Number {0} dispense volume is {1}", aParam.DisParam.CurrentDisTime,
                aParam.DisParam.DisVolume);
            int resCode = Adp.SetMaxSpeed(aParam.DisParam.DispenseSpeed, mode);
            if (resCode == ResultCode.S_SUCCESSED)
            {
                resCode = Adp.SetCutOffSpeed(aParam.DisParam.BreakOffSpeed, mode);//设置断流速度
                if (resCode == ResultCode.S_SUCCESSED)
                {
                    ////Z轴下移步数
                    //if (_dispenseDepthStep > 0)
                    //{
                    //    signCode = Common.CODE_Z;
                    //    resCode = MotorZ
                    //        .RelativeMove(MotorConstant.DIR_P, _dispenseDepthStep); //DisInnerLiquidStep
                    //}

                    if (resCode == ResultCode.S_SUCCESSED)
                    {
                        signCode = Common.CODE_ADP;
                        if (aParam.DisParam.DisTimes == 1)
                        {
                            if (aParam.DisParam.ExtraVol == 0 && Math.Abs(aParam.DisParam.LeftVolume) < Double.Epsilon)
                            {
                                resCode = Adp.AbsMove(0, mode); //排空
                                _log.Debug("ADP dispense with command A {0:X2}", resCode);
                            }
                            else
                            {
                                var disVolume = aParam.DisParam.DisVolume;
                                disVolume += aParam.DisParam.TrailingAirgap;
                                resCode = Adp.Dispense(disVolume, mode);
                                _log.Debug("ADP dispense with command D {0:X2}", resCode);
                            }

                            waitEvent.WaitOne(aParam.DisParam.DelayDis);
                            if (aParam.MixParameter != null && aParam.MixParameter.IsMixAfterDis)
                            {
                                res = Mix(aParam);//混匀处理
                            }

                            if (res == ResultCode.S_SUCCESSED && resCode == ResultCode.S_SUCCESSED && aParam.DisParam.TrailingAirgap > 0)
                            {
                                if (_dispenseDepthStep > 0)
                                {
                                    signCode = Common.CODE_Z;
                                    resCode = MotorZ
                                        .RelativeMove(MotorConstant.DIR_N, _dispenseDepthStep); //向上移动Z轴到排液深度位置DisInnerLiquidStep
                                }

                                if (resCode == ResultCode.S_SUCCESSED)
                                {
                                    signCode = Common.CODE_ADP;
                                    resCode = Adp.SetMaxSpeed(aParam.DisParam.AspiratingSpeed, mode);
                                    if (resCode == ResultCode.S_SUCCESSED)
                                    {
                                        resCode = Adp.Aspirate(aParam.DisParam.TrailingAirgap, mode);
                                        waitEvent.WaitOne(DelayBalance);
                                    }
                                }
                            }
                        }
                        else //吸一加多
                        {
                            var disVolume = aParam.DisParam.DisVolume;
                            double airGap = 0;
                            airGap = aParam.DisParam.TrailingAirgap;
                            disVolume += airGap;
                            resCode = Adp.Dispense(disVolume, mode);
                            waitEvent.WaitOne(aParam.DisParam.DelayDis);
                            _log.Debug("The number {0} dispense volume is {1}, include airGap = {2}",
                                aParam.DisParam.CurrentDisTime,
                                disVolume,
                                airGap);

                            if (resCode == ResultCode.S_SUCCESSED)
                            {
                                if (_dispenseDepthStep > 0)
                                {
                                    signCode = Common.CODE_Z;
                                    resCode = MotorZ
                                        .RelativeMove(MotorConstant.DIR_N, _dispenseDepthStep); //DisInnerLiquidStep
                                }

                                if (resCode == ResultCode.S_SUCCESSED)
                                {
                                    signCode = Common.CODE_ADP;
                                    resCode = Adp.SetMaxSpeed(aParam.DisParam.AspiratingSpeed, mode);
                                    if (resCode == ResultCode.S_SUCCESSED)
                                    {
                                        resCode = Adp.Aspirate(aParam.DisParam.TrailingAirgap, mode);
                                        waitEvent.WaitOne(DelayBalance);
                                    }
                                }
                            }
                            else
                            {
                                _log.Debug(
                                    "The number {0} dispense volume is {1}, include airGap = {2}, error code is {3}",
                                    aParam.DisParam.CurrentDisTime,
                                    disVolume,
                                    airGap, resCode);
                            }
                        }
                    }
                }
            }

            if (res == ResultCode.S_SUCCESSED)
            {
                res = GetResultCode(signCode, resCode);
            }
            return res;
        }

        /// <summary>
        /// 排液
        /// </summary>
        /// <param name="aParam">液体参数</param>
        /// <returns></returns>
        private int DispenseInProcess(LiqHaParameter aParam)
        {
            int res = CheckDeviceState(ResultCode.S_SUCCESSED, aParam.Channel);
            if (res != ResultCode.S_SUCCESSED)
                return res;
            var retryErrCodes = new List<Int32>()
            {
                ResultCode.ERR_TIP_LOST,
                ResultCode.ERR_ADP_INVALID_COMMAND,
                ResultCode.ERR_ADP_INVALID_OPERAND,
                ResultCode.ERR_ADP_OVER_PRESSURE,
                ResultCode.ERR_ADP_LIQUID_LEVEL_DETECT_FAILURE,
                ResultCode.ERR_ADP_PLUNGER_OVERLOAD,
                ResultCode.ERR_ADP_TIP_LOST_OR_NOT_PRESENT,
                ResultCode.ERR_ADP_COMMAND_BUFFER_EMPTY_OR_EXECUTED_OR_NOT_READY_FOR_REPEAT,
                ResultCode.ERR_ADP_COMMAND_BUFFER_OVERFLOW,
            };

            int tmpZ = aParam.DesLocation.Z;
            _dispenseDepthStep = GetStep(aParam.DispenseDepth);
            //aParam.DesLocation.Z += _dispenseDepthStep;
            var loc = aParam.DesLocation.Clone();
            loc.Z += GetStep(aParam.DispenseDepth);
            var isChannelPcr = aParam.Channel > Channels.Tip;
            if (isChannelPcr)
            {
                loc.Z -= GetStep(5);
            }
            res = MoveTo(loc, aParam.Channel);//移动到排液孔
            if (res == ResultCode.S_SUCCESSED && isChannelPcr)
            {
                _dispenseDepthStep = GetStep(8);
                res = MotorZ.SetSlowSpeedFlag(3);
                res = MoveToZ(tmpZ);
                WaitForBalance(1000);
            }
            if (res == ResultCode.S_SUCCESSED)
            {
                res = SingleDispense(aParam);
                if (retryErrCodes.Contains(res % 100))
                {
                    CurrentErrCode = res;
                    res = MoveToSafeZ();
                    if (res == ResultCode.S_SUCCESSED)
                    {
                        if (DoRetry(CurrentErrCode, false))
                        {
                            switch (RetryFlag)
                            {
                                case RetryFlag.Ignore:
                                    res = ResultCode.S_SUCCESSED;
                                    break;
                                case RetryFlag.Retry:
                                    res = DispenseInProcess(aParam);
                                    break;
                                default:
                                    res = CurrentErrCode;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        _log.Information("Dispense error {0:X}", res);
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// 排液
        /// </summary>
        /// <param name="aParam">液体参数</param>
        /// <returns></returns>
        public int Dispense(LiqHaParameter aParam)
        {
            var res = ResultCode.S_SUCCESSED;
            res = DispenseInProcess(aParam);
            return res;
        }

        /// <summary>
        /// 刺膜
        ///// </summary>
        /// <param name="loc">位置</param>
        /// <param name="ch">通道</param>
        /// <returns></returns>
        public int PrickSealingFilm(Location loc, Channels ch)
        {
            var res = ResultCode.S_SUCCESSED;
            res = CheckDeviceState(res, ch);
            var tmpZ = loc.Z;
            loc.Z = tmpZ + PrickStep;
            res = MoveTo(loc, ch);//移动，刺膜
            if (CheckDeviceState(res, ch) == ResultCode.S_SUCCESSED)
            {
                res = MoveToSafeZ();
                if (CheckDeviceState(res, ch) == ResultCode.S_SUCCESSED)
                {
                    res = MotorZReset();
                }
            }

            loc.Z = tmpZ;
            return res;

        }

        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceState DeviceState
        {
            get;
            private set;
        }

        public DeviceStateManager DevStateManager { get; }

        private int CheckDeviceState(int res)
        {
            if (res == ResultCode.S_SUCCESSED)
            {
                if (DeviceState == DeviceState.Stop)
                {
                    res = ResultCode.S_STOP;
                }
                else if (DeviceState == DeviceState.Pause)
                {
                    _stateEvent.WaitOne();
                    res = ResultCode.S_SUCCESSED;
                }
            }

            return res;
        }

        public void Suspend()
        {
            _stateEvent.Reset();
            DevStateManager.CurrentDevState = DeviceState.Pause;
        }

        public void Continue()
        {
            DevStateManager.CurrentDevState = DeviceState.Running;
            _stateEvent.Set();
        }

        public void Stop()
        {
            DevStateManager.CurrentDevState = DeviceState.Stop;
            _stateEvent.Set();
        }

        private AutoResetEvent _stateEvent = new AutoResetEvent(false);
        private void _devStateManager_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            DeviceState = e.DeviceState;
        }

        public List<Channels> StopChannelList { get; } = new List<Channels>();
        public void Stop(Channels ch)
        {
            if (!StopChannelList.Contains(ch))
            {
                StopChannelList.Add(ch);
            }
            DevStateManager.CurrentDevState = DeviceState.Stop;
            _stateEvent.Set();
        }

        public void RemoveStopFlag(Channels ch)
        {
            if (StopChannelList.Contains(ch))
            {
                StopChannelList.Remove(ch);
            }
        }

        private int CheckDeviceState(int res, Channels ch)
        {
            if (res == ResultCode.S_SUCCESSED)
            {
                if (DeviceState == DeviceState.Stop)
                {
                    if (StopChannelList.Contains(ch))
                    {
                        StopChannelList.Remove(ch);
                        res = Common.CODE_MODULE_CH + Common.CODE_CHANNEL * ((int)ch) + ResultCode.S_STOP;
                    }
                }
                else if (DeviceState == DeviceState.Pause)
                {
                    _stateEvent.WaitOne();
                    res = ResultCode.S_SUCCESSED;
                }
            }

            return res;
        }

        public int TestSingleAspirate()
        {
            var aParam = new LiqHaParameter();
            aParam.DisParam.DisTimes = 1;
            aParam.DisParam.DisVolume = 100;
            aParam.DisParam.TipType = 2;
            aParam.DisParam.DisVolumeList.Add(100);
            var step = 0;
            _curTipType = 2;
            return SingleAspirate(aParam, false, ref step);
        }
    }
}
