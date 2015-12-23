using BaseLogic;
using PownedLogic.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using WRCHelperLibrary.Settings;
using XamlControlLibrary;

namespace PownedLogic.ViewModels
{
    public class SettingsViewModel : ViewModel
    {
        public static readonly SettingsViewModel instance = new SettingsViewModel();

        public LoginInfo loginInfo { get; private set; }
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
            this.loginInfo = LoginInfoDataHandler.instance.GetLoginInfo();

            if (MainpageViewModel.instance.LoginTask != null && !MainpageViewModel.instance.LoginTask.IsCompleted)
            {
                this.IsLoading = true;
                loginInfo.LoginControlsEnabled = false;
                Task t = Task.Run(async () =>
                    {
                        await MainpageViewModel.instance.LoginTask;
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                loginInfo.LoginControlsEnabled = true;
                                loginInfo.UpdateStatus();
                                this.IsLoading = false;
                            });

                    });
            }
        }

        public async Task Login()
        {
            this.IsLoading = true;
            loginInfo.LoginControlsEnabled = false;

            await Task.Run(async () =>
                {
                    LoginInfoDataHandler.instance.UpdateLoginInfo(loginInfo);
                    await LoginInfoDataHandler.instance.Login(loginInfo);
                });

            loginInfo.LoginControlsEnabled = true;
            loginInfo.UpdateStatus();
            this.IsLoading = false;
        }


    }
}
