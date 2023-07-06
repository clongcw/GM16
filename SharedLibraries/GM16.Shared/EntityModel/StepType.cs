using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    public enum StepType
    {
        [Description("移液")]
        Pipptte,
        [Description("加样")]
        AddSmp,
        [Description("分配模板")]
        TransferTemplate,
        [Description("戳膜")]
        Prick,
        [Description("转移至芯片")]
        TransToChip,
        [Description("PCR")]
        Pcr,
    }
}
