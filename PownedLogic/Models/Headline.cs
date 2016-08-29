using BaseLogic.DataHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SQLite.Net.Attributes;
using System.Net;
using BaseLogic.HtmlUtil;
using BaseLogic.Xaml_Controls.Interfaces;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using PownedLogic.DataHandlers;

namespace PownedLogic.Model
{
    public sealed class Headline : DataObject, INewsLink
    {
        public string URL { get; set; }
        public string ImageURL { get; set; }
        public Uri ImageUrl2
        {
            get
            {
                return new Uri(ImageURL);
            }
        }

        public string Title { get; set; }
        public string HashTag { get; set; }
        public bool New { get; set; }
        public bool Seen { get; set; }
        public DateTime TimeStamp
        {
            get; set;
        }

        [Ignore]
        public int Width
        {
            get
            {
                return WindowSizeManager.instance.ItemWidth;
            }
        }

        public Headline()
        {
            AttachEventHandler();
        }

        public async void AttachEventHandler()
        {
            WindowSizeManager.instance.SizeManagerUpdate += Instance_SizeManagerUpdate;
        }

        private void Instance_SizeManagerUpdate(object sender, WindowSizeChangedEventArgs e)
        {
            NotifyPropertyChanged("Width");
        }

        public Headline(string URL, string ImageURL, string Title, string HashTag)
        {
            this.URL = URL;
            this.ImageURL = Constants.Hostname + ImageURL;
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Title));
            this.HashTag = WebUtility.HtmlDecode(HashTag).Replace("&amp;", "&");

            AttachEventHandler();
        }

        public string Content
        {
            get { return string.Empty; }
        }

        public string CommentCount
        {
            get { return string.Empty; }
        }

        public string Time
        {
            get { return string.Empty; }
        }


    }
}
