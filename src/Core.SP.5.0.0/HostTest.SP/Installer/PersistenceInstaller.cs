using Castle.Facilities.AutoTx;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NHibernate;
using com.Sconit.Persistence;

namespace com.Sconit.Web.Installer
{
    public class PersistenceInstaller : IWindsorInstaller
    {

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //RegisterTransactionFacility(container);
            RegisterPersistence(container);
        }

        //protected virtual void RegisterTransactionFacility(IWindsorContainer container)
        //{
        //    container.AddFacility<TransactionFacility>();
        //}

        private void RegisterPersistence(IWindsorContainer container)
        {
            container.Register(AllTypes.FromAssemblyNamed("com.Sconit.Persistence")
                .Pick().If(t => t.Name.EndsWith("Dao") && !t.Name.Equals("NHQueryDao") && !t.Name.Equals("SqlDao"))
                .Configure(c => c.LifeStyle.Singleton)
                .WithService.DefaultInterface()
                );

            Parameter p = Parameter.ForKey("sessionFactoryAlias").Eq("sub");
            container.Register(Component.For<INHQueryDao>()
                .LifeStyle.Singleton
                .Parameters(p)
                .ImplementedBy<NHQueryDao>()
               );
        }
    }
}
