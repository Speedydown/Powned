using BaseLogic;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlControlLibrary;

namespace PownedLogic.ViewModels
{
    public class NewsItemViewModel : ViewModel
    {
        public static Task PlaceCommentTask = null;

        public NewsItem newsItem { get; private set; }
        public LoginInfo loginInfo { get; private set; }
        public string CommentText { get; set; }
        public string URL { get; private set; }

        public Visibility PlaceCommentProgressRingVisibility { get; private set; }
        public Visibility CommentsGridVisiblity { get; private set; }
        public Visibility PlaceCommentPanel { get; private set; }
        public bool CommentControlsEnabled { get; private set; }

        public NewsItemViewModel(LoadingControl loadingControl) : base(loadingControl)
        {
            PlaceCommentProgressRingVisibility = Visibility.Collapsed;
            PlaceCommentPanel = Visibility.Collapsed;
            CommentsGridVisiblity = Visibility.Collapsed;
            CommentControlsEnabled = false;

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }

        public async Task LoadState(string URL)
        {
            this.URL = URL;
            IsLoading = true;

            try
            {
                loginInfo = LoginInfoDataHandler.instance.GetLoginInfo();
                loginInfo.UpdateStatus();

                newsItem = await Datahandler.instance.GetNewsItemByURL(URL) as NewsItem;

                if (newsItem.Comments.Count > 0)
                {
                    CommentsGridVisiblity = Visibility.Visible;
                    NotifyPropertyChanged("CommentsGridVisiblity");
                }

                if (ApplicationData.Current.LocalSettings.Values["Reacties weergeven"] != null && Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["Reacties weergeven"]))
                {
                    PlaceCommentPanel = Visibility.Visible;
                    NotifyPropertyChanged("PlaceCommentPanel");
                }

                if ((PlaceCommentTask == null || PlaceCommentTask.IsCompleted) && LoginInfoDataHandler.instance.IsLoggedIn)
                {
                    CommentControlsEnabled = true;
                    NotifyPropertyChanged("CommentControlsEnabled");
                }
                else if (!LoginInfoDataHandler.instance.IsLoggedIn)
                {
                    PlaceCommentPanel = Visibility.Collapsed;
                    NotifyPropertyChanged("PlaceCommentPanel");
                }
                else
                {
                    CommentControlsEnabled = false;
                    NotifyPropertyChanged("CommentControlsEnabled");
                    await PlaceCommentTask;
                    CommentControlsEnabled = true;
                    NotifyPropertyChanged("CommentControlsEnabled");
                }
            }
            catch
            {
                DisplayError = true;
            }
            finally
            {
                NotifyPropertyChanged("newsItem");
                NotifyPropertyChanged("loginInfo");
                IsLoading = false;
            }
        }

        public async Task PlaceCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommentText != null && CommentText.Length > 3)
            {
                PlaceCommentProgressRingVisibility = Visibility.Visible;
                NotifyPropertyChanged("PlaceCommentProgressRingVisibility");
                PlaceCommentPanel = Visibility.Collapsed;
                NotifyPropertyChanged("PlaceCommentPanel");
                
                PlaceCommentTask = LoginInfoDataHandler.instance.PlaceComment(CommentText, newsItem.entry_id);
                CommentText = string.Empty;
                await PlaceCommentTask;
            }
        }
    }
}
