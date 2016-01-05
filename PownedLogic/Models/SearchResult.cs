using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;

namespace PownedLogic.Model
{
    public sealed class SearchResult
    {
        public string URL { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }

        public SearchResult(string URL, string Title, string Content)
        {
            this.URL = URL;
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(Title);
            this.Content = HTMLParserUtil.CleanHTMLTagsFromString(Content); ;
        }
    }
}
