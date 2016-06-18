using BaseLogic;
using BaseLogic.Xaml_Controls;
using BaseLogic.Xaml_Controls.Interfaces;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace PownedLogic.ViewModels
{
    public sealed class NewsViewModel : ViewModel
    {
        public static readonly NewsViewModel instance = new NewsViewModel();

        public ObservableCollection<PopularHeadline> PopularHeadlines { get; private set; }

        private Task LoadDataTask = null;

        private NewsViewModel()
        {
            PopularHeadlines = new ObservableCollection<PopularHeadline>();
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

        public async Task LoadData(LoadingControl loadingControl = null)
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
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PopularHeadlines.Clear();
                    this.DisplayError = false;
                    this.IsLoading = true;
                });

                Task<IList<PopularHeadline>> PopularHeadlinesTask = GetPopularHeadlinesOperationAsTask();

                var Result2 = await PopularHeadlinesTask;

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PopularHeadlines.Clear();
                    this.IsLoading = false;

                    foreach (PopularHeadline ph in Result2)
                    {
                        PopularHeadlines.Add(ph);
                    }
                });

                LastLoadedTimeStamp = DateTime.Now;

                if (Result2.Count == 0)
                {
                    throw new Exception("No items");
                }

                IsLoading = false;
            }
            catch
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        IsLoading = false;
                        DisplayError = true;
                    });
            }
        }

        public void ClearCachedData()
        {
            LastLoadedTimeStamp = DateTime.Now.AddDays(-1);
        }
    }
}
