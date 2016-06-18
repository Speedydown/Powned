using BaseLogic.HtmlUtil;
using HtmlAgilityPack;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic
{
    public static class PopularHeadlinesParser
    {
        public static IList<PopularHeadline> GetHeadlinesFromSource(string Source)
        {
            List<PopularHeadline> Headlines = new List<PopularHeadline>();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(Source);

            if (htmlDoc.DocumentNode != null)
            {
                var HeadlineNode = htmlDoc.DocumentNode.Descendants("div").Where(d => d.Attributes.Count(a => a.Value.Contains("page-sidebar__section--mostread")) > 0).FirstOrDefault();

                var HeadlineNodes = HeadlineNode.Descendants("li").Where(d => d.Attributes.Count(a => a.Value.Contains("list-item")) > 0);

                foreach (HtmlNode Node in HeadlineNodes)
                {
                    IEnumerable<HtmlNode> SpanNodes = Node.Descendants("span");
                    string Date = SpanNodes.FirstOrDefault(n => n.Attributes.Count(a => a.Value == "content-date") > 0).InnerText;
                    string Title = SpanNodes.FirstOrDefault(n => n.Attributes.Count(a => a.Value == "content-title") > 0).InnerText;
                    string Url = Node.Descendants("a").FirstOrDefault().Attributes.FirstOrDefault(a => a.Name == "href").Value;

                    Headlines.Add(new PopularHeadline(Date, Url, Title));
                }
            }

            return Headlines;
        }
    }
}
