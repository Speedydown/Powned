using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;

namespace PownedLogic.Model
{
    public sealed class PopularHeadline
    {
        public string CommentCount { get; private set; }
        public string URL { get; private set; }
        public string Title { get; private set; }

        public PopularHeadline(string CommentCount, string URL, string Title)
        {
            this.CommentCount = CommentCount;
            this.URL = URL;
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Title).Replace("\\", ""));
        }
    }
}
