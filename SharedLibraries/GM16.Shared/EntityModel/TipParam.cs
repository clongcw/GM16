using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("TipParams")]
    public class TipParam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TipType { get; set; }

        public double Slope { get; set; }

        public double Intercept { get; set; }

        public double MinPressureDiff { get; set; }

        public bool IsCheckBlock { get; set; }

        public bool IsCheckAspNull { get; set; }
    }
}
