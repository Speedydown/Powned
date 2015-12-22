using BaseLogic.DataHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.Storage;
using Windows.UI.Core;
using WRCHelperLibrary;

namespace PownedLogic.DataHandlers
{
    public class NewsLinksDataHandler : DataHandler
    {
        public static readonly NewsLinksDataHandler instance = new NewsLinksDataHandler();

        private NewsLinksDataHandler()
            : base()
        {
            lock (locker)
            {
                int Result = base.CreateTable(typeof(NewsLink));
            }
        }

        public async Task<IList<INewsLink>> GetNewsLinks()
        {
            MarkNewsLinksAsOld();
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));
            IList<INewsLink> NewslinksFromSource = GetNewsLinksFromSource(PageSource);

            foreach (NewsLink nl in NewslinksFromSource)
            {
                if (base.GetItems<NewsLink>().Where(n => n.URL == nl.URL ||
                    (n.Title == nl.Title && n.ImageURL == nl.ImageURL)).Count() == 0)
                {
                    nl.New = true;
                    nl.TimeStamp = DateTime.Now;
                    lock (locker)
                    {
                        base.Insert(nl);
                    }
                    System.Diagnostics.Debug.WriteLine("[NewsLink]Adding " + nl.Title + " to localDB.");
                }
            }

            var NewsLinksFromDB = base.GetItems<NewsLink>().OrderByDescending(h => h.Time).Take(15).ToList();

            await Task.Delay(2000);

            foreach (NewsLink n in NewsLinksFromDB)
            {
                System.Diagnostics.Debug.WriteLine("[NewsLink]" + n.Title + " " + n.TimeStamp + " " + n.InternalID);
            }

            if (NewsLinksFromDB == null)
            {
                return NewslinksFromSource;
            }

            return NewsLinksFromDB.Cast<INewsLink>().ToList();
        }

        private void MarkNewsLinksAsOld()
        {
            foreach (NewsLink nl in GetItems<NewsLink>().Where(n => n.New))
            {
                nl.New = false;
                lock (locker)
                {
                    Update(nl);
                }
                System.Diagnostics.Debug.WriteLine("[NewsLink]Marking " + nl.Title + " as old");
            }
        }

        private IList<INewsLink> GetNewsLinksFromSource(string Source)
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
