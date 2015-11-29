using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WRCHelperLibrary;
using XamlControlLibrary;

namespace PownedLogic.ViewModels
{
    public sealed class NewsViewModel : ViewModel
    {
        public static readonly NewsViewModel instance = new NewsViewModel();

        public ObservableCollection<INewsLink> NewsLinks { get; private set; }
        public ObservableCollection<PopularHeadline> PopularHeadlines { get; private set; }

        private Visibility _HeadersVisibility;
        public Visibility HeadersVisibility
        {
            get { return _HeadersVisibility; }
            set
            {
                _HeadersVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private Task LoadDataTask = null;

        private NewsViewModel()
        {
            HeadersVisibility = Visibility.Collapsed;
            NewsLinks = new ObservableCollection<INewsLink>();
            PopularHeadlines = new ObservableCollection<PopularHeadline>();
        }

        private async Task<IList<INewsLink>> GetNewsLinksOperationAsTask()
        {
            try
            {
                return await Datahandler.instance.GetNewsLinksByPage(0);
            }
            catch
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DisplayError = true;
                });

                return new List<INewsLink>();
            }
        }

        private async Task<IList<PopularHeadline>> GetPopularHeadlinesOperationAsTask()
        {
            try
            {
                return await Datahandler.instance.GetPopularNewsLinks();
            }
            catch
            {
                return new List<PopularHeadline>();
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
                HeadersVisibility = Visibility.Collapsed;
                NewsLinks.Clear();
                PopularHeadlines.Clear();
                this.IsLoading = true;
            });

            Task<IList<INewsLink>> NewsLinkTask = GetNewsLinksOperationAsTask();
            Task<IList<PopularHeadline>> PopularHeadlinesTask = GetPopularHeadlinesOperationAsTask();

            var Result = await NewsLinkTask;
            var Result2 = await PopularHeadlinesTask;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.IsLoading = false;

                foreach (INewsLink n in Result)
                {
                    NewsLinks.Add(n);
                }

                foreach (PopularHeadline ph in Result2)
                {
                    PopularHeadlines.Add(ph);
                }

                if (NewsLinks.Count > 0)
                {
                    HeadersVisibility = Visibility.Visible;
                }
            });

            LastLoadedTimeStamp = DateTime.Now;
            SetLastNewsItemAsNotificationMarker();
        }

        private void SetLastNewsItemAsNotificationMarker()
        {
            try
            {
                localSettings.Values["LastActualNewsItem"] = NewsLinks.First().URL;
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
