using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public class TipManager : List<TipBoxState>
    {
        public TipManager()
        {
            MaxTipBoxCount = 2;
        }

        public void SetMaxTipBoxCount(int count)
        {
            MaxTipBoxCount = count;
        }
        /// <summary>
        /// 最大TIP头数量
        /// </summary>
        public static int MaxTipBoxCount { get; private set; }
        /// <summary>
        /// 添加Tip头盒
        /// </summary>
        /// <param name="tipIdx"></param>
        /// <param name="tipCount"></param>
        public TipBoxState Add(int tipIdx, int tipCount)
        {
            TipBoxState tipBox = new TipBoxState(tipIdx, tipCount);
            tipBox.Index = this.Count % MaxTipBoxCount;
            this.Add(tipBox);
            return tipBox;
        }

        /// <summary>
        /// 设置指定Tip头盒的状态
        /// </summary>
        /// <param name="tipBoxIndex"></param>
        /// <param name="startTipId"></param>
        /// <param name="endTipId"></param>
        /// <param name="state"></param>
        public void SetTipBoxState(int tipBoxIndex, int startTipId, int endTipId, bool state)
        {
            for (int i = startTipId; i < endTipId; i++)
            {
                this[tipBoxIndex][i].State = state;
            }
            this[tipBoxIndex].Where(p => p.Index >= startTipId && p.Index < endTipId)
               .ToList()
               .ForEach(p => p.State = state);
        }

        private void SetFullTipBoxState(int index, bool state)
        {
            SetTipBoxState(index, 0, 95, state);
            this[index].CurrentTipIndex = 0;
            this[index].Enabled = state;
        }

        /// <summary>
        /// 填充指定Tip头盒
        /// </summary>
        /// <param name="index"></param>
        public void FillTipBox(int index)
        {
            SetFullTipBoxState(index, true);
        }

        /// <summary>
        /// 清空指定Tip头盒
        /// </summary>
        /// <param name="index"></param>
        public void ClearTipBox(int index)
        {
            SetFullTipBoxState(index, false);
        }
    }
}
