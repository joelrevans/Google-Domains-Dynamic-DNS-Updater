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
using System.Xml.Serialization;
using System.Xml;


namespace DnsUpdater
{
    public partial class Service : ServiceBase
    {
        private IPAddress lastIP = new IPAddress(0);
        Timer timer = new Timer(Properties.Settings.Default.UpdateInterval);


        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog.Source = "DNS Updater";

            timer.Elapsed += Cycle;
            timer.Enabled = true;
            timer.Start();

            Cycle(null, null);
        }

        public void Cycle(Object source, ElapsedEventArgs e)
        {
            IPAddress IP = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    IP = IPAddress.Parse(wc.DownloadString("https://domains.google.com/checkip"));
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Failed to connect to the DNS service, or failure to parse response.\n\n" + ex.Message);
                return;
            }

            if (IP.ToString() != lastIP.ToString())
            {
                EventLog.WriteEntry("Stored IP address is being changed from " + lastIP.ToString() + " to " + IP.ToString());

                lastIP = IP;
                
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
