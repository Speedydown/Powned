using PownedLogic.DataHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WRCHelperLibrary;
using XamlControlLibrary.Interfaces;

namespace PownedLogic.Model
{
    public sealed class NewsItem : INewsItem
    {
        public string Title { get; private set; }
        public string Added { get; private set; }
        public string Updated { get; private set; }
        public string TimeStamp { get; private set; }
        public string ContentSummary { get; private set; }
        public IList<string> Body { get; private set; }
        public IList<string> ImageList { get; private set; }
        public string Author { get; private set; }
        public Uri YoutubeURL { get; private set; }
        public string entry_id { get; private set; }

        public Brush TitleColor
        {
            get
            {
                return new SolidColorBrush(Color.FromArgb((byte)255, (byte)255, (byte)0, (byte)153));
            }
        }

        public Brush TitleColorWindows
        {
            get { return new SolidColorBrush(Color.FromArgb((byte)255, (byte)255, (byte)0, (byte)153)); }
        }

        public Visibility MediaVisibilty
        {
            get { return Visibility.Collapsed; }
        }

        public Windows.UI.Xaml.Visibility SummaryVisibilty
        {
            get { return Visibility.Visible; }
        }

        public Windows.UI.Xaml.Visibility TimeStampVisibilty
        {
            get { return Visibility.Collapsed; }
        }

        private Thickness _ContentMargins = new Thickness(4, 15, 4, 5);
        public Thickness ContentMargins
        {
            get
            {
                return _ContentMargins;
            }
        }

        public Uri MediaFile { get; private set; }

        public Visibility DisplayWebView
        {
            get { return YoutubeURL != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public IList<Comment> Comments { get; private set; }

        public NewsItem(string Title, string Summary, IList<string> ArticleContent, string Date, string AuthorDate,  string ArticleImage, IList<Comment> Comments, IList<string> Images, string Youtube, string entry_id)
        {
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(Title);
            this.ContentSummary =  HTMLParserUtil.CleanHTMLTagsFromString(Summary);
            this.Body = ArticleContent;
            this.Added = Date;
            this.Author = AuthorDate;

            if (Youtube != null && Youtube.Length > 0)
            this.YoutubeURL = new Uri(Youtube);

            this.ImageList = Images;
            this.Comments = Comments;
            this.TimeStamp = "";
            this.Updated = "";
            this.MediaFile = null;
            this.entry_id = entry_id;
        }
    }

    public sealed class Comment
    {
        public string Content { get; private set; }
        public string AuthorDateTime { get; private set; }

        public Comment(string Content, string AuthorDateTIme)
        {
            this.Content = Content;

            while (this.Content.EndsWith("\n"))
            {
                this.Content = this.Content.Substring(0, this.Content.Length - 1);
            }

            if (this.Content.Contains("#"))
            {
                this.Content = WebUtility.HtmlDecode(this.Content);
            }

            this.Content = HTMLParserUtil.CleanHTMLTagsFromString(this.Content);

            this.AuthorDateTime = HTMLParserUtil.CleanHTMLTagsFromString(AuthorDateTIme); ;
        }
    }
}
