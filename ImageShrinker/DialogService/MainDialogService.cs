using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ImageShrinker.DialogService
{
    class MainDialogService : IDialogService
    {
        public void ShowErrorBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInformationBox(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowYesNoQuestionBox(string message, string caption)
        {
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public string OpenFolderBrowser()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : "" ;
        }
    }
}
