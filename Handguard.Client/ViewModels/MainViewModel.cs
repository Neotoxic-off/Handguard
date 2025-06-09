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
        private bool _online = false;

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
        private bool _canPing = true;

        [ObservableProperty]
        private string? _downloadSpeed;

        [ObservableProperty]
        private string? _uploadSpeed;

        [ObservableProperty]
        private double? _progress = 0.0;

        [ObservableProperty]
        private string _logs = string.Empty;

        [ObservableProperty]
        private string? _uploadFile = string.Empty;

        [ObservableProperty]
        private string? _downloadFolder = string.Empty;

        private Windows.SettingsWindow? _settingsWindow;

        public MainViewModel()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Online = await Ping();

            CanDownload = Online;
            CanUpload = Online;
            CanPing = true;
        }

        private void UpdateLogs(string message)
        {
            Logs = message;
        }

        [RelayCommand]
        private void Copy(string value)
        {
            Clipboard.SetText(value);
        }

        [RelayCommand]
        private async Task Upload()
        {
            string? result = null;
            long totalSize = 0;
            Lib.Models.UploadResponse? uploadResponse = null;

            Progress = 0;
            CanDownload = false;
            CanUpload = false;
            CanPing = false;

            if (Online == true)
            {
                if (Utils.Dialogs.File.ShowDialog().HasValue == true)
                {
                    if (Utils.Dialogs.File.FileName != string.Empty)
                    {
                        UploadFile = Utils.Dialogs.File.FileName;
                        totalSize = new FileInfo(UploadFile).Length;
                        result = await Lib.Client.UploadSecureAsync(UploadFile, Host, (bytesUploaded, speed) =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Progress = (double)bytesUploaded / totalSize * 100;
                                UpdateLogs($"Uploading {Progress:F1}%");
                                UploadSpeed = $"{(speed / (1024.0 * 1024.0)):0.00} MB/s";
                            });
                        });

                        if (result is not null)
                        {
                            Progress = 100;
                            uploadResponse = JsonConvert.DeserializeObject<Lib.Models.UploadResponse>(result);
                            if (uploadResponse is not null)
                            {
                                UpdateLogs($"File uploaded");

                                Id = uploadResponse?.Id;
                                Password = uploadResponse?.Password;
                            }
                            else
                            {
                                UpdateLogs($"File upload failed");
                            }
                        } else
                        {
                            UpdateLogs($"File upload failed");
                        }

                            Utils.Dialogs.File.FileName = string.Empty;
                    }
                }

                CanDownload = true;
                CanUpload = true;
            }

            CanPing = true;
        }

        [RelayCommand]
        private async Task Download()
        {
            long totalSize = 0;
            Lib.Models.FileInfoResponse? info = null;

            Progress = 0;
            CanDownload = false;
            CanUpload = false;
            CanPing = false;

            if (Online == true)
            {
                if (Utils.Dialogs.Directory.ShowDialog().HasValue == true)
                {
                    if (Utils.Dialogs.Directory.FolderName != string.Empty)
                    {
                        DownloadFolder = Utils.Dialogs.Directory.FolderName;
                        info = await Lib.Client.GetFileInfoAsync(Id, Password, Host);

                        if (info is not null)
                        {
                            totalSize = info.Size;
                            await Lib.Client.DownloadSecureAsync(Id, Password, Host, DownloadFolder,
                                (bytesDownloaded, speed) =>
                                {
                                    Progress = (double)bytesDownloaded / totalSize * 100;
                                    UpdateLogs($"Downloading {Progress:F1}%");
                                    DownloadSpeed = $"{(speed / (1024.0 * 1024.0)):0.00} MB/s";
                                }
                            );

                            Progress = 100;

                            UpdateLogs($"File downloaded");

                            Utils.Dialogs.Directory.FolderName = string.Empty;
                        }
                        else
                        {
                            UpdateLogs($"File download failed");
                        }
                    }
                }

                CanDownload = true;
                CanUpload = true;
            }

            CanPing = true;
        }

        [RelayCommand]
        private void Settings()
        {
            _settingsWindow = new Windows.SettingsWindow();
            _settingsWindow.DataContext = new SettingsViewModel()
            {
                Online = Online,
                Host = Host
            };

            _settingsWindow.ShowDialog();
        }

        [RelayCommand]
        private async Task PingServer()
        {
            CanDownload = false;
            CanUpload = false;
            CanPing = false;

            if (await Ping() == true)
            {
                Online = true;
                CanDownload = true;
                CanUpload = true;
            }
            else
            {
                Online = false;
                CanDownload = false;
                CanUpload = false;
                UpdateLogs("Server is offline");
            }

            CanPing = true;
        }

        private async Task<bool> Ping()
        {
            HttpClient _httpClient = new HttpClient();
            HttpResponseMessage? response = null;

            try
            {
                response = await _httpClient.GetAsync(Host);
                Online = response.IsSuccessStatusCode;

                if (Online == true)
                {
                    UpdateLogs("Server is online");
                }
                else
                {
                    UpdateLogs("Server is offline");
                }

            }
            catch (HttpRequestException)
            {
                UpdateLogs("Server is offline");
            }

            return Online;
        }
    }
}
