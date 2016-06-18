using BaseLogic;
using BaseLogic.Xaml_Controls;
using BaseLogic.Xaml_Controls.Settings;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace PownedLogic.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        public static readonly SettingsViewModel instance = new SettingsViewModel();

        public SettingsContainer settingsContainer { get; private set; }

        private SettingsViewModel()
            : base()
        {
           
        }

        public void Init(LoadingControl loadingControl)
        {
            if (this.loadingControl == null)
            {
                this.loadingControl = loadingControl;
            }

            this.settingsContainer = Settings.settingsContainer;

        }
    }
}
