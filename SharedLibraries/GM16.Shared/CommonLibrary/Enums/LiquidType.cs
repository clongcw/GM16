using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary.Enums
{
    /// <summary>
    /// 液体类型
    /// </summary>
    public enum LiquidType
    {
        [Description("水或试剂")]
        Water = 0x01,
        [Description("血清")]
        Serum,
        [Description("其它")]
        Other,
    }
}
