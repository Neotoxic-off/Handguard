using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handguard.Client
{
    public static class Constants
    {
        public static string SettingsFolderPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData
            ),
            "Handguard"
        );
        public static string SettingsFilePath = Path.Combine(
            SettingsFolderPath,
            "settings.json"
        );
        public static string DefaultHost = "http://127.0.0.1:5000";
    }
}
