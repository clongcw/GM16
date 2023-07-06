using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("LiquidContainers")]

    public class LiquidContainer
    {
        public int Id { get; set; }

        public int ConType { get; set; }

        public string ChineseName { get; set; }

        public string EnglishName { get; set; }

        public double Height { get; set; }

        public double ExcessVolHeight { get; set; }

        public double Volume { get; set; }

        public double AspirateZ { get; set; }

        public double DispenseZ { get; set; }

        public double LiquidDetectZ { get; set; }

        public double HeightDiff { get; set; }
        public double DetectLiquidLevelErr { get; set; }//检测液位
        public double UnitStepVolume { get; set; } = 1;//每一步容积
        public int OverLiquidLevel { get; set; }
    }
}
