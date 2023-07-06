using GM16.ViewModels;
using GM16.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GM16
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App? Current => Application.Current as App;


        public App()
        {
            Services = ConfigureServices();

        }

        public IServiceProvider Services { get; private set; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainView>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MutiplePipetteAdjustViewModel>();
            services.AddSingleton<AdpAdjustViewModel>();
            services.AddSingleton<AdpAdjust>();
            services.AddSingleton<ProtocolManagementViewModel>();
            



            return services.BuildServiceProvider();
        }

        public void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = Services.GetService<MainView>();
            mainWindow.DataContext= Services.GetService<MainViewModel>();
            mainWindow!.Show();
        }
    }
}
