using PownedLogic.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PownedUWP
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Frame.BackStack.Clear();
            this.DataContext = MainpageViewModel.instance;
           // SearchViewModel.instance.SetLoadingControl(SearchLoadingControl);

            //WindowsPhone only functions
           // StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            //ToastNotificationManager.History.Clear();

           // TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            //BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

            Task HeadlinesTask = Task.Run(() => HeadlinesViewModel.instance.LoadData());
            Task NewsTask = Task.Run(() => NewsViewModel.instance.LoadData());

           // SizeChanged += MainPage_SizeChanged;
        }
    }
}
