using System.ServiceModel;
using com.Sconit.Services.SP.Impl;
using com.Sconit.Services.SP;
using System;
using com.Sconit.Service;
using System.ServiceModel.Description;
using System.Threading;

namespace SP.HostTest
{
    class Program
    {

        ServiceHost _publishServiceHost = null;
        ServiceHost _subscribeServiceHost = null;

        //ServiceHost _queryHost = null;

        static void Main(string[] args)
        {
            var program = new Program();
            program.Run();

            Console.ReadLine();
        }

        public void Run()
        {
            try
            {

                //ServiceHost host = new ServiceHost(typeof(QuerySvc));
                
                //host.AddServiceEndpoint(typeof(IQuerySvc), new WSHttpBinding(), "http://127.0.0.1:9999/QuerySvc");
                //if (host.Description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                //{
                //    ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                //    behavior.HttpGetEnabled = true;
                //    behavior.HttpGetUrl = new Uri("http://127.0.0.1:9999/QuerySvc/metadata");
                //    host.Description.Behaviors.Add(behavior);
                //}
                //host.Open();
                Publishing pub = new Publishing();
                Thread thread = new Thread(pub.HeartBeatLink);
                thread.Start();

                HostPublishService();
                HostSubscriptionService();
            }
            catch
            {

            }
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
