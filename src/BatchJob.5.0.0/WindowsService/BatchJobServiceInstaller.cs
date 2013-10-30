using System.ComponentModel;
using System.ServiceProcess;
using com.Sconit.BatchJob.WindowsService.Properties;

namespace com.Sconit.BatchJob.WindowsService
{
    [RunInstaller(true)]
    public partial class BatchJobServiceInstaller : System.Configuration.Install.Installer
    {
        public BatchJobServiceInstaller()
        {
            InitializeComponent();
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = Settings.Default.ServiceDisplayName;
            serviceInstaller.Description = Settings.Default.ServiceDisplayDescription;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //# This must be identical to the WindowsService.ServiceBase name
            //# set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = Settings.Default.ServiceName;

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
