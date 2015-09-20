﻿using PownedLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using WRCHelperLibrary;

namespace PownedBackgroundWP
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            IList<NewsLink> NewNewsLinks = await NotificationDataHandler.GenerateNotifications();

            if (NewNewsLinks.Count > 0)
            {
                CreateTile(NewNewsLinks.Cast<INewsLink>().ToList(), NewNewsLinks.Count);
                BadgeHandler.CreateBadge(NewNewsLinks.Count);
            }

            deferral.Complete();
        }

        private void CreateTile(IList<INewsLink> Content, int Counter)
        {
            XmlDocument RectangleTile = TileXmlHandler.CreateRectangleTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150IconWithBadgeAndText), Content, Counter, "ms-appx:///assets/Square71x71Logo.scale-240.png", "Powned", "Headlines:");
            XmlDocument SquareTile = TileXmlHandler.CreateSquareTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150IconWithBadge), Content, "ms-appx:///assets/SQUARE71x71Logo.scale-240.png", "Powned", "Headlines:");
            XmlDocument SmallTile = TileXmlHandler.CreateSmallSquareTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare71x71IconWithBadge), "ms-appx:///assets/SQUARE71x71Logo.scale-240.png", "Powned");

            TileXmlHandler.CreateTileUpdate(new XmlDocument[] { RectangleTile, SquareTile, SmallTile });
        }
    }
}
