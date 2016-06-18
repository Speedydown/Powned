﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace PownedLogic
{
    public class MessageService
    {
        public static readonly MessageService instance = new MessageService();

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private MessageService()
        {

        }

        public async Task DisplayInfoMessage()
        {
            bool InfoMessageShown = false;

            try
            {
                InfoMessageShown = (bool)localSettings.Values["InfoMessage1"];
            }
            catch
            {

            }

            if (!InfoMessageShown)
            {
                localSettings.Values["InfoMessage1"] = true;

                var messageDialog = new Windows.UI.Popups.MessageDialog("Doordat PowNed een nieuwe site heeft uitgerold is deze app op dit moment in beperkte vorm beschikbaar. Nieuwe functies worden z.s.m. toegevoegd.", "Nieuwe site PowNed");
                messageDialog.Commands.Add(
                new UICommand("Ik snap het", CommandInvokedHandler));
                await messageDialog.ShowAsync();
            }
        }

        private static async void CommandInvokedHandler(IUICommand command)
        {

        }
    }
}
