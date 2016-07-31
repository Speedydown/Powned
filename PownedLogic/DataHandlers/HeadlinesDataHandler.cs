using BaseLogic.DataHandler;
using BaseLogic.HtmlUtil;
using HtmlAgilityPack;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace PownedLogic.DataHandlers
{
    public class HeadlinesDataHandler : DataHandler
    {
        public static readonly HeadlinesDataHandler instance = new HeadlinesDataHandler();

        private HeadlinesDataHandler()
            : base()
        {
            CreateItemTable<Headline>();
        }

        public List<Headline> GetNewHeadlines()
        {
            return GetItems<Headline>().Where(h => h.New == true).OrderByDescending(h => h.TimeStamp).ThenBy(h => h.InternalID).ToList();
        }

        public List<Headline> GetUnseenHeadlines()
        {
            return GetItems<Headline>().Where(h => h.Seen == false).OrderByDescending(h => h.TimeStamp).ThenBy(h => h.InternalID).ToList();
        }

        public async Task<IList<Headline>> GetLatestHeadlines(bool MarkHeadlinesAsSeen = false, int Limit = 20)
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("https://www.powned.tv/nieuws/", Encoding.GetEncoding("iso-8859-1"));
            Task<IList<Headline>> NewHeadlinesTask = Task.Run(() => GetHeadlinesFromSource(PageSource));

            MarkHeadlinesAsOld();

            List<Headline> HeadLines = (await NewHeadlinesTask) as List<Headline>;

            //Reverse list because the items from source are from new to old.
            HeadLines.Reverse();
            List<Headline> HeadlinesToAdd = new List<Headline>();
            List<Headline> CurrentHeadlines = base.GetItems<Headline>().ToList();

            foreach (Headline hl in HeadLines)
            {
                if (CurrentHeadlines.Where(h => h.URL == hl.URL ||
                    (h.Title == hl.Title && h.ImageURL == h.ImageURL)).Count() == 0)
                {
                    hl.New = true;
                    hl.Seen = false;
                    hl.TimeStamp = DateTime.Now;

                    HeadlinesToAdd.Add(hl);
                    CurrentHeadlines.Add(hl);

                    System.Diagnostics.Debug.WriteLine("[Headline]Adding " + hl.Title + " to localDB.");
                }
            }

            SaveItems(HeadlinesToAdd);

            if (MarkHeadlinesAsSeen)
            {
                this.MarkHeadlinesAsSeen();
            }

            var HeadlinesFromDB = base.GetItems<Headline>().OrderByDescending(h => h.TimeStamp).ThenByDescending(h => h.InternalID).Take(Limit).ToList();
            return HeadlinesFromDB;
        }

        private void MarkHeadlinesAsOld()
        {
            Headline[] Headlines = GetItems<Headline>().Where(h => h.New).ToArray();

            foreach (Headline hl in Headlines)
            {
                hl.New = false;
                System.Diagnostics.Debug.WriteLine("[Headline]Marking " + hl.Title + " as old");
            }

            SaveItems(Headlines);
        }

        private void MarkHeadlinesAsSeen()
        {
            Headline[] Headlines = GetItems<Headline>().Where(h => h.Seen == false).ToArray();

            foreach (Headline h in Headlines)
            {
                h.Seen = true;
                System.Diagnostics.Debug.WriteLine("[Headline]Marking " + h.Title + " as seen");
            }

            SaveItems(Headlines);
        }

        private IList<Headline> GetHeadlinesFromSource(string Source)
        {
            List<Headline> Headlines = new List<Headline>();

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(Source);

            if (htmlDoc.DocumentNode != null)
            {
                var HeadlineNodes = htmlDoc.DocumentNode.Descendants("article").Where(d => d.Attributes.Count(a => a.Value.Contains("medium-news-item")) > 0);

                foreach (HtmlNode Node in HeadlineNodes)
                {
                    string ImageUrl = Node.Descendants("img").FirstOrDefault().Attributes.FirstOrDefault(a => a.Name == "data-src").Value.Split('?').First() + "?anchor=center&mode=crop&width=75&height=75";

                    IEnumerable<HtmlNode> SpanNodes = Node.Descendants("span");
                    string HashTag = SpanNodes.FirstOrDefault(n => n.Attributes.Count(a => a.Value == "item-title__label label") > 0).InnerText;
                    string Title = SpanNodes.FirstOrDefault(n => n.Attributes.Count(a => a.Value == "item-title__heading__title") > 0).InnerText;
                    string Url = Node.Descendants("a").FirstOrDefault().Attributes.FirstOrDefault(a => a.Name == "href").Value;

                    Headlines.Add(new Headline(Url, ImageUrl, Title, HashTag));
                }
            }

            return Headlines;
        }
    }
}
