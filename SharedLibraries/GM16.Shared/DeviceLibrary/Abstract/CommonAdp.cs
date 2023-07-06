using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public abstract class CommonAdp
    {
        public int PressureInitial { get; set; }

        /// <summary>
        /// ADP配置信息
        /// </summary>
        public AdpInfo AdpInfo { get; set; }

        public abstract int Init();

        public abstract int EjectTip(char mode);

        public abstract int Reset();

        public abstract int SetMaxSpeed(float speed, byte mode);

        public abstract int SetStartSpeed(float speed, byte mode);

        public abstract int SetCutOffSpeed(float speed, byte mode);

        /// <summary>
        /// 将ADP活塞移至指定绝对位置，排空液体
        /// </summary>
        /// <param name="pos">体积</param>
        /// <param name="mode">模式（0-1）</param>
        /// <returns></returns>
        public abstract int AbsMove(double pos, byte mode);

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="pos">体积</param>
        /// <param name="mode">模式（0-1）0：增量，1-微升</param>
        /// <returns></returns>
        public abstract int Aspirate(double pos, byte mode);

        public abstract int LiquidDetection(char mode);

        public abstract int SetPLLD(byte mode, UInt32 paramValue);

        /// <summary>
        /// 报告Tip头状态
        /// </summary>
        /// <param name="state">false：不存在，true：存在</param>
        /// <returns></returns>
        public abstract int ReportTipState(ref bool state);
        /// <summary>
        /// 以ADC计数报告当前压力
        /// </summary>
        /// <param name="pressure"></param>
        /// <returns></returns>

        public abstract int ReportPressure(ref int pressure);
        /// <summary>
        /// 分配液体
        /// </summary>
        /// <param name="pos">体积</param>
        /// <param name="mode">模式（0-1）</param>
        /// <returns></returns>

        public abstract int Dispense(double pos, byte mode);

    }
}
