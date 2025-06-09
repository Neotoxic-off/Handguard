using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Handguard.Client.ViewModels
{
    public partial class UploadViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _uploadFile;

        [ObservableProperty]
        private string? _uploadSpeed;

        public UploadViewModel()
        {
        }
    }
}
