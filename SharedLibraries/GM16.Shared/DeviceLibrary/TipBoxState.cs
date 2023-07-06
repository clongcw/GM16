using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public class TipBoxState : List<TipState>
    {
        /// <summary>
        /// 每盒TIP数量
        /// </summary>
        public static readonly int MaxTipCount = 96;

        public int Index { get; set; }

        public int LayoutIdx { get; set; }

        public int TipType { get; set; }

        public int CurrentTipIndex { get; set; }

        public bool Enabled { get; set; }

        public TipBoxState()
        {
            TipType = 0;
        }

        public TipBoxState(int tipType, int tipCount)
        {
            TipType = tipType;
            CurrentTipIndex = 0;
            Enabled = true;
            if (tipCount > MaxTipCount)
            {
                tipCount = MaxTipCount;
            }
            for (int i = 0; i < tipCount; i++)
            {
                this.Add(new TipState(this.Count, true));
            }
        }

        public void Add(int tipCount)
        {
            Enabled = true;
            if (tipCount + Count > MaxTipCount)
            {
                tipCount = MaxTipCount - Count;
            }
            for (int i = 0; i < tipCount; i++)
            {
                this.Add(new TipState(this.Count, true));
            }
        }
    }
}
