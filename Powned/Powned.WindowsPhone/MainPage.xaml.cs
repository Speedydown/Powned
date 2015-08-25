using Powned.Common;
using PownedLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WRCHelperLibrary;

namespace Powned
{
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private static IList<NewsLink> NewsLinks = null;
        private static IList<Headline> Headlines = null;
        private static IList<PopularHeadline> PopularHeadlines = null;
        public static MainPage instance = null;

        public static DateTime TimeLoaded = DateTime.Now.AddDays(-1);

        public MainPage()
        {
            this.InitializeComponent();
            instance = this;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public static void ClearCachedData()
        {
            TimeLoaded = DateTime.Now.AddDays(-1);
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

            if (NewsLinks == null || DateTime.Now.Subtract(TimeLoaded).TotalMinutes > 5)
            {
                await LoadData();
            }

            if (e.NavigationParameter != null && e.NavigationParameter.ToString() != "")
            {
                try
                {
                    Frame.Navigate(typeof(ItemPage), (e.NavigationParameter));
                    return;
                }
                catch
                {

                }
            }
        }

        private async Task<IList<NewsLink>> GetNewsLinksOperationAsTask()
        {
            try
            {
                return await Datahandler.GetNewsLinksByPage();
            }
            catch
            {
                LoadingControlActueel.DisplayLoadingError(true);
                return new List<NewsLink>();
            }
        }

        private async Task<IList<Headline>> GetHeadlinesOperationAsTask()
        {
            try
            {
                return await Datahandler.GetHeadlines();
            }
            catch
            {
                LoadingControl.DisplayLoadingError(true);
                return new List<Headline>();
            }
        }

        private async Task<IList<PopularHeadline>> GetPopularHeadlinesOperationAsTask()
        {
            try
            {
                return await Datahandler.GetPopularNewsLinks();
            }
            catch
            {
                return new List<PopularHeadline>();
            }
        }

        public async Task LoadData()
        {
            PopularTextblock.Visibility = Visibility.Collapsed;
            ActueelTextblock.Visibility = Visibility.Collapsed;
            LoadingControl.DisplayLoadingError(false);
            LoadingControlActueel.DisplayLoadingError(false);
            LoadingControl.SetLoadingStatus(true);
            LoadingControlActueel.SetLoadingStatus(true);
            SearchLoadingControl.SetLoadingStatus(false);
            HeadlinesListview.ItemsSource = null;
            ActueelListview.ItemsSource = null;
            PopularListview.ItemsSource = null;

            Task<IList<NewsLink>> GetNewsLinksTask = GetNewsLinksOperationAsTask();
            Task<IList<Headline>> GetHeadlinesTask = GetHeadlinesOperationAsTask();
            Task<IList<PopularHeadline>> GetPopularHeadlinesTask = GetPopularHeadlinesOperationAsTask();

            Headlines = await GetHeadlinesTask;
            LoadingControl.SetLoadingStatus(false);
            HeadlinesListview.ItemsSource = Headlines;

            NewsLinks = await GetNewsLinksTask;
            ActueelListview.ItemsSource = NewsLinks;

            PopularHeadlines = await GetPopularHeadlinesTask;
            PopularListview.ItemsSource = PopularHeadlines;
            PopularTextblock.Visibility = Visibility.Visible;
            ActueelTextblock.Visibility = Visibility.Visible;
            LoadingControlActueel.SetLoadingStatus(false);

            TimeLoaded = DateTime.Now;

            ApplicationData applicationData = ApplicationData.Current;
            ApplicationDataContainer localSettings = applicationData.LocalSettings;

            try
            {
                localSettings.Values["LastNewsItem"] = NewsLinks.First().URL;
                NotificationHandler.Run("PownedBackgroundWP.BackgroundTask", "PownedBackgroundWorker", 30);
            }
            catch
            {

            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void PrivacyPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://wiezitwaarvandaag.nl/privacypolicy.aspx"));
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadData();

            LoadingControl.SetLoadingStatus(false);
        }

        private void SearchTextbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Task Search = this.Search();

                var control = sender as Control;
                var isTabStop = control.IsTabStop;
                control.IsTabStop = false;
                control.IsEnabled = false;
                control.IsEnabled = true;
                control.IsTabStop = isTabStop;
            }
        }

        private async Task Search()
        {
            SearchListView.ItemsSource = null;
            SearchLoadingControl.DisplayLoadingError(false);
            SearchLoadingControl.SetLoadingStatus(true);

            try
            {
                SearchListView.ItemsSource = await Datahandler.Search(SearchTextbox.Text);
            }
            catch
            {
                SearchLoadingControl.DisplayLoadingError(true);
            }

            SearchLoadingControl.SetLoadingStatus(false);
        }

        private void PownedPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PownedPivot.SelectedItem == ZoekenPI)
            {
                ReloadButton.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Visible;

            }
            else
            {
                ReloadButton.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Task Search = this.Search();

            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        private void HeadlinesListview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemPage), (e.ClickedItem as Headline).URL);
        }

        private async void PownedButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.powned.tv/"));
        }

        private void ActueelListview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemPage), (e.ClickedItem as NewsLink).URL);
        }

        private void PopularListview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemPage), (e.ClickedItem as PopularHeadline).URL);
        }

        private void SearchListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemPage), (e.ClickedItem as SearchResult).URL);
        }
    }
}
