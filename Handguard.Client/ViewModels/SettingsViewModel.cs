using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Handguard.Client.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private Settings? _settings;

        public SettingsViewModel()
        {

        }

        private void SaveSettings()
        {
            if (Directory.Exists(Constants.SettingsFolderPath) == false)
            {
                Directory.CreateDirectory(Constants.SettingsFolderPath);
            }

            File.WriteAllText(Constants.SettingsFilePath, JsonConvert.SerializeObject(_settings));
        }

        private void Close()
        {
            Application.Current.Windows.OfType<Windows.SettingsWindow>().FirstOrDefault()?.Close();
        }

        [RelayCommand]
        private void Save()
        {
            SaveSettings();
            Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            Close();
        }
    }
}
