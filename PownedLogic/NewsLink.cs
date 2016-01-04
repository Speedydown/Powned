using BaseLogic.DataHandler;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using WRCHelperLibrary;

namespace PownedLogic
{
    public sealed class NewsLink : DataObject, INewsLink
    {
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
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(Title.Replace("\\", "")); ;
            this.Time = Time;
            this.URL = URL;
        }
    }
}
