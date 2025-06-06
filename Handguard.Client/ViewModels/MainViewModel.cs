using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows;

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

        private readonly int _timeout = 5000;

        public MainViewModel()
        {
            //CheckServerStatus();
        }

        [RelayCommand]
        private async Task Upload()
        {
            string file = @"C:\Users\paulg\Downloads\Five.Nights.at.Freddys.rar";
            Dictionary<string, string>? result = await Lib.Client.UploadSecureAsync(file, Host);
            if (result == null)
            {
                return;
            }
            if (result.ContainsKey("id") && result.ContainsKey("pass"))
            {
                Id = result["id"];
                Password = result["pass"];
            }
        }

        [RelayCommand]
        private async Task Download()
        {
            string file = @"C:\Users\paulg\Downloads";
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
            using Ping ping = new Ping();
            PingReply reply = await ping.SendPingAsync(_host, _timeout);
 
            Online = (reply.Status == IPStatus.Success);
        }
    }
}
