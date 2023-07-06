using GM16.Shared.CommonLibrary;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("LiqHaParameters")]

    public class LiqHaParameter
    {
        /// <summary>
        /// 目标位置
        /// </summary>
        public Location DesLocation
        {
            get;
            set;
        }

        /// <summary>
        /// ADP移液参数
        /// </summary>
        public AdpParameter DisParam
        {
            get;
            set;
        }

        public LiquidContainer LiqContainer
        {
            get;
            set;
        }

        public Channels Channel { get; set; }

        public bool IsLiquidDetect { get; set; }
        /// <summary>
        /// 吸液深度
        /// </summary>

        public double AspirateDepth { get; set; }

        /// <summary>
        /// 排液深度
        /// </summary>
        public double DispenseDepth { get; set; }
        public MixParameter MixParameter { get; set; }


        public LiqHaParameter()
        {
            DesLocation = new Location();
            DisParam = new AdpParameter();
            LiqContainer = new LiquidContainer();
        }
    }
}
