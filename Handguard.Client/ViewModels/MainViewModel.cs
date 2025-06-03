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

        private readonly string _host = "google.com";
        private readonly int _timeout = 5000;

        public MainViewModel()
        {
            CheckServerStatus();
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
