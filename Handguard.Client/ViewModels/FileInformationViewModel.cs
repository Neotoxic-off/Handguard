using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Handguard.Client.ViewModels
{
    public partial class FileInformationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _fileID;

        [ObservableProperty]
        private string? _filePassword;

        public FileInformationViewModel()
        {
        }
    }
}
