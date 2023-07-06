
using GM16.Shared.EntityModel;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Global
{
    [AddINotifyPropertyChangedInterface]
    public static class GlobalModel
    {
        public static User CurrentUser { get; set; }
    }
}
