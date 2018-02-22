using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace WSM.SynData
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
    public class WorkSpace
    {
        #region Properties
        public Location local;
        public string attMachineIp;
        public int attMachinePort;
        public MachineType attMachineType;
        public OffTime WorkingTime;
        [JsonIgnore]
        public List<Attendance> lstAtt;
        [JsonIgnore]
        public zkemkeeper.CZKEMClass connecter;
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
        private string uri = SynData.Properties.Settings.Default.api;
        [JsonIgnore]
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion
        #region Methods
        public WorkSpace(Location lcLocal, string strIP, int iPort, MachineType mtype)
        {
            local = lcLocal;
            attMachineIp = strIP;
            attMachinePort = iPort;
            attMachineType = mtype;
            connecter = new zkemkeeper.CZKEMClass();
            lstAtt = new List<Attendance>();
            //reportmail = new MailClient();
        }
        private bool ConnectDevice()
        {
            try
            {
                if (connecter.Connect_Net(attMachineIp, attMachinePort))
                {
                    connecter.RegEvent(1, 65535);
                    connecter.EnableDevice(1, false);
                    begin = DateTime.Now;
                    return true;
                }
                else
                {
                    int iError = 0;
                    connecter.GetLastError(ref iError);
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
        private bool DisconnectDivice()
        {
            try
            {
                connecter.EnableDevice(1, true);
                connecter.Disconnect();
                end = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                log.Error("DisconnectDivice | " + attMachineIp + " | " + ex.Message);
                return false;
            }
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
                if (connecter.ReadGeneralLogData(1))
                {
                    if (attMachineType == MachineType.BlackNWhite)
                    {
                        while (connecter.GetGeneralLogData(1, ref idwTMachineNumber, ref idwEnrollNumber, ref idwEMachineNumber, ref idwVerifyMode,
                            ref idwInOutMode, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute))
                        {
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
                        while (connecter.SSR_GetGeneralLogData(1, out sdwEnrollNumber, out idwVerifyMode,
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
                PushCount = 0;
                string strJSON = string.Empty;
                foreach (var item in lstAtt.Where(x => x.pushed == false))
                {
                    if (PushCount != 0)
                    {
                        strJSON += ",";
                    }
                    strJSON += "{\"EnrollNumber\": " + item.EnrollNumber
                        + ", \"date\": \"" + item.date.ToString("yyyy-MM-dd HH:mm") + "\"}";
                    PushCount++;
                }
                strJSON = "{\"workspace_id\": " + ((int)local).ToString() + ", \"data\": [ " + strJSON + " ] }";
                strJSON = "{\"attention_data\": [ " + strJSON + " ]}";
                StringContent stringContent = new StringContent(strJSON, UnicodeEncoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
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
                    DisconnectDivice();
                    SendData();
                    log.Info(attMachineIp + " | " + ErrorMess + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
                }

            }
            catch (Exception ex)
            {
                log.Error("SynDaily | " + ex.Message);
            }
        }
        public bool SynManual(DateTime dtFrom, DateTime dtTo)
        {
            try
            {
                if (ConnectDevice())
                {
                    GetData(dtFrom, dtTo);
                    DisconnectDivice();
                    SendData();
                    log.Info("SynManual | " + attMachineIp + " | " + ErrorMess + " | " + PushCount.ToString() + " | " + begin.ToLongTimeString() + " | "
                        + end.ToLongTimeString() + " | " + (end - begin).TotalSeconds.ToString());
                    lstAtt.Clear();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("SynManual | " + ex.Message);
                return false;
            }
        }
        #endregion
    }
}
