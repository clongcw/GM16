using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace GM16.Shared.EntityModel
{
    /// <summary>
    /// 移液类型（吸一加一、吸一加多）
    /// </summary>
    public enum PipettingType
    {
        [Description("吸一加一")]
        Single,
        [Description("吸一加多")]
        Multiple
    }

    [SugarTable("AdpParameters")]

    public class AdpParameter
    {
        public long Id
        {
            get;
            set;
        }

        private double _disVolume;

        /// <summary>
        /// 分配液体体积
        /// </summary>
        public double DisVolume
        {
            get
            {
                return _disVolume;
            }
            set
            {
                _disVolume = Math.Round(value, 3);
            }
        }

        /// <summary>
        /// 分配液体次数
        /// </summary>
        public int DisTimes
        {
            get;
            set;
        }

        /// <summary>
        /// 多次分配液体体积列表（每次可能不一样）
        /// </summary>
        [NotMapped]
        public List<double> DisVolumeList
        {
            get;
            set;
        }


        /// <summary>
        /// 当前分配次数
        /// </summary>
        [NotMapped]
        public int CurrentDisTime
        {
            get;
            set;
        }

        ///// <summary>
        ///// 液体类型
        ///// </summary>
        //public LiquidType LiquidType
        //{
        //    //get
        //    //{
        //    //    return (LiquidType)LiquidTypeValue;
        //    //}
        //    //set
        //    //{
        //    //    LiquidTypeValue = (int)value;
        //    //}
        //    get;
        //    set;
        //}

        public int LiquidType
        {
            get;
            set;
        }

        /// <summary>
        /// Tip头容量
        /// </summary>
        public int TipType
        {
            get;
            set;
        }

        public PipettingType PipettingType
        {
            get;
            set;
        }

        /// <summary>
        /// 吸液速度
        /// </summary>
        public int AspiratingSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 吸液延时
        /// </summary>
        public int DelayAsp
        {
            get;
            set;
        }

        /// <summary>
        /// 预吸空气柱体积
        /// </summary>
        public int LeadingAirgap
        {
            get;
            set;
        }

        /// <summary>
        /// 封口空气柱体积
        /// </summary>
        public int TrailingAirgap
        {
            get;
            set;
        }

        /// <summary>
        /// 多吸取液体百分比
        /// </summary>
        public double ExcessVolume
        {
            get;
            set;
        }

        /// <summary>
        /// 调节作用液体体积
        /// </summary>
        public int ConditioningVolume
        {
            get;
            set;
        }

        /// <summary>
        /// 排液速度
        /// </summary>
        public int DispenseSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 断流速度
        /// </summary>
        public int BreakOffSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 排液延时
        /// </summary>
        public int DelayDis
        {
            get;
            set;
        }

        /// <summary>
        /// 吸液时Z轴移动速度
        /// </summary>
        public int AspiratingZSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// 吸液时回吐量
        /// </summary>
        public int ExtraVol
        {
            get;
            set;
        }

        private double _bakDisVolume;
        private double _leftVolume;

        /// <summary>
        /// 分配液体体积
        /// </summary>
        [NotMapped]
        public double BakDisVolume
        {
            get
            {
                return _bakDisVolume;
            }
            set
            {
                _bakDisVolume = Math.Round(value, 3);
            }
        }

        /// <summary>
        /// 多次分配液体体积列表（每次可能不一样）
        /// </summary>
        [NotMapped]
        public List<double> BakDisVolumeList
        {
            get;
            set;
        }

        public double LeftVolume { get => _leftVolume; set => _leftVolume = value; }

        public int Row { get; set; }

        public AdpParameter()
        {
            this.AspiratingZSpeed = 0;
            this.ExtraVol = 0;
            this.AspiratingSpeed = 650;
            this.BreakOffSpeed = 200;
            this.ConditioningVolume = 0;
            this.DelayAsp = 300;
            this.DelayDis = 1000;
            this.DispenseSpeed = 400;
            this.DisTimes = 0;
            this.DisVolume = 100;
            this.ExcessVolume = 0;
            this.LeadingAirgap = 20;
            this.LiquidType = 0;
            this.PipettingType = PipettingType.Single;
            this.TipType = 0;
            this.TrailingAirgap = 10;
            this.ExtraVol = 0;
            this.LeftVolume = 0;
            this.Row = 0;
            DisVolumeList = new List<double>();
            BakDisVolumeList = new List<double>();
        }

        public AdpParameter Clone()
        {
            AdpParameter res = new AdpParameter();
            res.ExtraVol = ExtraVol;
            res.AspiratingZSpeed = AspiratingZSpeed;
            res.AspiratingSpeed = AspiratingSpeed;
            res.BreakOffSpeed = BreakOffSpeed;
            res.ConditioningVolume = ConditioningVolume;
            res.DelayAsp = DelayAsp;
            res.DelayDis = DelayDis;
            res.DispenseSpeed = DispenseSpeed;
            res.DisTimes = DisTimes;
            res.DisVolume = DisVolume;
            res.ExcessVolume = ExcessVolume;
            res.LeadingAirgap = LeadingAirgap;
            res.LiquidType = LiquidType;
            res.PipettingType = PipettingType;
            res.TipType = TipType;
            res.TrailingAirgap = TrailingAirgap;
            res.CurrentDisTime = CurrentDisTime;
            res.LeftVolume = LeftVolume;
            res.Row = Row;
            res.DisVolumeList.AddRange(this.DisVolumeList);
            res.BakDisVolume = BakDisVolume;
            res.BakDisVolumeList.AddRange(this.BakDisVolumeList);
            return res;
        }
    }
}
