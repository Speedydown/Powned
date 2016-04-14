using BaseLogic.Xaml_Controls.Settings;
using System;
using System.Collections.Generic;
using System.Text;

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
            _settingsContainer.AddSwitch(new ToggleSwitchSetting(false, "Notificaties actueel", "Aan", "Uit"));
            _settingsContainer.AddSwitch(new ToggleSwitchSetting(false, "Notificaties headlines", "Aan", "Uit"));
            _settingsContainer.AddSwitch(new ToggleSwitchSetting(true, "Reacties weergeven", "Aan", "Uit"));
            _settingsContainer.AddSwitch(new ToggleSwitchSetting(true, "Media weergeven", "Aan", "Uit"));
        }
    }
}
