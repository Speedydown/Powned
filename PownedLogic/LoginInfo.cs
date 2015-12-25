﻿using BaseLogic.DataHandler;
using PownedLogic.DataHandlers;
using SQLiteForWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic
{
    public class LoginInfo : DataObject
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CookieCollection { get; set; }


        private bool _LoginControlsEnabled;
        [Ignore]
        public bool LoginControlsEnabled
        {
            get { return _LoginControlsEnabled; }
            set
            {
                _LoginControlsEnabled = value;
                NotifyPropertyChanged();
            }
        }


        [Ignore]
        public string LoginStatus
        {
            get
            {
                if (LoginInfoDataHandler.instance.IsLoggedIn)
                {
                    return string.Format("Je bent ingelogd als {0}.", LoginInfoDataHandler.instance.CommenterName);
                }
                else
                {
                    return "Je bent nu niet ingelogd.";
                }
            }
        }

        [Ignore]
        public string LoginText
        {
            get
            {
                return (LoginInfoDataHandler.instance.IsLoggedIn) ? "Wijzigen" : "Inloggen";
            }
        }

        public LoginInfo()
            : base()
        {
            LoginControlsEnabled = true;
        }

        public void UpdateStatus()
        {
            NotifyPropertyChanged("LoginText");
            NotifyPropertyChanged("LoginStatus");
        }
    }
}