using BaseLogic.DataHandler;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
        public static readonly SemaphoreSlim NewsSemaphore = new SemaphoreSlim(1,1);

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
            await NewsSemaphore.WaitAsync();

            if (DateTime.Now.Subtract(LoginInfoDataHandler.instance.GetLoginInfo().LastNewsRetrival).TotalMinutes > 4)
            {
                MarkNewsLinksAsOld();
                string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));
                IList<INewsLink> NewslinksFromSource = GetNewsLinksFromSource(PageSource);

                foreach (NewsLink nl in NewslinksFromSource.Reverse())
                {
                    if (GetItems<NewsLink>().Where(n => n.URL == nl.URL).Count() == 0)
                    {
                        nl.New = true;
                        nl.TimeStamp = DateTime.Now;
                        lock (locker)
                        {
                            try
                            {
                                base.Insert(nl);
                            }
                            catch
                            {
                                //Double
                            }

                        }

                        LoginInfoDataHandler.instance.GetLoginInfo().UpdateLastNewsRetrival(DateTime.Now); 
                        System.Diagnostics.Debug.WriteLine("[NewsLink]Adding " + nl.Title + " to localDB.");
                    }
                }
            }

            var NewsLinksFromDB = base.GetItems<NewsLink>().OrderByDescending(h => h.TimeStamp).ThenByDescending(h => h.InternalID).Take(15).ToList();
            NewsSemaphore.Release();
            Task ClearDoubleItemsFromDBTask = Task.Run(() => ClearDoubleItemsFromDB());
            return NewsLinksFromDB.Cast<INewsLink>().ToList();
        }

        private async Task ClearDoubleItemsFromDB()
        {
            await NewsSemaphore.WaitAsync();

            int Counter = 0;
            var Newslinks = GetItems<NewsLink>().OrderByDescending(h => h.TimeStamp).ThenByDescending(h => h.InternalID);

            foreach (NewsLink nl in Newslinks)
            {
                Counter++;

                if (GetItems<NewsLink>().Where(n => n.URL == nl.URL).Count() == 2 || Counter > 40)
                {
                    DeleteItem<NewsLink>(nl);
                }
            }

            NewsSemaphore.Release();
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
                if (!Source.Contains("<li><span class=\"t\">"))
                {
                    break;
                }

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
