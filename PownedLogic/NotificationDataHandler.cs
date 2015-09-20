using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using WRCHelperLibrary;

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

                //Headlines
                IList<Headline> News = await Datahandler.instance.GetHeadlines();
                int NotificationCounter = 0;

                //Test Code
                //LastURL = News[3].URL;

                foreach (Headline h in News)
                {
                    if (h.URL == LastURL)
                    {
                        break;
                    }

                    if (localSettings.Values["Notificaties headlines"] != null && Convert.ToBoolean(localSettings.Values["Notificaties headlines"]))
                    {
                        NotificationHandler.CreateNotification(h.Title, h.HashTag, h.URL);
                        await Task.Delay(1000);
                    }

                    NewNewsLinks.Add(new NewsLink(h.Title, h.HashTag, h.URL));
                    NotificationCounter++;
                }


                //Actual news notifications
                string LastActualNews = localSettings.Values["LastActualNewsItem"] != null ? localSettings.Values["LastActualNewsItem"].ToString() : string.Empty;

                if (LastActualNews != string.Empty && localSettings.Values["Notificaties actueel"] != null && Convert.ToBoolean(localSettings.Values["Notificaties actueel"]))
                {
                    IList<INewsLink> ActualNews = await Datahandler.instance.GetNewsLinksByPage(0);

                    //Test code
                    //LastActualNews = ActualNews[3].URL;

                    if (LastActualNews != string.Empty)
                    {
                        foreach (NewsLink l in ActualNews)
                        {
                            if (l.URL == LastActualNews)
                            {
                                break;
                            }

                            NotificationHandler.CreateNotification(l.Title, string.Empty, l.URL);
                            await Task.Delay(1000);
                        }

                    }

                }
            }
            catch (Exception)
            {

            }

            return NewNewsLinks;
        }


    }
}
