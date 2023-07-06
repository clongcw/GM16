using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommunicationLibrary
{
    public class CanDataReceivedEventArgs : EventArgs
    {
        public readonly CanData CanData;
        public CanDataReceivedEventArgs(CanData canData)
        {
            CanData = canData;
        }
    }
}
