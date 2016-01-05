using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;

namespace PownedLogic
{
    public static class PopularHeadlinesParser
    {
        public static IList<PopularHeadline> GetHeadlinesFromSource(string Source)
        {
            List<PopularHeadline> Headlines = new List<PopularHeadline>();

            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<ul class=\"headlines\" id=\"hl-populair\">", Source, true));

            while (true)
            {
                if (!Source.Contains("<li><span class=\"c\""))
                {
                    break;
                }

                try
                {
                    string CommentCount = HTMLParserUtil.GetContentAndSubstringInput("<li><span class=\"c\">", "</span>", Source, out Source, "", true);
                    string URL = HTMLParserUtil.GetContentAndSubstringInput("a href=\"", "\">", Source, out Source, "", false);
                    string Title = HTMLParserUtil.GetContentAndSubstringInput("\">", "</a>", Source, out Source, "", true);

                    Headlines.Add(new PopularHeadline(CommentCount, URL, Title));
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
