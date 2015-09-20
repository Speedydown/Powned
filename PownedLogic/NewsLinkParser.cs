using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WebCrawlerTools;
using WRCHelperLibrary;

namespace PownedLogic
{
    static class NewsLinkParser
    {
        public static IList<INewsLink> GetNewsLinksFromSource(string Source)
        {
            List<INewsLink> NewsLinks = new List<INewsLink>();

            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("id=\"hl-actueel\">", Source, true));

            while (true)
            {
                try
                {
                    string Time = HTMLParserUtil.GetContentAndSubstringInput("<li><span class=\"t\">", "</span>", Source, out Source, "", true);
                    string URL = HTMLParserUtil.GetContentAndSubstringInput("a href=\"", "\">", Source, out Source, "", false);
                    string Title = HTMLParserUtil.GetContentAndSubstringInput("\">", "</a>", Source, out Source, "", true);

                    NewsLinks.Add(new NewsLink(Title, Time, URL));
                }
                catch
                {
                    break;
                }
            }

            return NewsLinks;
        }
    }
}
