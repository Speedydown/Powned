using Powned.Common;
using PownedLogic;
using PownedLogic.Model;
using PownedLogic.ViewModels;
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
        public static MainPage instance = null;

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
            NewsViewModel.instance.ClearCachedData();
            HeadlinesViewModel.instance.ClearCachedData();

            Task HeadlinesTask = Task.Run(() => HeadlinesViewModel.instance.LoadData(instance.LoadingControl));
            Task NewsTask = Task.Run(() => NewsViewModel.instance.LoadData(instance.LoadingControlActueel));
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Frame.BackStack.Clear();
            this.DataContext = MainpageViewModel.instance;
            SearchViewModel.instance.SetLoadingControl(SearchLoadingControl);

            //WindowsPhone only functions
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            ToastNotificationManager.History.Clear();

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

            Task HeadlinesTask = Task.Run(() => HeadlinesViewModel.instance.LoadData(LoadingControl));
            Task NewsTask = Task.Run(() => NewsViewModel.instance.LoadData(LoadingControlActueel));
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

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ClearCachedData();
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
            await Task.Run(() =>SearchViewModel.instance.Search());
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void PrivacyPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PrivacyPolicy));
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

    }
}
