using System;
using System.Windows;
using System.Windows.Input;

namespace WSM.SynData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsChecked { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GoToPageExecuteHandler(object sender, ExecutedRoutedEventArgs e)
        {
            IsChecked = true;
            frmContent.NavigationService.Navigate(new Uri((string)e.Parameter, UriKind.Relative));
        }

        private void GoToPageCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
