using System;
using System.Collections.Generic;
using System.Text;
using WRCHelperLibrary.Settings;

namespace PownedLogic
{
    public static class Settings
    {
        private static readonly SettingsContainer _settingsContainer = new SettingsContainer();
        public static SettingsContainer settingsContainer
        {
            get
            {
                return _settingsContainer;
            }
        }

        public static void Init()
        {
            _settingsContainer.AddSwitch(new ToggleSwitchSetting(true, "Achtergrond taak", "Aan", "Uit"));
            _settingsContainer.AddSwitch(new ToggleSwitchSetting(true, "Notificaties weergeven", "Aan", "Uit"));
        }
    }
}
