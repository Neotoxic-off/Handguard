using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Net.Http;

namespace Handguard.Client.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _title = "Handguard v1.0";

        [ObservableProperty]
        private bool? _online = false;

        [ObservableProperty]
        private string? _statusMessage;

        [ObservableProperty]
        private string? _id;

        [ObservableProperty]
        private string? _password;

        [ObservableProperty]
        private string _host = "http://127.0.0.1:5000";

        private OpenFileDialog _fileDialog = new OpenFileDialog
        {
            Title = "Select a file",
            Filter = "All Files (*.*)|*.*"
        };

        public MainViewModel()
        {
            CheckServerStatus();
        }

        [RelayCommand]
        private async Task Upload()
        {
            if (_fileDialog.ShowDialog().HasValue == true)
            {
                string file = _fileDialog.FileName;
                string? result = await Lib.Client.UploadSecureAsync(file, Host);
                Lib.Models.UploadResponse? uploadResponse = null;

                if (result is not null)
                {
                    uploadResponse = JsonConvert.DeserializeObject<Lib.Models.UploadResponse>(result);
                    Id = uploadResponse?.Id;
                    Password = uploadResponse?.Password;
                }
            }
        }

        [RelayCommand]
        private async Task Download()
        {
            string file = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            await Lib.Client.DownloadSecureAsync(Id, Password, Host, file);
        }

        [RelayCommand]
        private void Settings()
        {
            Windows.SettingsWindow settingsWindow = new Windows.SettingsWindow();

            settingsWindow.DataContext = new SettingsViewModel()
            {
                Online = Online,
                Host = _host
            };

            settingsWindow.ShowDialog();
        }

        private async void CheckServerStatus()
        {
            HttpClient _httpClient = new HttpClient();

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_host);
                Online = response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                Online = false;
            }
        }

    }
}
