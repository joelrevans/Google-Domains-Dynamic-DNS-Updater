using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Timers;


namespace DnsUpdater
{
    public partial class Service : ServiceBase
    {
        private IPAddress lastIP = null;
        Timer timer = new Timer(Properties.Settings.Default.UpdateInterval);
        
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog.Source = "DNS Updater";

            try
            {
                using (WebClient wc = new WebClient())
                {
                    lastIP = IPAddress.Parse(wc.DownloadString("https://domains.google.com/checkip"));
                    EventLog.WriteEntry("Current IP address set to " + lastIP.ToString());
                }
            }catch(Exception ex)
            {
                EventLog.WriteEntry("Failed to connect to the DNS service, or failure to parse response.\n\n" + ex.Message);
                return;
            }

            timer.Elapsed += Cycle;
            timer.Enabled = true;
            timer.Start();

            Cycle(null, null);
        }

        public void Cycle(Object source, ElapsedEventArgs e)
        {
            NetworkInterface card = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
            if (card == null)
            {
                EventLog.WriteEntry("No network card found.", EventLogEntryType.Error);
                return;
            }

            IPAddress GatewayAddress = null ;
            try
            {
                GatewayIPAddressInformation address = card.GetIPProperties().GatewayAddresses.FirstOrDefault();
                GatewayAddress = address.Address;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("An error occurred while retreiving the Gateway Address.\n\n" + ex.Message, EventLogEntryType.Error);
                return;
            }

            string gatewayPage = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Credentials = new NetworkCredential(Properties.Settings.Default.GatewayUser, Properties.Settings.Default.GatewayPass);
                    gatewayPage = wc.DownloadString("http://" + GatewayAddress.ToString() + "/userRpm/StatusRpm.htm");
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Failure to retrieve public IP information from gateway.\n\n" + ex.Message, EventLogEntryType.Warning);
                return;
            }

            Regex getIPLine = new Regex("wanPara[^,]*,[^,]*,[^,]*");
            Match IPLine = getIPLine.Match(gatewayPage);
            Regex getIP = new Regex(@"\d+\.\d+\.\d+\.\d+");
            Match IP = getIP.Match(IPLine.Value);

            if (IP.Value != lastIP.ToString())
            {
                lastIP = IPAddress.Parse(IP.Value);

                //https://Jf7PFy6g7zDHgAWp:pqYrZhfPnH8u8Eey@domains.google.com/nic/update?hostname=joelrevans.com

                foreach (DDNSEntry ddne in Properties.Settings.Default.DDNSEntries)
                {
                    string response = null;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Credentials = new NetworkCredential(ddne.Username, ddne.Password);
                        response = wc.DownloadString("https://domains.google.com/nic/update?hostname=" + ddne.HostName);
                    }

                    EventLog.WriteEntry("Response for request to update DNS: \n\n" + response, EventLogEntryType.Information);
                }
            }
                
        }

        protected override void OnStop()
        {
            timer.Stop();
        }
    }
}
