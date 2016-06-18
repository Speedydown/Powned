using BaseLogic.HtmlUtil;
using BaseLogic.Xaml_Controls.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic.Model
{
    public sealed class PopularHeadline : INewsLink
    {
        public string URL { get; private set; }
        public string Title { get; private set; }
        public string Date { get; private set; }

        public string ImageURL
        {
            get
            {
                return string.Empty;
            }
        }

        public string Content
        {
            get
            {
                return Title;
            }
        }

        public string CommentCount
        {
            get
            {
                return string.Empty;
            }
        }

        public string Time
        {
            get
            {
                return Date;
            }
        }

        public PopularHeadline(string Date, string URL, string Title)
        {
            this.Date = Date;
            this.URL = URL;
            this.Title = HTMLParserUtil.CleanHTMLTagsFromString(WebUtility.HtmlDecode(Title).Replace("\\", ""));
        }
    }
}
