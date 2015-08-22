using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;

namespace PownedLogic
{
    public static class HeadlinesParser
    {
        public static IList<Headline> GetHeadlinesFromSource(string Source)
        {
            List<Headline> Headlines = new List<Headline>();
            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<ul id=\"fpthumbs\">", Source, true));
            Source = Source.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("<div id=\"sidebar\">", Source, true));

            while (true)
            {
                try
                {
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<a href=\"", Source, false));
                    string URL = HTMLParserUtil.GetContentAndSubstringInput("<a href=\"", "\">", Source, out Source, "", true);
                    string ImageURL = HTMLParserUtil.GetContentAndSubstringInput("<img src=\"", "\" />", Source, out Source, "", true);

                    string TitleEndTag = "</span></h2>";

                    try
                    {
                        int EndTag1Index = HTMLParserUtil.GetPositionOfStringInHTMLSource("</span></h2>", Source, false);
                        int Endtag2Index = HTMLParserUtil.GetPositionOfStringInHTMLSource("<img", Source, false);

                        TitleEndTag = (EndTag1Index < Endtag2Index || Endtag2Index == -1)
                            ? "</span></h2>" : "<img";
                    }
                    catch
                    {

                    }

                    string Title = HTMLParserUtil.GetContentAndSubstringInput("<h2><span>", TitleEndTag, Source, out Source, "", true);
                    Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<span class=\"hashtag\">", Source, false));
                    string HashTag = HTMLParserUtil.GetContentAndSubstringInput("<span class=\"hashtag\">", "</span>", Source, out Source, "", true);

                    Headlines.Add(new Headline(URL, ImageURL, Title, HashTag));
                }
                catch
                {
                    break;
                }
            }

            return Headlines;
        }
    }
}
