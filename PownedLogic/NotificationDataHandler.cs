using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace PownedLogic
{
    public static class NotificationDataHandler
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static IAsyncOperation<IList<NewsLink>> GenerateNotifications()
        {
            return GenerateNotificationsHelper().AsAsyncOperation();
        }

        private static async Task<IList<NewsLink>> GenerateNotificationsHelper()
        {
            IList<NewsLink> NewNewsLinks = new List<NewsLink>();

            try
            {
                string LastURL = string.Empty;

                if (localSettings.Values["LastNewsItem"] != null)
                {
                    LastURL = localSettings.Values["LastNewsItem"].ToString();
                }
                else
                {
                    return NewNewsLinks;
                }

                IList<Headline> News = await Datahandler.GetHeadlines();
                int NotificationCounter = 0;

                //Test Code
                //LastURL = News[3].URL;

                foreach (Headline h in News)
                {
                    if (h.URL == LastURL)
                    {
                        return NewNewsLinks;
                    }

                    NewNewsLinks.Add(new NewsLink(h.Title, h.HashTag, h.URL));
                    NotificationCounter++;
                }
            }
            catch (Exception)
            {

            }

            return new List<NewsLink>();
        }

    }
}
