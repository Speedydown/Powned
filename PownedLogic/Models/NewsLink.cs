using BaseLogic.DataHandler;
using BaseLogic.HtmlUtil;
using BaseLogic.Xaml_Controls.Interfaces;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic.Model
{
    public sealed class NewsLink : DataObject, INewsLink
    {
        [Unique][NotNull]
        public string URL { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool New { get; set; }

        [Ignore]
        public string ImageURL
        {
            get { return string.Empty; }
        }

        [Ignore]
        public string Content
        {
            get
            {
                return string.Empty;
            }
        }

        [Ignore]
        public string CommentCount
        {
            get
            {
                return string.Empty;
            }
        }

        public NewsLink()
        {

        }

        public NewsLink(string Title, string Time, string URL)
        {
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Title.Replace("\\", "")));
            this.Time = Time;
            this.URL = URL;
        }
    }
}
