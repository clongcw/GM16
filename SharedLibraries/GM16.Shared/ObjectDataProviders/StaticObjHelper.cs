using GM16.Shared.CommonLibrary;
using GM16.Shared.EntityModel.DBContext;
using GM16.Shared.EntityModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Constant = GM16.Shared.CommonLibrary.Constant;

namespace GM16.Shared.ObjectDataProviders
{
    public class ItemSourceItemInfo : INotifyPropertyChanged
    {
        private int _val;
        public int Val
        {
            get { return _val; }
            set
            {
                if (_val != value)
                {
                    _val = value;
                    OnPropertyChanged("Val");
                }
            }
        }

        private string _des;
        public string Des
        {
            get { return _des; }
            set
            {
                if (_des != value)
                {
                    _des = value;
                    OnPropertyChanged("Des");
                }
            }
        }

        private void OnPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public static class StaticObjHelper
    {
        static StaticObjHelper()
        {
            var startName = 'A';
            for (int i = 0; i < Constant.ReagentWellCount; i++)
            {
                var name = (char)(startName + i);
                _locNames.Add(name.ToString());
            }
            _locNames.Add(LocationName.Extraction);
            _locNames.Add(LocationName.LocIC);
            _locNames.Add(LocationName.Waste);
            _locNames.Add(LocationName.Sample);
            _locNames.Add(LocationName.LocPcr);
            _tipPosNames.Add("Tip1");
            _tipPosNames.Add("Tip2");
            _tipPosNames.Add("Tip3");

            var context = new DatabaseContext();

            GetTipTypes.Clear();
            GetPosTipTypes.Clear();
            context.TipInfos.GetList().ForEach(t =>
            {
                GetTipTypes.Add(new ItemSourceItemInfo() { Des = t.Des, Val = t.TipType });

                if (t.TipType == 0 || t.TipType == 2)//只加载175和900的
                {
                    GetPosTipTypes.Add(new ItemSourceItemInfo() { Des = t.Des, Val = t.TipType });
                }

            });

            GetLiquidTypes.Clear();
            context.LiquidTypeInfos.GetList().ForEach(t =>
            {
                GetLiquidTypes.Add(new ItemSourceItemInfo() { Des = t.ChineseName, Val = t.LiquidType });
            });
            context.LiquidContainers.GetList().ForEach(l =>
            {
                _containerTypes.Add(new ItemSourceItemInfo() { Des = l.ChineseName, Val = l.ConType });
            });
            var smpTypes = context.SampleTypes.GetList().OrderBy(s => s.Sequence).ToList();
            smpTypes.ForEach(s =>
            {
                _smpTypes.Add(new ItemSourceItemInfo() { Des = s.ChineseName, Val = s.SmpType });
            });
        }

        private static ObservableCollection<string> _locNames = new ObservableCollection<string>();
        public static ObservableCollection<string> GetLocNames()
        {
            return _locNames;
        }

        public static ObservableCollection<string> GetLiqHaLocNames()
        {
            var locNames = new ObservableCollection<string>();
            locNames.Add(LocationName.Sample);
            locNames.Add(LocationName.Extraction);
            locNames.Add(LocationName.LocIC);
            locNames.Add(LocationName.Waste);
            locNames.Add(LocationName.LocPcr);
            return locNames;
        }

        private static ObservableCollection<string> _tipPosNames = new ObservableCollection<string>();
        public static ObservableCollection<string> GetTipPosNames()
        {
            return _tipPosNames;
        }

        public static ObservableCollection<ItemSourceItemInfo> GetTipTypes { get; } = new ObservableCollection<ItemSourceItemInfo>();
        public static ObservableCollection<ItemSourceItemInfo> GetPosTipTypes { get; } = new ObservableCollection<ItemSourceItemInfo>();//调试界面，Tip头位置类型

        public static Dictionary<Enum, string> GetPipetteTypeSource()
        {
            return EnumMapHelper.GetEnumDataSourceDictionary(typeof(PipettingType));
        }

        public static ObservableCollection<ItemSourceItemInfo> GetLiquidTypes { get; } = new ObservableCollection<ItemSourceItemInfo>();

        public static Dictionary<Enum, string> GetAllChannels()
        {
            return EnumMapHelper.GetEnumDataSourceDictionary(typeof(Channels));
        }

        /// <summary>
        /// 获取所有通道
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Enum, string> GetExtractChannels()
        {
            var channels = new Dictionary<Enum, string>();
            channels.Add(Channels.A, EnumMapHelper.GetStringFromEnum(Channels.A));
            channels.Add(Channels.B, EnumMapHelper.GetStringFromEnum(Channels.B));
            channels.Add(Channels.C, EnumMapHelper.GetStringFromEnum(Channels.C));
            channels.Add(Channels.D, EnumMapHelper.GetStringFromEnum(Channels.D));
            return channels;
        }

        public static ObservableCollection<ItemSourceItemInfo> GetCalibrateVolumes()
        {
            var vols = new ObservableCollection<ItemSourceItemInfo>();
            DeviceConstant.PraseAppConfToList("CalibrateVolumes").ForEach(c =>
            {
                vols.Add(new ItemSourceItemInfo() { Des = c.ToString(), Val = c });
            });
            return vols;
        }

        public static ObservableCollection<ItemSourceItemInfo> GetTipIds()
        {
            var tipIds = new ObservableCollection<ItemSourceItemInfo>();
            for (int i = 0; i < DeviceConstant.MaxTipCount; i++)
            {
                tipIds.Add(new ItemSourceItemInfo() { Val = i, Des = (i + 1).ToString() });
            }
            return tipIds;
        }

        public static Dictionary<Enum, string> GetStepTypes()
        {
            return EnumMapHelper.GetEnumDataSourceDictionary(typeof(StepType));
        }

        private static ObservableCollection<ItemSourceItemInfo> _smpTypes = new ObservableCollection<ItemSourceItemInfo>();
        public static ObservableCollection<ItemSourceItemInfo> GetSampleTypes()
        {
            return _smpTypes;
        }

        private static ObservableCollection<ItemSourceItemInfo> _containerTypes = new ObservableCollection<ItemSourceItemInfo>();
        public static ObservableCollection<ItemSourceItemInfo> GetContainerTypes()
        {
            return _containerTypes;
        }

        public static ObservableCollection<ItemSourceItemInfo> GetDispenseRows()
        {
            var rows = new ObservableCollection<ItemSourceItemInfo>();
            for (int i = 0; i < 2; i++)
            {
                rows.Add(new ItemSourceItemInfo() { Val = i, Des = (i + 1).ToString() });
            }
            return rows;
        }
    }
}
