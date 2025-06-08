using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handguard.Client.Utils
{
    public static class Dialogs
    {
        public static OpenFileDialog File = new OpenFileDialog
        {
            Title = "Select a file",
            Filter = "All Files (*.*)|*.*"
        };

        public static OpenFolderDialog Directory = new OpenFolderDialog
        {
            Title = "Select a directory"
        };
    }
}
