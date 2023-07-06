using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("TipInfos")]
    public class TipInfo
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [Required]
        public int TipType { get; set; }
        /// <summary>
        /// 容量
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; } = 30;
        /// <summary>
        /// 供应商
        /// </summary>
        //[AllowNull]
        public string Vender { get; set; }

        /// <summary>
        /// PLLD值
        /// </summary>
        public int PLLDValue { get; set; }

        /// <summary>
        /// AD阈值
        /// </summary>
        public int AdThread { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        //[AllowNull]
        public string Des { get; set; }

        public int ThresholdDuration { get; set; } = 5;

        public int AbsPos { get; set; } = 3000;

        public int MaxRelativePlunger { get; set; } = 3000;

        public int PerformMode { get; set; } = 1;

        public int Interval { get; set; } = 20;

        public int SmpNumber { get; set; } = 3;

        public int MaxDuration { get; set; } = 50;
    }
}
