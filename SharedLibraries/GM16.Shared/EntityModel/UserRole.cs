using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.EntityModel
{
    public enum UserRole
    {
        [Description("工程师")]
        Engineer,
        [Description("管理员")]
        Admin,
        [Description("普通用户")]
        Operator
    }
}
