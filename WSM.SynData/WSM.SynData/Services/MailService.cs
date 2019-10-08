using Newtonsoft.Json;
using WSM.SynData.Models;

namespace WSM.SynData.Services
{
    public class MailService
    {
        public MailClient GetMailClient()
        {
            try
            {
                return JsonConvert.DeserializeObject<MailClient>(Properties.Settings.Default.mailreport);
            }
            catch
            {
                return null;
            }
        }
    }
}
