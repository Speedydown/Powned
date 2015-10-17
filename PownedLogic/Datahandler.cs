using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawlerTools;
using Windows.Foundation;
using WRCHelperLibrary;

namespace PownedLogic
{
    public sealed class Datahandler : IDataHandler
    {
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
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv/sidebar.js", Encoding.GetEncoding("iso-8859-1"));
            return NewsLinkParser.GetNewsLinksFromSource(PageSource);
        }

        public IAsyncOperation<IList<Headline>> GetHeadlines()
        {
            return GetHeadlinesHelper().AsAsyncOperation();
        }

        private async Task<IList<Headline>> GetHeadlinesHelper()
        {
            await Login("speedy@speedydown.nl", "smartlink");
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("http://www.powned.tv", Encoding.GetEncoding("iso-8859-1"));
            return HeadlinesParser.GetHeadlinesFromSource(PageSource);
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

            Task PostAppStats = Task.Run(() => PostAppStatsHelper(URL));
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL(URL, Encoding.GetEncoding("iso-8859-1"));

            return NewsItemParser.GetNewsItemFromSource(PageSource);
        }

        internal async Task PostAppStatsHelper(string URL)
        {
            await HTTPGetUtil.GetDataAsStringFromURL("http://speedydown-001-site2.smarterasp.net/api.ashx?Powned=" + URL);
        }

        public IAsyncAction Login(string Email, string Password)
        {
            return LoginHelper(Email, Password).AsAsyncAction();
        }

        private async Task LoginHelper(string Email, string Password)
        {
            string LoginURL = "http://registratie.geenstijl.nl/registratie/?view=login";
            string LoginPostUrl = "http://registratie.geenstijl.nl/registratie/gs_engine.php?action=login";

            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL(LoginURL, Encoding.GetEncoding("iso-8859-1"));

            Dictionary<string, string> ValueDictionary = LoginHandler.GetLoginFormFields(PageSource, Email, Password);
            bool Result = await HTTPGetUtil.PostDataToURL(LoginPostUrl, ValueDictionary);
        }
    }
}
