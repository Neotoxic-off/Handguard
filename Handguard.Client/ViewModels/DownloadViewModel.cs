using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Handguard.Client.ViewModels
{
    public partial class DownloadViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _downloadFolder;

        [ObservableProperty]
        private string? _downloadSpeed;

        public DownloadViewModel()
        {
        }
    }
}
