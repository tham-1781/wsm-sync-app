using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using WSM.SynData.Models;
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
        public List<OffTime> lsTime = new List<OffTime>();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly WorkspaceService _workspaceService;
        public Home()
        {
            InitializeComponent();
            _workspaceService = new WorkspaceService();
            MailClient mail = JsonConvert.DeserializeObject<MailClient>(Properties.Settings.Default.mailreport);
            //mail.password = EncrypData.DecryptAES(mail.password);
            lsTime = JsonConvert.DeserializeObject<List<OffTime>>(Properties.Settings.Default.timeoff);
            lstSpace = _workspaceService.GetWorkSpaces();
            iLoopHour = Properties.Settings.Default.timeloop;
            timer = new System.Timers.Timer(iLoopHour * 60 * 60 * 1000);
            timer.Elapsed += Timer_Elapsed;
            List<string> lstCombo = new List<string>();
            lstCombo.Add("All");
            foreach (var item in lstSpace)
            {
                lstCombo.Add(item.attMachineIp);
                item.reportmail = mail;
            }
            cbWorkSpace.ItemsSource = lstCombo;
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
                string ip = (string)cbWorkSpace.SelectedItem;
                if (ip == "All")
                {
                    foreach (var item in lstSpace)
                    {
                        item.SynManual((DateTime)dtFrom, (DateTime)dtTo);
                    }
                }
                else
                {
                    var space = lstSpace.Where(x => x.attMachineIp == ip).First();
                    space.SynManual((DateTime)dtFrom, (DateTime)dtTo);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
