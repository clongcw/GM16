using CommunityToolkit.Mvvm.ComponentModel;
using GM16.Global;
using GM16.Shared.EntityModel;
using GM16.UI.Controls.Guide;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GM16.ViewModels
{
    public class AdpAdjustViewModel : ObservableObject
    {
        public User CurrentUser { get; set; }

        public AdpAdjustViewModel()
        {
            CurrentUser = GlobalModel.CurrentUser;
        }
    }
}
