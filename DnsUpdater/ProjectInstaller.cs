using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace DnsUpdater
{
    [RunInstaller(true)]
    public partial class DNSUpdaterInstaller : Installer
    {
        public DNSUpdaterInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = "DNS Updater";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = "DNS Updater";
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
