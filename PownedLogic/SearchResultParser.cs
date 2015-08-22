using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;

namespace PownedLogic
{
    public static class SearchResultParser
    {
        public static IList<SearchResult> GetSearchResult(string Source)
        {
            List<SearchResult> SearchResults = new List<SearchResult>();
            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<ul id=\"searchresults\">", Source, true));
            Source = Source.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("<div class=\"searchpagination\">", Source, true));

            while (true)
            {
                try
                {
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<a href=\"", Source, false));
                    string URL = HTMLParserUtil.GetContentAndSubstringInput("<a href=\"", "\">", Source, out Source, "", false);

                    string TitleEndTag = "</a>";

                    try
                    {
                        int EndTag1Index = HTMLParserUtil.GetPositionOfStringInHTMLSource("</a>", Source, false);
                        int Endtag2Index =  HTMLParserUtil.GetPositionOfStringInHTMLSource("<img", Source, false);

                        TitleEndTag = (EndTag1Index < Endtag2Index || Endtag2Index == -1)
                            ? "</a>" : "<img";
                    }
                    catch
                    {

                    }

                    string Title = HTMLParserUtil.GetContentAndSubstringInput("\">", TitleEndTag, Source, out Source, "", true);
                    string Content = HTMLParserUtil.GetContentAndSubstringInput("<p>", "</p>", Source, out Source, "", true);

                    SearchResults.Add(new SearchResult(URL, Title, Content));
                }
                catch
                {
                    break;
                }
            }

            return SearchResults;
        }
    }
}
