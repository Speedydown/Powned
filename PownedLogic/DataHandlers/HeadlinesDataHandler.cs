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
            base.CreateTable(typeof(Headline));
        }

        public async Task<IList<Headline>> GetLatestHeadlines(string Source, int Limit = 30)
        {
            foreach (Headline hl in GetItems<Headline>().Where(h => h.New))
            {
                hl.New = false;
                base.Update(hl);
            }

            List<Headline> HeadLines = (await Task.Run(() => GetHeadlinesFromSource(Source))) as List<Headline>;

            foreach (Headline hl in HeadLines)
            {
                if (base.GetItems<Headline>().Where(h => h.URL == hl.URL || 
                    (h.Title == hl.Title && h.ImageURL == h.ImageURL)).Count() == 0)
                {
                    hl.New = true;
                    hl.TimeStamp = DateTime.Now;
                    base.Insert(hl);
                    System.Diagnostics.Debug.WriteLine("Adding " + hl.Title + " to localDB.");
                }
            }

            var HeadlinesFromDB = base.GetItems<Headline>().OrderByDescending(h => h.TimeStamp).ThenBy(h => h.InternalID).Take(Limit).ToList();

            foreach (Headline hl in HeadlinesFromDB)
            {
                System.Diagnostics.Debug.WriteLine("Returning " + hl.Title + " with timestamp " + hl.TimeStamp.ToString("hh:mm:ss.fff tt") + " from localDB.");
            }

            return HeadlinesFromDB;
        }

        private async Task<IList<Headline>> GetHeadlinesFromSource(string Source)
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
                    //try
                    //{
                    //    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //    {
                    //        Headlines.Add(new Headline(URL, ImageURL, Title, HashTag));
                    //    });
                    //}
                    //catch
                    //{
                    //    Headlines.Add(new Headline(URL, ImageURL, Title, HashTag));
                    //}
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
