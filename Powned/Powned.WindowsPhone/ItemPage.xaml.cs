using Powned.Common;
using PownedLogic;
using PownedLogic.DataHandlers;
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
        private static Task PlaceCommentTask = null;

        private string CurrentURL = string.Empty;
        private RelayCommand _checkedGoBackCommand;
        private NavigationHelper navigationHelper;
        public NewsItem newsItem { get; private set; }
        public LoginInfo loginInfo { get; private set; }
        public bool CommentingIsEnabled { get; private set; }
        public string CommentText { get; set; }
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
            try
            {
                LoadingControl.SetLoadingStatus(true);

                if (e.NavigationParameter != null)
                {
                    loginInfo = LoginInfoDataHandler.instance.GetLoginInfo();
                    NewsItemControl.DisableFullScreen = true;
                    CurrentURL = (string)e.NavigationParameter;
                    newsItem = await Datahandler.instance.GetNewsItemByURL(e.NavigationParameter.ToString()) as NewsItem;
                    LoadingControl.SetLoadingStatus(false);
                    this.DataContext = this;

                    if (newsItem.Comments.Count > 0)
                    {
                        CommentsGrid.Visibility = Visibility.Visible;
                    }

                    PlaceCommentPanel.Visibility = Visibility.Visible;

                    if (!(ApplicationData.Current.LocalSettings.Values["Reacties weergeven"] != null && Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["Reacties weergeven"])))
                    {
                        PlaceCommentPanel.Visibility = Visibility.Collapsed;
                    }

                    if ((PlaceCommentTask == null || PlaceCommentTask.IsCompleted) && LoginInfoDataHandler.instance.IsLoggedIn)
                    {
                        CommentTextbox.IsEnabled = true;
                        PlaceCommentButton.IsEnabled = true;
                    }
                    else if (!LoginInfoDataHandler.instance.IsLoggedIn)
                    {
                        PlaceCommentButton.Visibility = Visibility.Collapsed;
                        CommentTextbox.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        CommentTextbox.IsEnabled = false;
                        await PlaceCommentTask;
                        CommentTextbox.IsEnabled = true;
                        PlaceCommentButton.IsEnabled = true;
                    }
                }
            }
            catch
            {
                LoadingControl.DisplayLoadingError(true);
            }
            finally
            {
                LoadingControl.SetLoadingStatus(false);
            }

            await ArticleCounter.AddArticleCount();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            this.DataContext = null;
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
            if (CommentText != null && CommentText.Length > 3)
            {
                PlaceCommentButton.Visibility = Visibility.Collapsed;
                CommentTextbox.Visibility = Visibility.Collapsed;
                PlaceCommentTask = LoginInfoDataHandler.instance.PlaceComment(CommentText, newsItem.entry_id);
                CommentText = string.Empty;
                await PlaceCommentTask;
            }
        }
    }
}
