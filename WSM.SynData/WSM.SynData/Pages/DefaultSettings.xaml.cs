using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Controls;
using WSM.SynData.Services;
using WSM.SynData.Utils;

namespace WSM.SynData.Pages
{
    public partial class Settings : Page
    {
        private readonly MailService _mailService;
        private readonly Properties.Settings _settings;
        public Settings()
        {
            _mailService = new MailService();
            _settings = Properties.Settings.Default;
            InitializeComponent();
            LoadReceiver();
            LoadGeneralSetting();
        }

        private void LoadReceiver(bool showEditor = false)
        {

            var mailCient = _mailService.GetMailClient();
            txtReceiver.Text = mailCient.receiver;
            txtBlReceiver.Text = mailCient.receiver;
            btnEditMailSettings.Visibility = Display(!showEditor);
            txtBlReceiver.Visibility = Display(!showEditor);
            btnSaveMailSettings.Visibility = Display(showEditor);
            txtReceiver.Visibility = Display(showEditor);
            btnCancelMailSettings.Visibility = Display(showEditor);
        }

        private Visibility Display(bool showEditor = false)
        {
            Visibility isDisplay;
            if (showEditor)
                isDisplay = Visibility.Visible;
            else
                isDisplay = Visibility.Hidden;
            return isDisplay;
        }

        private void LoadGeneralSetting(bool showEditor = false)
        {
            txtApi.Text = _settings.api;
            txtTimeKillThread.Text = _settings.timekillthread.ToString();
            txtTimeLoop.Text = _settings.timeloop.ToString();
            txtBlApi.Text = _settings.api;
            txtBlTimeKillThread.Text = _settings.timekillthread.ToString();
            txtBlTimeLoop.Text = _settings.timeloop.ToString();
            txtBlToken.Text = _settings.token;
            txtToken.Text = _settings.token;
            txtBlCompanyCode.Text = _settings.companyCode;
            txtCompanyCode.Text = _settings.companyCode;
            btnEditGener.Visibility = Display(!showEditor);
            txtBlApi.Visibility = Display(!showEditor);
            txtBlTimeKillThread.Visibility = Display(!showEditor);
            txtBlTimeLoop.Visibility = Display(!showEditor);
            txtBlCompanyCode.Visibility = Display(!showEditor);
            txtBlToken.Visibility = Display(!showEditor);
            btnSaveGener.Visibility = Display(showEditor);
            btnCancelGener.Visibility = Display(showEditor);
            txtApi.Visibility = Display(showEditor);
            txtTimeKillThread.Visibility = Display(showEditor);
            txtTimeLoop.Visibility = Display(showEditor);
            txtToken.Visibility = Display(showEditor);
            txtCompanyCode.Visibility = Display(showEditor);
        }

        private void btnSaveMailSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtReceiver.Text.IsValidEmailAddress())
                {
                    var mailCient = _mailService.GetMailClient();
                    mailCient.receiver = txtReceiver.Text;
                    _settings.mailreport = JsonConvert.SerializeObject(mailCient);
                    SettingService.Commit();
                    LoadReceiver();
                }
                else
                {
                    LoadReceiver(showEditor: true);
                    SettingService.Error("Email is invalid");
                }
            }
            catch
            {
                LoadReceiver(showEditor: true);
                SettingService.Error();
            }
        }

        private void btnEditMailSettings_Click(object sender, RoutedEventArgs e)
        {
            LoadReceiver(showEditor: true);
        }

        private void btnCancelMailSettings_Click(object sender, RoutedEventArgs e)
        {
            LoadReceiver();
        }

        private void btnEditGener_Click(object sender, RoutedEventArgs e)
        {
            LoadGeneralSetting(showEditor: true);
        }

        private void btnCancelGener_Click(object sender, RoutedEventArgs e)
        {
            LoadGeneralSetting();
        }

        private void btnSaveGener_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!txtTimeKillThread.Text.IsNumeric())
                {
                    SettingService.Error("Time to kill thread must be a number");
                }
                else if (!txtTimeLoop.Text.IsNumeric())
                {
                    SettingService.Error("Time loop must be a number");
                }
                else if (!txtApi.Text.IsValidUrl())
                {
                    SettingService.Error("Invalid api end point");
                }
                else
                {
                    _settings.timekillthread = int.Parse(txtTimeKillThread.Text);
                    _settings.api = txtApi.Text;
                    _settings.token = txtToken.Text;
                    _settings.companyCode = txtCompanyCode.Text;
                    _settings.timeloop = int.Parse(txtTimeLoop.Text);
                    SettingService.Commit();
                    LoadGeneralSetting();
                }
            }
            catch (Exception)
            {
                LoadGeneralSetting(showEditor: true);
                SettingService.Error();
            }
        }
    }
}
