using BaseLogic.DataHandler;
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

        private HeadlinesDataHandler() : base()
        {
            lock (locker)
            {
                int Result = base.CreateTable(typeof(Headline));
            }
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

            HeadLines.Reverse();

            foreach (Headline hl in HeadLines)
            {
                if (base.GetItems<Headline>().Where(h => h.URL == hl.URL || 
                    (h.Title == hl.Title && h.ImageURL == h.ImageURL)).Count() == 0)
                {
                    hl.New = true;
                    hl.Seen = false;
                    hl.TimeStamp = DateTime.Now;

                    lock (locker)
                    {
                        base.Insert(hl);
                    }

                    System.Diagnostics.Debug.WriteLine("[Headline]Adding " + hl.Title + " to localDB.");
                }
            }

            if (MarkHeadlinesAsSeen)
            {
                this.MarkHeadlinesAsSeen();
            }

            var HeadlinesFromDB = base.GetItems<Headline>().OrderByDescending(h => h.TimeStamp).ThenByDescending(h => h.InternalID).Take(Limit).ToList();
            return HeadlinesFromDB;
        }

        private void MarkHeadlinesAsOld()
        {
            foreach (Headline hl in GetItems<Headline>().Where(h => h.New))
            {
                hl.New = false;
                lock (locker)
                {
                    base.Update(hl);
                }
                System.Diagnostics.Debug.WriteLine("[Headline]Marking " + hl.Title + " as old");
            }
        }

        private void MarkHeadlinesAsSeen()
        {
            foreach (Headline h in GetItems<Headline>().Where(h => h.Seen == false))
            {
                h.Seen = true;
                lock (locker)
                {
                    Update(h);
                }
                System.Diagnostics.Debug.WriteLine("[Headline]Marking " + h.Title + " as seen");
            }
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
