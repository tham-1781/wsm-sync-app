using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WSM.SynData.Models;
using WSM.SynData.Parameters;
using WSM.SynData.Services;

namespace WSM.SynData.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public List<Workspace> lstSpace = new List<Workspace>();
        public System.Timers.Timer timer;
        public System.Timers.Timer timerstart;
        public int iLoopHour = 1;
        public int iProcessCounter = 1;
        public List<OffTime> lsTime = new List<OffTime>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly WorkspaceService _workspaceService;
        private List<Attendance> Attendances;
        public Home()
        {
            InitializeComponent();
            _workspaceService = new WorkspaceService();
            Attendances = new List<Attendance>();
            MailClient mail = JsonConvert.DeserializeObject<MailClient>(Properties.Settings.Default.mailreport);
            //mail.password = EncrypData.DecryptAES(mail.password);
            lsTime = JsonConvert.DeserializeObject<List<OffTime>>(Properties.Settings.Default.timeoff);
            lstSpace = _workspaceService.GetWorkspaces();
            iLoopHour = Properties.Settings.Default.timeloop;
            timer = new System.Timers.Timer(iLoopHour * 60 * 60 * 1000);
            timer.Elapsed += Timer_Elapsed;
            LoadListBoxData();
            IsShowResult();
        }

        private void LoadListBoxData()
        {
            var groupedWorkspace = from wsp in lstSpace group wsp by wsp.local into itemGroup orderby itemGroup.Key select itemGroup;
            lbWorkspaces.ItemsSource = groupedWorkspace;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var time in lsTime)
            {
                if (time.CheckTime(DateTime.Now.TimeOfDay))
                    return;
            }
            foreach (var item in lstSpace)
            {
                try
                {
                    Thread run = new Thread(item.SynDaily);
                    run.Start();
                    if (!run.Join(TimeSpan.FromMinutes(Properties.Settings.Default.timekillthread)))
                    {
                        run.Abort();
                        log.Error(DateTime.Now.ToShortTimeString() + " | " + item.attMachineIp + " | " + "Get Data Timeout");
                    }
                    Thread.Sleep(10000);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (timer.Enabled == false)
            {
                var currenttime = DateTime.Now;
                var addhour = iLoopHour - ((currenttime.Hour) % iLoopHour);
                TimeSpan nexttime = new TimeSpan(currenttime.Hour + addhour, 0, 0);
                timerstart = new System.Timers.Timer((int)(nexttime - currenttime.TimeOfDay).TotalMilliseconds - 10 * 60 * 1000);
                timerstart.Elapsed += Timerstart_Elapsed;
                timerstart.Enabled = true;
                btnStart.Content = "Stop";
            }
            else
            {
                timer.Enabled = false;
                btnStart.Content = "Start";
            }
        }

        private void Timerstart_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timer.Enabled == false)
            {
                timer.Enabled = true;
                timerstart.Enabled = false;
                Timer_Elapsed(null, null);
            }
        }

        private void btnSyn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime? dtFrom = dpFrom.SelectedDate;
                DateTime? dtTo = dpTo.SelectedDate + new TimeSpan(24, 0, 0);

                var args = new WorkspaceParameters()
                {
                    DateFrom = (DateTime)dtFrom,
                    DateTo = (DateTime)dtTo,
                    SelectedWorkspaces = lstSpace.Where(wsp => wsp.IsChecked)
                };

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.WorkerReportsProgress = true;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync(args);

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            prgBarSync.Value = e.ProgressPercentage;
            prgBarSync.UpdateDefaultStyle();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                dgridAttendances.ItemsSource = Attendances;
                IsShowResult(show: true);
                AttendanceGridHeader();
                prgBarSync.Value = 100;
                prgBarSync.UpdateDefaultStyle();

                MessageBox.Show("Sync data completed", "WSM", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var worker = sender as BackgroundWorker;
                var args = (WorkspaceParameters)e.Argument;
                foreach (var space in args.SelectedWorkspaces)
                {
                    space.SynManual(args.DateFrom, args.DateTo);
                    Attendances.AddRange(space.lstAtt);
                    worker.ReportProgress((iProcessCounter * 100) / args.SelectedWorkspaces.Count());
                    iProcessCounter++;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
        
        private void ckbSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var workspace in lstSpace)
            {
                workspace.IsChecked = true;
                UpdateSelectAllWorkspace(workspace);
            }
        }

        private void ckbSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var workspace in lstSpace)
            {
                workspace.IsChecked = false;
                UpdateSelectAllWorkspace(workspace);
            }
        }

        private void AttendanceGridHeader()
        {
            dgridAttendances.Columns[0].Header = "Enroll Number";
            dgridAttendances.Columns[1].Header = "Date";
            dgridAttendances.Columns[2].Header = "Pushed";
        }

        private void cbxWorkspaceItem_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCheckboxItem(sender);
        }

        private void cbxWorkspaceItem_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateCheckboxItem(sender);
        }

        private void UpdateCheckboxItem(object sender)
        {
            var checkBox = (CheckBox)sender;
            var workspace = (Workspace)checkBox.CommandParameter;
            workspace.IsChecked = checkBox.IsChecked.Value;
            UpdateSelectAllWorkspace(workspace);
        }

        private void UpdateSelectAllWorkspace(Workspace workspace)
        {
            var workspaces = lstSpace.Where(wsp => wsp.local == workspace.local);
            for (int i = 0; i < lbWorkspaces.Items.Count; i++)
            {
                var listBoxItemObj = (ListBoxItem)lbWorkspaces.ItemContainerGenerator.ContainerFromItem(lbWorkspaces.Items[i]);
                var contentPresenterObj = FindVisualChild<ContentPresenter>(listBoxItemObj);
                var dataTemplateObj = contentPresenterObj.ContentTemplate;
                var foundCheckbox = (CheckBox)dataTemplateObj.FindName("cbCheckAllWsp", contentPresenterObj);
                if (workspaces.All(wsp => wsp.IsChecked && wsp.local == (Location)foundCheckbox.CommandParameter))
                {
                    foundCheckbox.IsChecked = true;
                }
                else
                {
                    foundCheckbox.IsChecked = false;
                }
            }
        }

        private ChildControl FindVisualChild<ChildControl>(DependencyObject DependencyObj)
        where ChildControl : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(DependencyObj); i++)
            {
                DependencyObject Child = VisualTreeHelper.GetChild(DependencyObj, i);

                if (Child != null && Child is ChildControl)
                {
                    return (ChildControl)Child;
                }
                else
                {
                    ChildControl ChildOfChild = FindVisualChild<ChildControl>(Child);

                    if (ChildOfChild != null)
                    {
                        return ChildOfChild;
                    }
                }
            }
            return null;
        }

        private void cbCheckAllWsp_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var workspaces = lstSpace.Where(wsp => wsp.local == (Location)checkBox.CommandParameter);
            var isChecked = checkBox.IsChecked.Value;
            foreach (var workspace in workspaces)
            {
                workspace.IsChecked = isChecked;
            }
            var s = workspaces;
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            var filteredResult = Attendances.Where(attendance => attendance.EnrollNumber.ToLower().Contains(txtSearch.Text.ToLower())).ToList();
            dgridAttendances.ItemsSource = filteredResult;
        }

        private void IsShowResult(bool show = false)
        {
            var isVisible = show ? Visibility.Visible : Visibility.Hidden;
            dgridAttendances.Visibility = isVisible;
            txtSearch.Visibility = isVisible;
        }
    }
}
