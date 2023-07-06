using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public class AdpInfo
    {
        public string Name { get; set; }

        public int NodeId { get; set; }

        public bool Enabled { get; set; }

        public bool IsInitialized { get; set; }
    }
}
