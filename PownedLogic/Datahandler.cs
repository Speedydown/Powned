using PownedLogic.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.Foundation;
using WRCHelperLibrary;

namespace PownedLogic
{
    public sealed class Datahandler : IDataHandler
    {
        private static readonly Random randomizer = new Random();

        private static readonly Datahandler _instance = new Datahandler();
        public static Datahandler instance
        {
            get
            {
                return _instance;
            }
        }

        private Datahandler()
        {

        }

        public IAsyncOperation<IList<INewsLink>> GetNewsLinksByPage(int PageID)
        {
            return GetNewsLinksByPageHelper().AsAsyncOperation();
        }

        private async Task<IList<INewsLink>> GetNewsLinksByPageHelper()
        {
            return await NewsLinksDataHandler.instance.GetNewsLinks();
        }

        public IAsyncOperation<IList<PopularHeadline>> GetPopularNewsLinks()
        {
            return GetPopularNewsLinksHelper().AsAsyncOperation();
        }

        private async Task<IList<PopularHeadline>> GetPopularNewsLinksHelper()
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));

            return PopularHeadlinesParser.GetHeadlinesFromSource(PageSource);
        }

        public IAsyncOperation<IList<SearchResult>> Search(string Input)
        {
            return SearchHelper(Input).AsAsyncOperation();
        }

        private async Task<IList<SearchResult>> SearchHelper(string Input)
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/fastsearch?query=" + Input + "&zoek=zoek", Encoding.GetEncoding("iso-8859-1"));

            return SearchResultParser.GetSearchResult(PageSource);
        }

        public IAsyncOperation<INewsItem> GetNewsItemByURL(string URL)
        {
            return GetNewsItemByURLHelper(URL).AsAsyncOperation();
        }

        private async Task<INewsItem> GetNewsItemByURLHelper(string URL)
        {
            if (!URL.StartsWith("http://www.powned.tv"))
            {
                URL = "http://www.powned.tv" + URL;
            }

            URL += "?" + randomizer.Next(0, 20000000);

            Task PostAppStats = Task.Run(() => PostAppStatsHelper(URL));
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL(URL, Encoding.GetEncoding("iso-8859-1"));

            return await NewsItemParser.GetNewsItemFromSource(PageSource);
        }

        internal async Task PostAppStatsHelper(string URL)
        {
            await HTTPGetUtil.GetDataAsStringFromURL("http://speedydown-001-site2.smarterasp.net/api.ashx?Powned=" + URL);
        }
    }
}
