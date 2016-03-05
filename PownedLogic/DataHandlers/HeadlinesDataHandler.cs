using BaseLogic.DataHandler;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
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

        public async Task<IList<Headline>> GetLatestHeadlines(bool MarkHeadlinesAsSeen = false, int Limit = 30)
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv", Encoding.GetEncoding("iso-8859-1"));
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
            Source = Source.Substring(HTMLParserUtil.GetPositionOfStringInHTMLSource("<ul id=\"fpthumbs\">", Source, true));
            Source = Source.Substring(0, HTMLParserUtil.GetPositionOfStringInHTMLSource("<div id=\"sidebar\">", Source, true));

            while (true)
            {
                try
                {
                    if (!Source.Contains("<a href=\""))
                    {
                        break;
                    }

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
