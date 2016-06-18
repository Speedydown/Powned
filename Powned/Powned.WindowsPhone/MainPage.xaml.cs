using Powned.Common;
using PownedLogic;
using PownedLogic.Model;
using PownedLogic.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Powned
{
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public static MainPage instance = null;

        public MainPage()
        {
            InitializeComponent();
            instance = this;
            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += this.NavigationHelper_LoadState;
            navigationHelper.SaveState += this.NavigationHelper_SaveState;
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }

        public static void ClearCachedData()
        {
            NewsViewModel.instance.ClearCachedData();
            HeadlinesViewModel.instance.ClearCachedData();

            Task HeadlinesTask = Task.Run(() => HeadlinesViewModel.instance.LoadData(instance.LoadingControl));
            Task NewsTask = Task.Run(() => NewsViewModel.instance.LoadData(instance.LoadingControlActueel));
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Frame.BackStack.Clear();
            this.DataContext = MainpageViewModel.instance;      

            //WindowsPhone only functions
            StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            ToastNotificationManager.History.Clear();

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

            Task HeadlinesTask = Task.Run(() => HeadlinesViewModel.instance.LoadData(LoadingControl));
            Task NewsTask = Task.Run(() => NewsViewModel.instance.LoadData(LoadingControlActueel));
            Task MessageServiceTask = MessageService.instance.DisplayInfoMessage();

            await StatusBar.GetForCurrentView().HideAsync();
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

        private void HeadlinesListview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemPage), (e.ClickedItem as Headline).URL);
        }

        private void PopularListview_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemPage), (e.ClickedItem as PopularHeadline).URL);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void PrivacyPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PrivacyPolicy));
        }
    }
}
