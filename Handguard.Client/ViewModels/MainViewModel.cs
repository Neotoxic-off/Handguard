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
using System.Net;

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

        [ObservableProperty]
        private bool _canUpload = true;

        [ObservableProperty]
        private bool _canDownload = true;

        [ObservableProperty]
        private double _progress = 0.0;

        [ObservableProperty]
        private string _progressMessage = string.Empty;

        private Windows.SettingsWindow _settingsWindow;
        private Windows.FileCredentialsWindow _fileWindow;

        public MainViewModel()
        {
            CheckServerStatus();
        }

        [RelayCommand]
        private async Task Upload()
        {
            string? file = null;
            string? result = null;
            long totalSize = 0;
            Lib.Models.UploadResponse? uploadResponse = null;

            Progress = 0;

            if (Utils.Dialogs.File.ShowDialog().HasValue == true)
            {
                if (Utils.Dialogs.File.FileName != string.Empty)
                {
                    file = Utils.Dialogs.File.FileName;
                    totalSize = new FileInfo(file).Length;
                    result = await Lib.Client.UploadSecureAsync(file, Host, (bytesUploaded) =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Progress = (double)bytesUploaded / totalSize * 100;
                            ProgressMessage = $"{Progress:F1}%";
                        });
                    });


                    if (result is not null)
                    {
                        Progress = 100;
                        uploadResponse = JsonConvert.DeserializeObject<Lib.Models.UploadResponse>(result);
                        Id = uploadResponse?.Id;
                        Password = uploadResponse?.Password;

                        DisplayFileCredentials(Id, Password);
                    }

                    Utils.Dialogs.File.FileName = string.Empty;
                }
            }
        }

        [RelayCommand]
        private async Task Download()
        {
            Progress = 0;

            if (Utils.Dialogs.Directory.ShowDialog().HasValue == true)
            {
                if (Utils.Dialogs.Directory.FolderName != string.Empty)
                {
                    GetFileCredentials();

                    string saveFolder = Utils.Dialogs.Directory.FolderName;

                    var info = await Lib.Client.GetFileInfoAsync(Id, Password, Host);
                    if (info == null)
                    {
                        MessageBox.Show("Failed to get file info.");
                        return;
                    }

                    long totalSize = info.Size;

                    await Lib.Client.DownloadSecureAsync(Id, Password, Host, saveFolder, (bytesDownloaded) =>
                    {
                        Progress = (double)bytesDownloaded / totalSize * 100;
                        ProgressMessage = $"{Progress:F1}%";
                    });

                    Progress = 100;

                    Utils.Dialogs.Directory.FolderName = string.Empty;
                }
            }
        }

        [RelayCommand]
        private void Settings()
        {
            _settingsWindow = new Windows.SettingsWindow();
            _settingsWindow.DataContext = new SettingsViewModel()
            {
                Online = Online,
                Host = _host
            };

            _settingsWindow.ShowDialog();
        }

        private void DisplayFileCredentials(string? id, string? password)
        {
            _fileWindow = new Windows.FileCredentialsWindow();
            FileCredentialsViewModel? context = _fileWindow.DataContext as FileCredentialsViewModel;

            context = new FileCredentialsViewModel()
            {
                ID = id ?? string.Empty,
                Password = password ?? string.Empty
            };

            _fileWindow.DataContext = context;
            _fileWindow.ShowDialog();
        }

        private void GetFileCredentials()
        {
            _fileWindow = new Windows.FileCredentialsWindow();
            FileCredentialsViewModel? context = _fileWindow.DataContext as FileCredentialsViewModel;

            context = new FileCredentialsViewModel();

            _fileWindow.DataContext = context;
            _fileWindow.ShowDialog();

            Id = context.ID;
            Password = context.Password;
        }

        private async void CheckServerStatus()
        {
            HttpClient _httpClient = new HttpClient();
            HttpResponseMessage? response = null;

            try
            {
                response = await _httpClient.GetAsync(_host);
                Online = response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                Online = false;
            }
        }
    }
}
