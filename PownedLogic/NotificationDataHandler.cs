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
                IList<Headline> News = (await Datahandler.instance.GetHeadlines());
                int NotificationCounter = 0;

                //Test Code
                //LastURL = News[1].URL;

                foreach (Headline h in News)
                {
                    if (h.URL == LastURL)
                    {
                        break;
                    }

                    NewNewsLinks.Add(new NewsLink(h.Title, h.HashTag, h.URL));
                    NotificationCounter++;
                }

                string LastNotificationHeadlines = string.Empty;

                if (localSettings.Values["LastNotificationHeadlines"] == null)
                {
                    localSettings.Values["LastNotificationHeadlines"] = News.First().URL;
                }

                LastNotificationHeadlines = localSettings.Values["LastNotificationHeadlines"].ToString();
                localSettings.Values["LastNotificationHeadlines"] = News.First().URL;

                int Counter = 0;

                foreach (Headline h in News)
                {
                    if (localSettings.Values["Notificaties headlines"] != null && Convert.ToBoolean(localSettings.Values["Notificaties headlines"]) && h.URL != LastNotificationHeadlines && Counter < 3)
                    {
                        NotificationHandler.CreateNotification(h.Title, h.HashTag, h.URL);
                        Counter++;
                    }
                    else
                    {
                        break;
                    }
                }






                //Actual news notifications
                string LastActualNews = localSettings.Values["LastActualNewsItem"] != null ? localSettings.Values["LastActualNewsItem"].ToString() : string.Empty;

                if (LastActualNews != string.Empty && localSettings.Values["Notificaties actueel"] != null && Convert.ToBoolean(localSettings.Values["Notificaties actueel"]))
                {
                    IList<INewsLink> ActualNews = (await Datahandler.instance.GetNewsLinksByPage(0));

                    //Test code
                    //LastActualNews = ActualNews[3].URL;
                    localSettings.Values["LastActualNewsItem"] = ActualNews.First().URL; 

                    if (LastActualNews != string.Empty)
                    {
                        Counter = 0;

                        foreach (NewsLink l in ActualNews)
                        {
                            if (l.URL == LastActualNews || Counter > 2)
                            {
                                break;
                            }

                            NotificationHandler.CreateNotification(l.Title, string.Empty, l.URL);
                            Counter++;
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
