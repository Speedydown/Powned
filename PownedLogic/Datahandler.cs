using BaseLogic.HtmlUtil;
using BaseLogic.Xaml_Controls.Interfaces;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

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
            return GetPopularNewsLinksHelper2().AsAsyncOperation();
        }

        public IAsyncOperation<IList<PopularHeadline>> GetPopularNewsLinks()
        {
            return GetPopularNewsLinksHelper().AsAsyncOperation();
        }

        public async Task<IList<INewsLink>> GetPopularNewsLinksHelper2()
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("https://www.powned.tv/nieuws/", Encoding.UTF8);

            return PopularHeadlinesParser.GetHeadlinesFromSource(PageSource).Cast<INewsLink>().ToList();
        }

        private async Task<IList<PopularHeadline>> GetPopularNewsLinksHelper()
        {
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL("https://www.powned.tv/nieuws/", Encoding.UTF8);

            return PopularHeadlinesParser.GetHeadlinesFromSource(PageSource);
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
            string PageSource = await HTTPGetUtil.GetDataAsStringFromURL(URL, Encoding.UTF8);

            return await NewsItemParser.GetNewsItemFromSource(PageSource);
        }
    }
}
