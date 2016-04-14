using BaseLogic.HtmlUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Title));
            this.Content = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Content));
        }
    }
}
