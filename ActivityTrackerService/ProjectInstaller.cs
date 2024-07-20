using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ActivityTrackerService
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            // Instantiate the installers
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // Set the account under which the service will run
            processInstaller.Account = ServiceAccount.User;


            // Set the service properties
            serviceInstaller.ServiceName = "Activity Tracker";
            serviceInstaller.DisplayName = "CCS Activity Tracker";
            serviceInstaller.Description = "Saves a written log of used apps";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            // Add installers to the Installers collection
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
