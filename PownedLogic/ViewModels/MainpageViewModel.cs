﻿using BaseLogic;
using PownedLogic.DataHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using XamlControlLibrary;

namespace PownedLogic.ViewModels
{
    public class MainpageViewModel : ViewModel
    {
        public static readonly MainpageViewModel instance = new MainpageViewModel();

        public string LastParameter { get; set; }
        public HeadlinesViewModel headlinesViewModel { get; private set; }
        public NewsViewModel newsViewModel { get; private set; }
        public SearchViewModel searchViewModel { get; private set; }
        public Task LoginTask { get; private set; }

        private MainpageViewModel()
        {
            this.headlinesViewModel = HeadlinesViewModel.instance;
            this.newsViewModel = NewsViewModel.instance;
            this.searchViewModel = SearchViewModel.instance;

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();

            LoginTask = Task.Run(async () =>
                {
                    LoginInfo loginInfo = LoginInfoDataHandler.instance.GetLoginInfo();

                    if (!(loginInfo.UserName == string.Empty || loginInfo.Password == string.Empty))
                    {
                        await LoginInfoDataHandler.instance.Login(loginInfo);
                    }
                });
        }
    }
}
