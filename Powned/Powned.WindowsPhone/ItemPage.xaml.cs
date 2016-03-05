using Powned.Common;
using PownedLogic;
using PownedLogic.DataHandlers;
using PownedLogic.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
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
    public sealed partial class ItemPage : Page
    {
        public bool RefreshPage { get; private set; }
        public NewsItemViewModel ViewModel { get; private set; }
        private RelayCommand _checkedGoBackCommand;
        private NavigationHelper navigationHelper;

        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            _checkedGoBackCommand = new RelayCommand(
                                    () => this.CheckGoBack(),
                                    () => this.CanCheckGoBack()
                                );

            navigationHelper.GoBackCommand = _checkedGoBackCommand;
            RefreshPage = true;
        }

        private bool CanCheckGoBack()
        {
            return true;
        }

        private void CheckGoBack()
        {
            if (NewsItemControl.CanGoBack())
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.NavigationParameter != null)
            {
                NewsItemControl.DisableFullScreen = true;
                ViewModel = new NewsItemViewModel(LoadingControl);
                this.DataContext = ViewModel;
                await ViewModel.LoadState(e.NavigationParameter.ToString());
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            this.DataContext = null;
            RefreshPage = false;
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void PlaceCommentButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.PlaceCommentButton_Click(sender, e);

            await NewsItemViewModel.PlaceCommentTask;

            if (RefreshPage)
            {
                Frame.Navigate(typeof(ItemPage), ViewModel.URL);
            }
        }
    }
}
