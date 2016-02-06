using BaseLogic;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using XamlControlLibrary;
using XamlControlLibrary.Settings;

namespace PownedLogic.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        public static readonly SettingsViewModel instance = new SettingsViewModel();

        public LoginInfo loginInfo { get; private set; }
        public SettingsContainer settingsContainer { get; private set; }
        public Visibility LoginLoadingPanelVisibility { get; private set; }

        private SettingsViewModel()
            : base()
        {
            LoginLoadingPanelVisibility = Visibility.Collapsed;
        }

        public void Init(LoadingControl loadingControl)
        {
            if (this.loadingControl == null)
            {
                this.loadingControl = loadingControl;
            }

            this.settingsContainer = Settings.settingsContainer;
            this.loginInfo = LoginInfoDataHandler.instance.GetLoginInfo();

            if (MainpageViewModel.instance.LoginTask != null && !MainpageViewModel.instance.LoginTask.IsCompleted)
            {
                this.IsLoading = true;
                loginInfo.LoginControlsEnabled = false;
                LoginLoadingPanelVisibility = Visibility.Visible;
                NotifyPropertyChanged("LoginLoadingPanelVisibility");
                Task t = Task.Run(async () =>
                    {
                        await MainpageViewModel.instance.LoginTask;
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                loginInfo.LoginControlsEnabled = true;
                                loginInfo.UpdateStatus();
                                this.IsLoading = false;
                                LoginLoadingPanelVisibility = Visibility.Collapsed;
                                NotifyPropertyChanged("LoginLoadingPanelVisibility");
                            });

                    });
            }
        }

        public async Task Login()
        {
            this.IsLoading = true;
            loginInfo.LoginControlsEnabled = false;
            LoginLoadingPanelVisibility = Visibility.Visible;
            NotifyPropertyChanged("LoginLoadingPanelVisibility");
            await Task.Run(async () =>
                {
                    LoginInfoDataHandler.instance.UpdateLoginInfo(loginInfo);
                    await LoginInfoDataHandler.instance.Login(loginInfo);
                });

            loginInfo.LoginControlsEnabled = true;
            loginInfo.UpdateStatus();
            this.IsLoading = false;
            LoginLoadingPanelVisibility = Visibility.Collapsed;
            NotifyPropertyChanged("LoginLoadingPanelVisibility");
        }


    }
}
