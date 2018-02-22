using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WSM.SynData
{
    public class MailClient
    {
        #region Property
        public string mailserver;
        public string username;
        public string password;
        public int serverport;
        public bool issshserver;
        public string receiver;
        [JsonIgnore]
        public string from;
        [JsonIgnore]
        public string to;
        [JsonIgnore]
        public string title;
        [JsonIgnore]
        public string content;
        #endregion
        #region Methods
        public MailClient()
        {
            // Debug code
            mailserver = "smtp.gmail.com";
            serverport = 587;
            issshserver = true;
            username = "viet456@gmail.com";
            password = "LetitGo146";
            receiver = "viet456@gmail.com,mtviet@gmail.com,mai.tuan.viet@framgia.com";
            // Debug code
        }
        public MailClient(string strMailServer, int iServerPort, bool isSSHServer, string strUserName, string strPassword, string strReceiver)
        {
            mailserver = strMailServer;
            serverport = iServerPort;
            issshserver = isSSHServer;
            username = strUserName;
            password = strPassword;
            receiver = strReceiver;
        }
        public bool SendMail(string strTitle, string strContent)
        {
            try
            {
                from = username;
                to = receiver;
                title = strTitle;
                content = strContent;
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(mailserver);

                mail.From = new MailAddress(from);
                mail.To.Add(to);
                mail.Subject = title;
                mail.Body = content;

                SmtpServer.Port = serverport;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);
                SmtpServer.EnableSsl = issshserver;

                SmtpServer.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }
        #endregion
    }
}
