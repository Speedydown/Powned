using BaseLogic;
using BaseLogic.ArticleCounter;
using BaseLogic.ClientIDHandler;
using BaseLogic.Xaml_Controls;
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

namespace PownedLogic.ViewModels
{
    public class NewsItemViewModel : ViewModel
    {
        public NewsItem newsItem { get; private set; }
        public string CommentText { get; set; }
        public string URL { get; private set; }

        public Visibility PlaceCommentProgressRingVisibility { get; private set; }
        public Visibility CommentsGridVisiblity { get; private set; }
        public bool CommentControlsEnabled { get; private set; }

        public NewsItemViewModel(LoadingControl loadingControl) : base(loadingControl)
        {
            PlaceCommentProgressRingVisibility = Visibility.Collapsed;
            CommentsGridVisiblity = Visibility.Collapsed;
            CommentControlsEnabled = false;

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

            Task ArticleCounterTask = ArticleCounter.AddArticleCount("Wij bieden PowNed kostenloos aan en we zouden het op prijs stellen als u de PowNed app een positieve review geeft in de Windows store.", "Bedankt");
            Task Notifier = Task.Run(async () => await ClientIDHandler.instance.PostAppStats(ClientIDHandler.AppName.Powned));
        }

        public async Task LoadState(string URL)
        {
            this.URL = URL;
            IsLoading = true;

            try
            {
                newsItem = await Datahandler.instance.GetNewsItemByURL(URL) as NewsItem;

                if (newsItem.Comments.Count > 0)
                {
                    CommentsGridVisiblity = Visibility.Visible;
                    NotifyPropertyChanged("CommentsGridVisiblity");
                }
            }
            catch
            {
                DisplayError = true;
            }
            finally
            {
                NotifyPropertyChanged("newsItem");
                IsLoading = false;
            }
        }
    }
}
