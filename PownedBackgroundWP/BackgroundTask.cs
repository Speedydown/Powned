using BaseLogic.Notifications;
using BaseLogic.Xaml_Controls.Interfaces;
using PownedLogic;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace PownedBackground
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            CreateTile(await NotificationDataHandler.GenerateNotifications());
            deferral.Complete();
        }

        private static void CreateTile(IList<INewsLink> Content)
        {
            System.Diagnostics.Debug.WriteLine("[Windows Phone] Notification");
            XmlDocument RectangleTile = TileXmlHandler.CreateRectangleTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150IconWithBadgeAndText), Content, Content.Count, "ms-appx:///assets/Square71x71Logo.scale-240.png", "Powned", "Headlines:");
            XmlDocument SquareTile = TileXmlHandler.CreateSquareTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150IconWithBadge), Content, "ms-appx:///assets/SQUARE71x71Logo.scale-240.png", "Powned", "Headlines:");
            XmlDocument SmallTile = TileXmlHandler.CreateSmallSquareTile(TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare71x71IconWithBadge), "ms-appx:///assets/SQUARE71x71Logo.scale-240.png", "Powned");

            TileXmlHandler.CreateTileUpdate(new XmlDocument[] { RectangleTile, SquareTile, SmallTile });
        }
    }
}
