using System;
using System.Linq;
using System.ServiceProcess;
using System.Reflection;
using System.Configuration.Install;
using System.Configuration;

namespace DnsUpdater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (System.Environment.UserInteractive)    //If process is user run, (un)install.  If auto-run, start the process.
            {
                ServiceController sc = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == "DNS Updater");
                Console.WriteLine("Commands are:\nINSTALL\nUNINSTALL\nSTART\nSTOP\nEXIT");
                Console.WriteLine("Service is " + (sc == null ? "not installed." : sc.Status.ToString()));
                while (true)
                {
                    Console.Write("> ");
                    string cmd = Console.ReadLine();
                    switch (cmd.ToLower())
                    {
                        case "install":
                            if (sc != null)
                            {
                                Console.WriteLine("Service is already installed");
                                break;
                            }
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetAssembly(typeof(Service)).Location });
                            sc = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == "DNS Updater");
                            break;
                        case "uninstall":
                            if (sc == null)
                            {
                                Console.WriteLine("Service is already not installed");
                                break;
                            }
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetAssembly(typeof(Service)).Location });
                            sc = null;
                            break;
                        case "start":
                            if (sc == null)
                            {
                                Console.WriteLine("Service is not yet installed.");
                            }
                            if (sc.Status == ServiceControllerStatus.StartPending || sc.Status == ServiceControllerStatus.Running)
                            {
                                Console.WriteLine("Service is already started or pending start.");
                                break;
                            }
                            sc.Start();
                            break;
                        case "stop":
                            if (sc == null)
                            {
                                Console.WriteLine("Service is not yet installed.");
                                break;
                            }
                            if (sc.Status == ServiceControllerStatus.StopPending || sc.Status == ServiceControllerStatus.Stopped)
                            {
                                Console.WriteLine("Service is already stopped or pending stop.");
                                break;
                            }
                            sc.Stop();
                            break;
                        case "exit":
                            return;
                        default:
                            Console.WriteLine("Invalid command.");
                            break;
                    }
                }
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new Service() });
            }
        }
    }
}
