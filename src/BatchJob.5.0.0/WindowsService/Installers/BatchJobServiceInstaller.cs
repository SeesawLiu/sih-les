using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using com.Sconit.BatchJob.Job;

namespace com.Sconit.BatchJob.WindowsService.Installer
{
    public class BatchJobServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(AllTypes.FromAssemblyNamed("com.Sconit.Service.BatchJob")
               .Pick().If(t => t.Name.EndsWith("MgrImpl"))
                   .Configure(c => c.LifeStyle.Singleton)
               .WithService.DefaultInterface()
               );
        }
    }
}
