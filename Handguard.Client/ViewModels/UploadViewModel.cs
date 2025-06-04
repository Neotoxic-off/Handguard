using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Handguard.Client.ViewModels
{
    public partial class UploadViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _id;

        [ObservableProperty]
        private string? _password;

        [ObservableProperty]
        private string? _file;

        [ObservableProperty]
        private string? _server;

        public UploadViewModel()
        {

        }

        [RelayCommand]
        private async Task Upload()
        {
            File = @"C:\Users\paulg\Downloads\[LimeTorrents.lol][IssouCorp].Mairimashita!.Iruma-kun.2..Welcome.to.Demon.School.S2..-.01.-.09.VOSTFR.[720p].torrent";
            Server = "http://127.0.0.1:5000";
            Dictionary<string, string>? result = await Lib.Client.UploadSecureAsync(File, Server);
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
    }
}
