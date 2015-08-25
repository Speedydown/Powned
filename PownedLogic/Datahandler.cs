using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.Foundation;

namespace PownedLogic
{
    public static class Datahandler
    {
        public static IAsyncOperation<IList<NewsLink>> GetNewsLinksByPage()
        {
            return GetNewsLinksByPageHelper().AsAsyncOperation();
        }

        private static async Task<IList<NewsLink>> GetNewsLinksByPageHelper()
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));
            return NewsLinkParser.GetNewsLinksFromSource(PageSource);
        }

        public static IAsyncOperation<IList<Headline>> GetHeadlines()
        {
            return GetHeadlinesHelper().AsAsyncOperation();
        }

        private static async Task<IList<Headline>> GetHeadlinesHelper()
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv", Encoding.GetEncoding("iso-8859-1"));

            return HeadlinesParser.GetHeadlinesFromSource(PageSource);
        }

        public static IAsyncOperation<IList<PopularHeadline>> GetPopularNewsLinks()
        {
            return GetPopularNewsLinksHelper().AsAsyncOperation();
        }

        private static async Task<IList<PopularHeadline>> GetPopularNewsLinksHelper()
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));

            return PopularHeadlinesParser.GetHeadlinesFromSource(PageSource);
        }

        public static IAsyncOperation<IList<SearchResult>> Search(string Input)
        {
            return SearchHelper(Input).AsAsyncOperation();
        }

        private static async Task<IList<SearchResult>> SearchHelper(string Input)
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/fastsearch?query=" + Input + "&zoek=zoek", Encoding.GetEncoding("iso-8859-1"));

            return SearchResultParser.GetSearchResult(PageSource);
        }

        public static IAsyncOperation<NewsItem> GetNewsPageFromURL(string URL)
        {
            return GetNewsPageFromURLHelper(URL).AsAsyncOperation();
        }

        private static async Task<NewsItem> GetNewsPageFromURLHelper(string URL)
        {
            if (!URL.StartsWith("http://www.powned.tv"))
            {
                URL = "http://www.powned.tv" + URL;
            }

            Task PostAppStats = Task.Run(() => PostAppStatsHelper(URL));
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL(URL, Encoding.GetEncoding("iso-8859-1"));

            return NewsItemParser.GetNewsItemFromSource(PageSource);
        }

        internal static async Task PostAppStatsHelper(string URL)
        {
            await HTTPGetUtil.GetDataAsStringFromURL("http://speedydown-001-site2.smarterasp.net/api.ashx?Powned=" + URL);
        }
    }
}
