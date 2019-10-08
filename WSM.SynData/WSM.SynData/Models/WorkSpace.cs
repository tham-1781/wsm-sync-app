﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WSM.SynData.Utils;
using WSM.SynData.ViewModels;

namespace WSM.SynData.Models
{
    public enum Location
    {
        Toong = 1
        , Laboratory = 2
        , HCM = 3
        , Danang = 4
        , Keangnam = 5
    }

    public enum MachineType
    {
        BlackNWhite = 1
        , IFace = 2
        , TFT = 3
    }
    public class Workspace : INotifyPropertyChanged
    {
        #region Properties
        public Location local { get; set; }
        public string attMachineIp { get; set; }

        public int attMachinePort { get; set; }

        private bool isChecked = false;

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyPropertyChanged();
            }
        }
        public MachineType attMachineType { get; set; }
        private const int dwMachineNumber = 1;
        public OffTime WorkingTime;
        public string Note { get; set; }
        [JsonIgnore]
        public List<Attendance> lstAtt;
        [JsonIgnore]
        public zkemkeeper.CZKEM connector;
        [JsonIgnore]
        public string ErrorMess;
        [JsonIgnore]
        public int PushCount;
        [JsonIgnore]
        public DateTime begin;
        [JsonIgnore]
        public DateTime end;
        [JsonIgnore]
        public MailClient reportmail;
        [JsonIgnore]
        private string uri = Properties.Settings.Default.api;
        [JsonIgnore]
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        #region Methods
        public Workspace()
        {
            lstAtt = new List<Attendance>();
            connector = new zkemkeeper.CZKEM();
        }
        public Workspace(Location lcLocal, string strIP, int iPort, MachineType mtype)
        {
            local = lcLocal;
            attMachineIp = strIP;
            attMachinePort = iPort;
            attMachineType = mtype;
            connector = new zkemkeeper.CZKEM();
            lstAtt = new List<Attendance>();
            //reportmail = new MailClient();
        }

        public WorkspaceVm ToWorkspaceVm()
        {
            return new WorkspaceVm()
            {
                local = this.local,
                attMachineIp = this.attMachineIp,
                attMachinePort = this.attMachinePort,
                attMachineType = this.attMachineType,
                Note = this.Note,
            };
        }

        public string ErrorMessages()
        {
            var errorMessages = "";
            if (string.IsNullOrWhiteSpace(attMachineIp))
            {
                errorMessages += $"\n- Machine Ip can't be empty";
            }
            if (attMachinePort <= 0)
            {
                errorMessages += $"\n- Machine port must greater than 0";
            }
            if (!attMachineIp.IsValidIPv4())
            {
                errorMessages += $"\n- Ip address is invalid";
            }
            return errorMessages;
        }

        private bool ConnectDevice()
        {
            try
            {
                if (connector.Connect_Net(attMachineIp, attMachinePort))
                {
                    connector.RegEvent(dwMachineNumber, 65535);
                    connector.EnableDevice(dwMachineNumber, false);
                    begin = DateTime.Now;
                    return true;
                }
                else
                {
                    int iError = 0;
                    connector.GetLastError(ref iError);
                    ErrorMess = "ConnectDevice | " + attMachineIp + " | Errorcode = " + iError + " | Cannot connect to device";
                    log.Error(ErrorMess);
                    reportmail.SendMail("WSMSyn connect error | " + DateTime.Now.ToString(), ErrorMess);
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error("ConnectDevice | " + attMachineIp + " | " + ex.Message);
                return false;
            }
        }
        private bool DisconnectDevice()
        {
            try
            {
                connector.EnableDevice(dwMachineNumber, true);
                connector.Disconnect();
                end = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                log.Error("DisconnectDivice | " + attMachineIp + " | " + ex.Message);
                return false;
            }
        }
        string years = "";

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool GetData(DateTime dtFrom, DateTime dtTo)
        {
            try
            {
                string sdwEnrollNumber = "";
                int idwTMachineNumber = 0;
                int idwEnrollNumber = 0;
                int idwEMachineNumber = 0;
                int idwVerifyMode = 0;
                int idwInOutMode = 0;
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;
                int idwSecond = 0;
                int idwWorkcode = 0;
                DateTime itemtime;
                Attendance item1;
                Attendance item2;
                lstAtt.Clear();

                if (connector.ReadGeneralLogData(dwMachineNumber))
                {
                    if (attMachineType == MachineType.BlackNWhite)
                    {
                        while (connector.GetGeneralLogData(dwMachineNumber, ref idwTMachineNumber, ref idwEnrollNumber, ref idwEMachineNumber, ref idwVerifyMode,
                            ref idwInOutMode, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute))
                        {
                            if (DateTime.DaysInMonth(idwYear, idwMonth) < idwDay)
                                idwDay = DateTime.DaysInMonth(idwYear, idwMonth);
                            if (idwYear == 2019)
                                years += idwYear + "\n\n";
                            itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, 0);
                            if (itemtime >= dtFrom && itemtime <= dtTo)
                            {
                                item1 = new Attendance(idwEnrollNumber.ToString(), itemtime, true);
                                item2 = new Attendance(idwEnrollNumber.ToString(), itemtime, false);
                                if (lstAtt.Contains(item1) || lstAtt.Contains(item2))
                                    continue;
                                else
                                    lstAtt.Add(item2);
                            }
                        }
                    }
                    if (attMachineType == MachineType.TFT)
                    {
                        while (connector.SSR_GetGeneralLogData(dwMachineNumber, out sdwEnrollNumber, out idwVerifyMode,
                           out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))
                        {
                            itemtime = new DateTime(idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond);
                            if (itemtime.DayOfYear >= dtFrom.DayOfYear && itemtime.DayOfYear <= dtTo.DayOfYear)
                            {
                                item1 = new Attendance(sdwEnrollNumber, itemtime, true);
                                item2 = new Attendance(sdwEnrollNumber, itemtime, false);
                                if (lstAtt.Contains(item1) || lstAtt.Contains(item2))
                                    continue;
                                else
                                    lstAtt.Add(item2);
                            }
                        }
                    }
                }
                else
                {
                    log.Error("GetData | Can't get data from devices");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("GetData | " + ex.Message);
                return false;
            }
        }
        private bool SendData()
        {
            try
            {
                var token = Properties.Settings.Default.token;
                var companyCode = Properties.Settings.Default.companyCode;
                PushCount = 0;
                string strJSON = string.Empty;
                foreach (var item in lstAtt.Where(x => x.pushed == false))
                {
                    if (PushCount != 0)
                    {
                        strJSON += ",";
                    }
                    strJSON += "{\"EnrollNumber\": " + item.EnrollNumber + ", \"date\": \"" + item.date.ToString("yyyy-MM-dd HH:mm") + "\"}";
                    PushCount++;
                }
                strJSON = "{\"workspace_id\": " + ((int)local).ToString() + ", \"data\": [" + strJSON + " ]}";
                strJSON = "{\"token\": " + token + ", \"company_code\": " + companyCode + ", \"attendance_records\": [ " + strJSON + " ]}";
                StringContent stringContent = new StringContent(strJSON, Encoding.UTF8, "application/json");
                using (HttpClient client = new HttpClient())
                {
                    var response = client.PostAsync(uri, stringContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ErrorMess = response.StatusCode.ToString();
                        foreach (var item in lstAtt.Where(x => x.pushed == false))
                            item.pushed = true;
                        return true;
                    }
                    else
                    {
                        ErrorMess = response.StatusCode.ToString();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("SendData | " + ex.Message);
                return false;
            }
        }
        public void SynDaily()
        {
            try
            {
                if (ConnectDevice())
                {
                    DateTime dtFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    DateTime dtTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
                    if (lstAtt.Count != 0)
                        if (lstAtt.Max(x => x.date) < dtFrom)
                            lstAtt.Clear();
                        else
                            dtFrom = lstAtt.Max(x => x.date);
                    GetData(dtFrom, dtTo);
                    DisconnectDevice();
                    SendData();
                    log.Info(attMachineIp + " | " + ErrorMessage() + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
                }

            }
            catch (Exception ex)
            {
                log.Error("SynDaily | " + ex.Message);
            }
        }

        public void SynManual(DateTime dtFrom, DateTime dtTo)
        {
            try
            {
                if (ConnectDevice())
                {
                    GetData(dtFrom, dtTo);
                    DisconnectDevice();
                    SendData();
                    
                    log.Info("SynManual | " + attMachineIp + " | " + ErrorMessage() + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("SynManual | " + ex.Message);
            }
        }
        #endregion

        private string ErrorMessage()
        {
            return string.IsNullOrEmpty(ErrorMess) ? "Success" : ErrorMess;
        }
    }
}
