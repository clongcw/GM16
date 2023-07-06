using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("Steps")]
    public class Step
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [SugarColumn]
        public int ProtocolId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>

        public string Name { get; set; }
        /// <summary>
        /// 吸液位置
        /// </summary>

        public string SourceName { get; set; }
        /// <summary>
        /// 排液位置
        /// </summary>

        public string TargetName { get; set; }
        /// <summary>
        /// 移液量
        /// </summary>

        public double Volume { get; set; }

        public double AspirateDepth { get; set; }

        public double DispenseDepth { get; set; }
        /// <summary>
        /// 磁吸时间
        /// </summary>

        public int MagneticTime { get; set; }

        public string TipPosName { get; set; }
        /// <summary>
        /// Tip头容积
        /// </summary>

        public int TipVolume { get; set; }

        /// <summary>
        /// 孵育时间
        /// </summary>

        public int Time { get; set; }

        public double Temperature { get; set; }

        public int Sequence { get; set; }
        /// <summary>
        /// tip头类型
        /// </summary>
        public int TipType { get; set; }

        public int LiquidType { get; set; }

        public bool IsEnabled { get; set; } = true;

        public StepType StepType { get; set; } = StepType.Pipptte;

        [SugarColumn(IsIgnore = true)]
        public List<MixParameter> MixParameters { get; set; }

        [Navigate(NavigateType.OneToOne, nameof(ProtocolId))]
        public Protocol Protocol { get; set; }

        [SugarColumn(IsIgnore = true)]
        public AutoResetEvent StepEvent { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int SubId { get; set; }

        [SugarColumn(IsIgnore = true)]
        public bool IsCompleted { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int EstimatedTime { get; set; }

        public Step()
        {
            MixParameters = new List<MixParameter>();
            StepEvent = new AutoResetEvent(false);
        }
    }
}
