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
    public partial class FileCredentialsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _iD;

        [ObservableProperty]
        private string? _password;

        public FileCredentialsViewModel()
        {

        }

        [RelayCommand]
        private void Close()
        {
            Application.Current.Windows.OfType<Windows.FileCredentialsWindow>().FirstOrDefault()?.Close();
        }
    }
}
