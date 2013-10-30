using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using com.Sconit.Service.Impl;
using com.Sconit.Service;

namespace com.Sconit.Web.Installer
{
    public class ServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(AllTypes.FromAssemblyNamed("com.Sconit.Service")
                .Pick().If(t => t.Name.EndsWith("MgrImpl") && !t.Name.Equals("SMSMgrImpl") && !t.Name.Equals("PubSubMgrImpl"))
                .Configure(c => c.LifeStyle.Singleton)
                .WithService.DefaultInterface()
                );

            //container.Register( Component.For<ISMSMgr>().ImplementedBy<SMSMgrImpl>().Parameters(Parameter.ForKey("SMSAccountId").Eq("${SMSAccountId}")));

            //container.Register(AllTypes.FromAssemblyNamed("com.Sconit.SDService")
            //    .Pick().If(t => t.Name.EndsWith("ServiceImpl"))
            //    .Configure(c => c.LifeStyle.Singleton)
            //    .WithService.DefaultInterface()
            //    );
        }
    }
}