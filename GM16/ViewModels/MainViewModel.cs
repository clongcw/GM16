using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GM16.Shared.EntityModel;
using GM16.Shared.EntityModel.DBContext;
using GM16.Shared.Services;
using GM16.UI.Utils;
using GM16.Views;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panuon.WPF.UI;
using PropertyChanged;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GM16.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public partial class MainViewModel : ObservableObject
    {
        #region Filed
        private readonly Logger _log = Logger.Instance;

        private Stopwatch _stopwatch;

        private DispatcherTimer _timer;

        private UserContext _contextUser = new UserContext();
        #endregion

        #region Property
        public string CurrentDateTime { get; set; }
        public object Content { get; set; } //= App.Current.Services.GetService<AdpAdjust>();

        public User CurrentUser { get; set; } = new();
        #endregion

        #region Command
        [RelayCommand]
        public void Loaded()
        {
            _log.Debug("hello,world");
        }

        [RelayCommand]
        public void Show()
        {
            Test ob = new Test();
            ob.Show();

            Toast.Show((MainView)Application.Current.MainWindow, "这是一条测试消息", 10000);
        }

        [RelayCommand]
        public void SelectionChanged(object listboxitem)
        {
            var viewname = string.Empty;

            if (listboxitem as ListBoxItem != null)
            {
                var textBlock = WpfUtils.FindVisualChild<TextBlock>(listboxitem as ListBoxItem);
                if (textBlock != null)
                {
                    viewname = textBlock.Text;
                }
            }

            switch (viewname)
            {
                case "主页":
                    Content = App.Current.Services.GetService<AdpAdjust>();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            #region 刷新时间
            _stopwatch = new Stopwatch();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // 每秒触发一次
            _timer.Tick += (s, e) =>
            {
                CurrentDateTime = DateTime.Now.ToString("yyyy年M月d日 HH:mm:ss dddd", new CultureInfo("zh-CN"));
            };
            _stopwatch.Start();
            _timer.Start();
            #endregion

            _contextUser.Db.DbMaintenance.CreateDatabase();
            _contextUser.Db.CodeFirst.InitTables<User, Role, Privilege>();


            Role role = new Role();
            role.RoleName = "普通用户";

            _contextUser.Roles.Insert(role);
            _contextUser.Privileges.Insert(new Privilege() { RoleId = 2, Name = "Lis", Visiual = "Collapsed" });
            _contextUser.Privileges.Insert(new Privilege() { RoleId = 2, Name = "Report", Visiual = "Collapsed" });
            _contextUser.Privileges.Insert(new Privilege() { RoleId = 2, Name = "Setting", Visiual = "Collapsed" });
            var list = _contextUser.Db.Queryable<Role>()
                                        .Includes(x => x.Privileges).ToArray();
            //.ToList();

            User user = new User();
            user.Name = "admin";
            user.Password = "123456";
            user.RoleName = list[1].RoleName;
            user.Role = list[1];
            _contextUser.Users.Insert(user);


        }
        #endregion
    }
}
