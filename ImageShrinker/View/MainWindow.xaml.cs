using System;
using System.Windows;
using ImageShrinker.Common;
using ImageShrinker.DialogService;
using ImageShrinker.Factories;
using ImageShrinker.ViewModel;

namespace ImageShrinker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                var viewModel = new MainViewModel(new MainDialogService(), new ShrinkerService(), new ShrinkableImageFactory());
                DataContext = viewModel;
                Closing += viewModel.CloseWindowWithConfirmation;
            }
            catch (Exception ex)
            {
                // namiafaszvan
                MessageBox.Show(ex.Message + "\n\r" + ex); 
            }
        }
    }
}
