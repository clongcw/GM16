using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GM16.Shared.CommonLibrary;
using GM16.Shared.EntityModel;
using GM16.Shared.EntityModel.DBContext;
using GM16.UI.Controls.Guide;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Virtue.iGenePad.Views;

namespace GM16.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public partial class ProtocolManagementViewModel : ObservableObject
    {
        #region Fields
        private ProtocolContext _contextPr = new ProtocolContext();
        #endregion

        #region Property
        public int PcrCount { get; set; }

        public string PcrTemplateFilePath { get; set; }

        public ObservableCollection<string> LocNames { get; set; } = new();

        public ObservableCollection<string> TipNames { get; set; } = new();
        public ObservableCollection<Protocol> Protocols { get; set; } = new();
        public Protocol CurrentProtocol { get; set; } //= new();
        public ObservableCollection<Step> Steps { get; set; } = new();
        public string TemplateFileName { get; set; }
        #endregion

        #region Constructor
        public ProtocolManagementViewModel()
        {
            GenerateProtocol();
            LoadConfigs();
            var startName = 'A';
            for (int i = 0; i < 13; i++)
            {
                var name = (char)(startName + i);
                LocNames.Add(name.ToString());
            }

            Protocols.Clear();

            // 执行查询获取 Protocol 列表
            var protocolList = _contextPr.Db.Queryable<Protocol>().ToList();

            // 遍历 Protocol 列表
            foreach (var protocol in protocolList)
            {
                // 手动加载关联的 Steps 数据
                var steps = _contextPr.Db.Queryable<Step>().Where(s => s.ProtocolId == protocol.Id).ToList();

                // 将步骤数据添加到对应的 Protocol 对象中
                protocol.Steps = steps;

                // 将 Protocol 对象添加到 _protocols 集合中
                Protocols.Add(protocol);
            }


            _contextPr.MixParametersDb.AsQueryable();
            if (Protocols.Count > 0)
            {
                CurrentProtocol = Protocols[0];
                GetSteps();

            }
        }



        #endregion

        #region Commands
        [RelayCommand]
        public void SelectProtocol()
        {
            if (CurrentProtocol != null)
            {
                TemplateFileName = CurrentProtocol.PcrTemplateFile;
                //GetAmountOfPcrTemplateContent(TemplateFileName);
                GetSteps();
            }
        }
        #endregion

        #region Mehtods
        public void GenerateProtocol()
        {

        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        private void LoadConfigs()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "Setting.ini";
            string section = "System";
            StringBuilder strVal = new StringBuilder(255);

            GM16.Shared.CommonLibrary.Common.GetPrivateProfileString(section, nameof(PcrTemplateFilePath), "", strVal, 255, filePath);
            var tmpPath = strVal.ToString();
            var defPath = $"{AppDomain.CurrentDomain.BaseDirectory}PcrTemplateFiles\\";
            if (!string.IsNullOrEmpty(tmpPath))
            {
                if (Directory.Exists(tmpPath))
                {
                    PcrTemplateFilePath = tmpPath;
                }
                else
                {
                    Directory.CreateDirectory(defPath);
                    PcrTemplateFilePath = defPath;

                }
            }
            else
            {
                Directory.CreateDirectory(defPath);
                PcrTemplateFilePath = defPath;
            }
        }

        private void GetSteps()
        {
            Steps.Clear();
            if (CurrentProtocol != null)
            {
                var tmpSteps = CurrentProtocol.Steps.OrderBy(s => s.Sequence);
                foreach (var step in tmpSteps)
                {
                    Steps.Add(step);
                }
            }
        }

        /// <summary>
        /// 获取模板PCR孔数量
        /// </summary>
        /// <returns></returns>
        private void GetAmountOfPcrTemplateContent(string fileName)
        {
            var pcrTemplateFile = $"{PcrTemplateFilePath}{fileName}";//PCR模板
            var experiment = ExperimentFactory.Instance.MakeExperiment(pcrTemplateFile, true);
            var experimentExtraViewModel = new ExperimentExtraViewModel(experiment);
            PcrCount = experimentExtraViewModel.Experiment.Template.Amount;
        }
        #endregion

        #region GuideControlViewModel
        private GuideInfo? _btnCloseGuide;

        private GuideInfo? _btnLeftBottomGuide;

        private GuideInfo? _btnLeftTopGuide;

        private GuideInfo? _btnRightBottomGuide;

        private GuideInfo? _btnShowGuide;

        //private GuideInfo? _listBoxItemGuide;

        public GuideInfo BtnCloseGuide =>
            _btnCloseGuide ??= new GuideInfo("关闭点这里", "不使用了点击这里关闭窗体");

        public GuideInfo BtnShowGuide =>
            _btnShowGuide ??= new GuideInfo("不会用？点击这里吧，可以查看引导在窗体不同位置的显示", "窗体的4个角、中间展示引导提示信息，标题或者描述较长以换行的形式展示");

        public GuideInfo BtnLeftTopGuide =>
            _btnLeftTopGuide ??= new GuideInfo("左上引导", "测试左上引导提示显示位置");

        public GuideInfo BtnLeftBottomGuide =>
            _btnLeftBottomGuide ??= new GuideInfo("左下引导", "测试左下引导提示显示位置");

        public GuideInfo BtnRightBottomGuide =>
            _btnRightBottomGuide ??= new GuideInfo("右下引导", "测试右下引导提示显示位置");

        //public GuideInfo ListBoxItemGuide =>_listBoxItemGuide ??= new GuideInfo("嵌套控件的引导", "测试嵌套控件的引导");

        public List<GuideInfo> GuideLists => new()
        {
            BtnShowGuide,
            BtnCloseGuide,
            BtnLeftTopGuide,
            BtnLeftBottomGuide,
            BtnRightBottomGuide,
            //ListBoxItemGuide
        };
        #endregion
    }
}
