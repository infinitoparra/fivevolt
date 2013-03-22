
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ZigBeeCoordinator.Service
{
    [RunInstaller(true)]
    public class ZigBeeCoordinatorServiceInstaller : Installer
    {
        public ZigBeeCoordinatorServiceInstaller()
        {
            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "ZigBee Coordinator";
            serviceInstaller.DelayedAutoStart = true;
            serviceInstaller.Description = "ZigBee Coordinator - Monitors ZigBee traffic and works with the website to provide that information";

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}
