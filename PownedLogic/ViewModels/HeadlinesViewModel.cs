using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using WRCHelperLibrary;
using XamlControlLibrary;

namespace PownedLogic.ViewModels
{
    public sealed class HeadlinesViewModel : ViewModel
    {
        public static readonly HeadlinesViewModel instance = new HeadlinesViewModel();

        public ObservableCollection<Headline> Headlines { get; private set; }
        private Task LoadDataTask = null;

        private HeadlinesViewModel()
        {
            Headlines = new ObservableCollection<Headline>();
        }

        private async Task<IList<Headline>> GetHeadlinesOperationAsTask()
        {
            try
            {
                return await Datahandler.instance.GetHeadlines();
            }
            catch
            {
               CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DisplayError = true;
                });

                return new List<Headline>();
            }
        }

        public async Task LoadData(LoadingControl loadingControl)
        {
            if (LoadDataTask == null || DateTime.Now.Subtract(LastLoadedTimeStamp).TotalMinutes > 5)
            {
                if (this.loadingControl == null)
                {
                    this.loadingControl = loadingControl;
                }

                LoadDataTask = LoadDataHelper();
                await LoadDataTask;
            }
        }

        private async Task LoadDataHelper()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Headlines.Clear();
                this.IsLoading = true;
            });

            var Result = await GetHeadlinesOperationAsTask();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.IsLoading = false;

                    foreach (Headline h in Result)
                    {
                        Headlines.Add(h);
                    }
                });

            LastLoadedTimeStamp = DateTime.Now;
            SetLastNewsItemAsNotificationMarker();
        }

        private void SetLastNewsItemAsNotificationMarker()
        {
            try
            {
                localSettings.Values["LastNewsItem"] = Headlines.First().URL;
                NotificationHandler.Run("PownedBackgroundWP.BackgroundTask", "PownedBackgroundWorker", 30);
            }
            catch
            {

            }

            try
            {
                localSettings.Values["LastNotificationHeadlines"] = Headlines.First().URL;
            }
            catch
            {

            }
        }

        public void ClearCachedData()
        {
            LastLoadedTimeStamp = DateTime.Now.AddDays(-1);
        }
    }
}
