using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("MixParameters")]
    public class MixParameter
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Times { get; set; } = 5;

        public double Volume { get; set; } = 100;

        public double AspirateDepth { get; set; } = 28;//吸液深度

        public double DispenseDepth { get; set; } = 23;//排液深度

        public bool IsMixAfterDis { get; set; } = true;

        public Step Step { get; set; }

        //[NotMapped]
        public double DispenseOffsetY { get; set; } = 5;
    }
}
