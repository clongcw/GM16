using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GM16.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel Main
        {
            get
            {
                return App.Current.Services.GetService<MainViewModel>();
            }
        }

        public MutiplePipetteAdjustViewModel MutiplePipetteAdjust
        {
            get { return App.Current.Services.GetService<MutiplePipetteAdjustViewModel>(); }
        }

        public ProtocolManagementViewModel ProtocolManagementViewModel
        {
            get { return App.Current.Services.GetService<ProtocolManagementViewModel>(); }
        }
    }
}
