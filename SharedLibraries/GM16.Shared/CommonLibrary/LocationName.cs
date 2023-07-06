using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommonLibrary
{
    public static class LocationName
    {
        public static string EjectTipLoc = "EjectTip";
        public static string TipBoxName = "TipBox";
        public static string TipBoxInterval = "TipBoxInterval";
        public static string Extraction = "Extraction";
        public static string Waste = "Waste";
        public static string LocIC = "IC";
        public static string Template = "T";
        public static string Sample = "Smp";
        public static string LocPcr = "PCR";
        public static string LocTip = "Tip";
        public static string EjectChipLoc = "EjectChip";//顶出芯片位置坐标
        public static string LocTransPcr = "TransPcr";
        public static string LocHandlePcr = "HandlePcr";//搬运模块，加载芯片的坐标位置
        public static string LocAdpDisPcr = "AdpDisPcr";
        public static string LocHandlePcrZ2X = "HandlePcrForZ2X";//Z搬运到X轴待吸取位置坐标
        public static string LocXHandlePcrZ2X = "XHandlePcrForZ2X";//x搬运移动到Z搬运取芯片位置
        public static string LocXHandleToPcr = "XHandleToPcr";
        public static string LocXHandlePcrToWaste = "XHandlePcrToWaste";
        public static string LocXHandlePcrToHeater = "LocXHandlePcrToHeater";
        public static string LocCap = "Cap";

        #region 热封
        public static string LocSealingYCollecttion = "SealingYCollection";
        public static string LocSealingYSeal = "SealingYSealing";
        public static string LocSealingYHome = "SealingYHome";
        public static string LocSealingZSeal = "SealingZSealing";
        public static string LocSealingZHome = "SealingZHome";
        #endregion
        public static string GetPcrName(int id)
        {
            return string.Format("{0}{1}", Template, id + 1);
        }

        public static string GetTipName(int id = 0)
        {
            return string.Format("{0}{1}", LocTip, id + 1);
        }

        public static string GetXHandleToPcr(int id)
        {
            return $"{LocXHandleToPcr}{id}";
        }

        public static string GetScanLocName(int ch)
        {
            return $"{(char)('A' + ch)}1";
        }
    }
}
