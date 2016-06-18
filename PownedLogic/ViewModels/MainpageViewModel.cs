using BaseLogic;
using PownedLogic.DataHandlers;
using PownedLogic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace PownedLogic.ViewModels
{
    public class MainpageViewModel : ViewModel
    {
        public static readonly MainpageViewModel instance = new MainpageViewModel();

        public string LastParameter { get; set; }
        public HeadlinesViewModel headlinesViewModel { get; private set; }
        public NewsViewModel newsViewModel { get; private set; }

        private MainpageViewModel()
        {
            this.headlinesViewModel = HeadlinesViewModel.instance;
            this.newsViewModel = NewsViewModel.instance;

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }
    }
}
