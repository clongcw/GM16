using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public class TipState
    {
        public int Index { get; set; }
        /// <summary>
        /// true ： 可以使用
        /// false： 已经被使用，无法继续使用
        /// </summary>
        public bool State { get; set; }

        public TipState(int index, bool state)
        {
            Index = index;
            State = state;
        }
    }
}
