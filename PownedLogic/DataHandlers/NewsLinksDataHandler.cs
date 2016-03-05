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
using XamlControlLibrary.Interfaces;

namespace PownedLogic.DataHandlers
{
    public class NewsLinksDataHandler : DataHandler
    {
        public static readonly NewsLinksDataHandler instance = new NewsLinksDataHandler();
        public static readonly SemaphoreSlim NewsSemaphore = new SemaphoreSlim(1, 1);

        private NewsLinksDataHandler()
            : base()
        {
            CreateItemTable<NewsLink>();
        }

        public async Task<IList<INewsLink>> GetNewsLinks()
        {
            if (DateTime.Now.Subtract(LoginInfoDataHandler.instance.GetLoginInfo().LastNewsRetrival).TotalMinutes > 4)
            {
                string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));
                Task<IList<INewsLink>> NewsLinkTask = Task.Run<IList<INewsLink>>(() => GetNewsLinksFromSource(PageSource));

                await NewsSemaphore.WaitAsync();

                try
                {
                    MarkNewsLinksAsOld();
                }
                catch
                {

                    ClearTable<NewsLink>();
                    System.Diagnostics.Debug.WriteLine("[NewsLink]Clearing newslinks from localDB.");
                }

                List<NewsLink> ItemsToAdd = new List<NewsLink>();
                List<NewsLink> CurrentNewsLink = GetItems<NewsLink>().ToList();

                IList<INewsLink> NewslinksFromSource = await NewsLinkTask;

                if (NewslinksFromSource.Count == 0)
                {
                    NewsSemaphore.Release();
                    return NewslinksFromSource;
                }

                try
                {
                    foreach (NewsLink nl in NewslinksFromSource.Reverse())
                    {
                        if (CurrentNewsLink.Where(n => n.URL == nl.URL).Count() == 0)
                        {
                            nl.New = true;
                            nl.TimeStamp = DateTime.Now;

                            ItemsToAdd.Add(nl);

                            System.Diagnostics.Debug.WriteLine("[NewsLink]Adding " + nl.Title + " to localDB.");
                        }
                    }

                    LoginInfoDataHandler.instance.GetLoginInfo().UpdateLastNewsRetrival(DateTime.Now);

                    if (ItemsToAdd.Count > 0)
                    {
                        SaveItems(ItemsToAdd);
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("[NewsLink]Clearing newslinks from localDB.");
                    ClearTable<NewsLink>();
                    return NewslinksFromSource;
                }

                NewsSemaphore.Release();
            }

            var NewsLinksFromDB = base.GetItems<NewsLink>().OrderByDescending(h => h.TimeStamp).ThenByDescending(h => h.InternalID).Take(15).ToList();
            Task ClearDoubleItemsFromDBTask = Task.Run(() => ClearDoubleItemsFromDB());
            return NewsLinksFromDB.Cast<INewsLink>().ToList();
        }

        private async Task ClearDoubleItemsFromDB()
        {
            await NewsSemaphore.WaitAsync();

            List<NewsLink> NewslinksToDelete = new List<NewsLink>();
            int Counter = 0;
            var Newslinks = GetItems<NewsLink>().OrderByDescending(h => h.TimeStamp).ThenByDescending(h => h.InternalID);

            foreach (NewsLink nl in Newslinks)
            {
                Counter++;

                if (GetItems<NewsLink>().Where(n => n.URL == nl.URL).Count() == 2 || Counter > 40)
                {
                    NewslinksToDelete.Add(nl);
                }
            }

            List<NewsLink> ItemsToDelete = new List<NewsLink>();

            foreach (NewsLink nl in NewslinksToDelete)
            {
                ItemsToDelete.Add(nl);
            }

            DeleteItems(ItemsToDelete);

            NewsSemaphore.Release();
        }

        private void MarkNewsLinksAsOld()
        {
            NewsLink[] NewsLinks = GetItems<NewsLink>().Where(n => n.New).ToArray();

            foreach (NewsLink nl in NewsLinks)
            {
                nl.New = false;
            }

            SaveItems(NewsLinks);
        }

        private IList<INewsLink> GetNewsLinksFromSource(string Source)
        {
            List<INewsLink> NewsLinks = new List<INewsLink>();

            try
            {
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

            }
            catch
            {
                //Locked page?
            }

            return NewsLinks;
        }
    }
}
