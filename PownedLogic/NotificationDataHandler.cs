using BaseLogic.Notifications;
using BaseLogic.Xaml_Controls.Interfaces;
using PownedLogic;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;

namespace PownedLogic
{
    public static class NotificationDataHandler
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static IAsyncOperation<IList<INewsLink>> GenerateNotifications()
        {
            return GenerateNotificationsHelper().AsAsyncOperation();
        }

        private static async Task<IList<INewsLink>> GenerateNotificationsHelper()
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

            var UnseenHeadlines = News.Where(x => x.Seen == false).ToList();

            if (UnseenHeadlines.Count > 0)
            {
                BadgeHandler.CreateBadge(UnseenHeadlines.Count);
            }
       
            return UnseenHeadlines.Cast<INewsLink>().ToList();
        }
    }
}
