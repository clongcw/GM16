using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public class TipLoc
    {
        public int TipBoxIdx { get; set; }          //Tip盒序号
        public int Idx { get; set; }                //Tip头序号

        public int LayoutIdx { get; set; }          //布局序号

        public override string ToString()
        {
            return string.Format("TipBox: {0}, Idx: {1}", TipBoxIdx, Idx);
        }
    }
}
