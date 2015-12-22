using PownedLogic.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;
using WRCHelperLibrary;

namespace PownedLogic
{
    public static class NotificationDataHandler
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static IAsyncAction GenerateNotifications()
        {
            return GenerateNotificationsHelper().AsAsyncAction();
        }

        private static async Task GenerateNotificationsHelper()
        {
            IList<Headline> News = (await HeadlinesDataHandler.instance.GetLatestHeadlines(false, 30));

            if (localSettings.Values["Notificaties headlines"] != null &&
                Convert.ToBoolean(localSettings.Values["Notificaties headlines"]))
            {
                foreach (Headline h in News)
                {
                    if (h.New)
                    {
                        NotificationHandler.CreateNotification(h.Title, h.HashTag, h.URL);
                    }
                }
            }

            var UnseenHeadlines = HeadlinesDataHandler.instance.GetUnseenHeadlines();

            if (UnseenHeadlines.Count > 0)
            {
                CreateTile(UnseenHeadlines.Cast<INewsLink>().ToList(), UnseenHeadlines.Count);
                BadgeHandler.CreateBadge(UnseenHeadlines.Count);
            }

            if (localSettings.Values["Notificaties actueel"] != null && 
                Convert.ToBoolean(localSettings.Values["Notificaties actueel"]))
            {
                foreach (INewsLink nl in await Datahandler.instance.GetNewsLinksByPage(0))
                {
                    if ((nl as NewsLink).New)
                    {
                        NotificationHandler.CreateNotification(nl.Title, string.Empty, nl.URL);
                    }
                }
            }
        }



        //IList<NewsLink> NewNewsLinks = new List<NewsLink>();

        //try
        //{
        //    string LastURL = string.Empty;

        //    if (localSettings.Values["LastNewsItem"] != null)
        //    {
        //        LastURL = localSettings.Values["LastNewsItem"].ToString();
        //    }
        //    else
        //    {
        //        return NewNewsLinks;
        //    }

        //    //Headlines
        //    IList<Headline> News = (await HeadlinesDataHandler.instance.GetLatestHeadlines(false, 9));
        //    int NotificationCounter = 0;

        //    //Test Code
        //    //LastURL = News[1].URL;

        //    foreach (Headline h in News)
        //    {
        //        if (h.URL == LastURL)
        //        {
        //            break;
        //        }

        //        NewNewsLinks.Add(new NewsLink(h.Title, h.HashTag, h.URL));
        //        NotificationCounter++;
        //    }

        //    string LastNotificationHeadlines = string.Empty;

        //    if (localSettings.Values["LastNotificationHeadlines"] == null)
        //    {
        //        localSettings.Values["LastNotificationHeadlines"] = News.First().URL;
        //    }

        //    LastNotificationHeadlines = localSettings.Values["LastNotificationHeadlines"].ToString();
        //    localSettings.Values["LastNotificationHeadlines"] = News.First().URL;

        //    //LastNotificationHeadlines = News[5].URL;

        //    int Counter = 0;

        //    foreach (Headline h in News)
        //    {
        //        if (localSettings.Values["Notificaties headlines"] != null && Convert.ToBoolean(localSettings.Values["Notificaties headlines"]) && h.URL != LastNotificationHeadlines && Counter < 3)
        //        {
        //            NotificationHandler.CreateNotification(h.Title, h.HashTag, h.URL);
        //            Counter++;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    //Actual news notifications
        //    string LastActualNews = localSettings.Values["LastActualNewsItem"] != null ? localSettings.Values["LastActualNewsItem"].ToString() : string.Empty;

        //    if (LastActualNews != string.Empty && localSettings.Values["Notificaties actueel"] != null && Convert.ToBoolean(localSettings.Values["Notificaties actueel"]))
        //    {
        //        IList<INewsLink> ActualNews = (await Datahandler.instance.GetNewsLinksByPage(0));

        //        //Test code
        //        //LastActualNews = ActualNews[3].URL;
        //        localSettings.Values["LastActualNewsItem"] = ActualNews.First().URL; 

        //        if (LastActualNews != string.Empty)
        //        {
        //            Counter = 0;

        //            foreach (NewsLink l in ActualNews)
        //            {
        //                if (l.URL == LastActualNews || Counter > 2)
        //                {
        //                    break;
        //                }

        //                NotificationHandler.CreateNotification(l.Title, string.Empty, l.URL);
        //                Counter++;
        //            }
        //        }

        //    }
        //}
        //catch (Exception)
        //{

        //}

        //return NewNewsLinks;


        private static void CreateTile(IList<INewsLink> Content, int Counter)
        {
#if WINDOWS_PHONE_APP
            XmlDocument RectangleTile = TileXmlHandler.CreateRectangleTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150IconWithBadgeAndText), Content, Counter, "ms-appx:///assets/Square71x71Logo.scale-240.png", "Powned", "Headlines:");
            XmlDocument SquareTile = TileXmlHandler.CreateSquareTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150IconWithBadge), Content, "ms-appx:///assets/SQUARE71x71Logo.scale-240.png", "Powned", "Headlines:");
            XmlDocument SmallTile = TileXmlHandler.CreateSmallSquareTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare71x71IconWithBadge), "ms-appx:///assets/SQUARE71x71Logo.scale-240.png", "Powned");

            TileXmlHandler.CreateTileUpdate(new XmlDocument[] { RectangleTile, SquareTile, SmallTile });
#endif
        }

    }
}
