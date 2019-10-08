using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;

namespace WSM.SynData
{
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        /// <summary>
        /// Tracking closed windows
        /// </summary>
        private int ClosedTime = 0;
        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon()
            {
                Icon = new Icon("wsm.ico"),
                Visible = true
            };
            notifyIcon.Text = "WSM";

            notifyIcon.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    WindowState = WindowState.Normal;
                };

            notifyIcon.ContextMenu = InitializeConTextMenu();
        }

        public ContextMenu InitializeConTextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();
            var showAppMenuItem = new MenuItem("Show");
            showAppMenuItem.Click += delegate { Show(); };
            var exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += delegate { System.Windows.Application.Current.Shutdown(); notifyIcon.Dispose(); };
            contextMenu.MenuItems.Add(showAppMenuItem);
            contextMenu.MenuItems.Add(exitMenuItem);
            return contextMenu;
        }

        private void GoToPageExecuteHandler(object sender, ExecutedRoutedEventArgs e)
        {
            frmContent.NavigationService.Navigate(new Uri((string)e.Parameter, UriKind.Relative));
        }

        private void GoToPageCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            if (ClosedTime == 0)
                notifyIcon.ShowBalloonTip(2000, "WSM", "WSM is running under background", ToolTipIcon.Info);
            Hide();
            ClosedTime++;
            base.OnClosing(e);
        }
    }
}
