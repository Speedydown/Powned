﻿using BaseLogic;
using BaseLogic.Notifications;
using BaseLogic.Xaml_Controls;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
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

namespace PownedLogic.ViewModels
{
    public sealed class HeadlinesViewModel : ViewModel
    {
        public static readonly HeadlinesViewModel instance = new HeadlinesViewModel();

        public ObservableCollection<Headline> Headlines { get; private set; }
        private Task LoadDataTask = null;
        private object locker = new object();

        private HeadlinesViewModel()
        {
            Headlines = new ObservableCollection<Headline>();
        }

        private async Task<IList<Headline>> GetHeadlinesOperationAsTask()
        {
            try
            {
                return await HeadlinesDataHandler.instance.GetLatestHeadlines(true, 30);
            }
            catch
            {

            }

            DisplayError = true;

            return new List<Headline>();
        }

        public async Task LoadData(LoadingControl loadingControl = null)
        {
            lock (locker)
            {
                if (DateTime.Now.Subtract(LastLoadedTimeStamp).TotalMinutes > 5)
                {
                    LastLoadedTimeStamp = DateTime.Now;

                    if (this.loadingControl == null)
                    {
                        this.loadingControl = loadingControl;
                    }

                    LoadDataTask = LoadDataHelper();
                }
            }

            if (LoadDataTask != null && !LoadDataTask.IsCompleted)
            {
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

            try
            {
                NotificationHandler.Run("PownedBackground.BackgroundTask", "PownedBackgroundWorker", 30);
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
