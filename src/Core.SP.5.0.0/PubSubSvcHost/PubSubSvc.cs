using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Threading;
using Castle.Windsor;
using com.Sconit.Service;
using com.Sconit.Services.SP;
using com.Sconit.Services.SP.Impl;
using Castle.Windsor.Installer;

namespace PubSubSvcHost
{
    partial class PubSubSvc : ServiceBase
    {
        private static log4net.ILog log;
        private static IWindsorContainer container;
        ServiceHost _publishServiceHost = null;
        ServiceHost _subscribeServiceHost = null;

        //ServiceHost _queryHost = null;

        public PubSubSvc()
        {
            InitializeComponent();
            container = new WindsorContainer();
            //container.Install(Configuration.FromAppConfig());
            container.Install(FromAssembly.This());
            log = log4net.LogManager.GetLogger("Log.PubSubSvc");
        }

        protected override void OnStart(string[] args)
        {
            log.Info("PubSubSvc started " + DateTime.Now.ToString());
            
            Publishing pub = new Publishing();
            Thread thread = new Thread(pub.HeartBeatLink);
            thread.Start();

            // TODO: Add code here to start your service.
            try
            {
                ServiceHost host = new ServiceHost(typeof(QuerySvc));

                host.AddServiceEndpoint(typeof(IQuerySvc), new WSHttpBinding(), "http://127.0.0.1:9999/QuerySvc");
                if (host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                {
                    ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                    behavior.HttpGetEnabled = true;
                    behavior.HttpGetUrl = new Uri("http://127.0.0.1:9999/QuerySvc/metadata");
                    host.Description.Behaviors.Add(behavior);
                }
                host.Open();

                HostPublishService();
                HostSubscriptionService();
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        protected override void OnStop()
        {
            log.Info("PubSubSvc stopped " + DateTime.Now.ToString());
            this.Dispose();
            GC.Collect();
        }

        private void HostSubscriptionService()
        {
            _subscribeServiceHost = new ServiceHost(typeof(Subscription));
            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None);
            tcpBinding.ReceiveTimeout = TimeSpan.FromHours(24);
            _subscribeServiceHost.AddServiceEndpoint(typeof(ISubscription), tcpBinding,
                    "net.tcp://localhost:7002/Sub");
            _subscribeServiceHost.Open();
        }

        private void HostPublishService()
        {
            _publishServiceHost = new ServiceHost(typeof(Publishing));
            NetTcpBinding tcpBindingpublish = new NetTcpBinding(SecurityMode.None);
            tcpBindingpublish.ReceiveTimeout = TimeSpan.FromHours(24);
            _publishServiceHost.AddServiceEndpoint(typeof(IPublishing), tcpBindingpublish,
                                    "net.tcp://localhost:7001/Pub");
            _publishServiceHost.Open();
        }
    }
}
