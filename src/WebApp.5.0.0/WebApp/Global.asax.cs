using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Facilities.AutoTx;
using Castle.Windsor;
using Castle.Windsor.Installer;
using com.Sconit.Web.Filters;
using com.Sconit.Web.Pluming;

namespace com.Sconit.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class Global : HttpApplication, IContainerAccessor
    {
        private static IWindsorContainer container;

        public IWindsorContainer Container
        {
            get { return container; }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            BootstrapContainer();
            //com.Sconit.Web.Installer.AutoMapperInstaller.Install();
            //com.Sconit.Service.AutoMapperInstaller.Install();
            ViewEngines.Engines.Add(new SconitRazorViewEngine());
            ViewEngines.Engines.Add(new SconitWebFormViewEngine());
        }

        protected void Application_End()
        {
            container.Dispose();
        }

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ExceptionFilter());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //   "CustomRoute",
            //   "{controller}/{Action}/{page}/{orderBy}/{filter}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Account", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        private static void BootstrapContainer()
        {
            container = new WindsorContainer();
            container.AddFacility("transactionmanagement", new TransactionFacility());
            container.Install(Configuration.FromAppConfig());
            container.Install(FromAssembly.Named("com.Sconit.Persistence"));
            container.Install(FromAssembly.Named("com.Sconit.Service"));
            container.Install(FromAssembly.This());
            var controllerFactory = new WindsorControllerFactory(container);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);           
        }
    }
} 