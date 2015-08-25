using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using WRCHelperLibrary;

namespace PownedLogic
{
    public sealed class NewsLink : INewsLink
    {
        public string URL { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public string Time { get; private set; }
        public string Catagory { get; private set; }
        public string CommentCount { get; private set; }

        public string ImageURL
        {
            get { return string.Empty; }
        }

        public NewsLink(string Title, string Time, string URL)
        {
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(Title.Replace("\\", "")); ;
            this.Time = Time;
            this.URL = URL;
            //this.Catagory = Catagory;
            //this.Time = Date.Trim().Split(' ').Last();
            //this.CommentCount = CommentCount;
        }
    }
}
