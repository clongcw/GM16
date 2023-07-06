using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    [SugarTable("LiquidTypeInfos")]

    public class LiquidTypeInfo
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [Required]
        public int LiquidType { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string EnglishName { get; set; }
    }
}
