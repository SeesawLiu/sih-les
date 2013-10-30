using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.ServiceModel;
using com.Sconit.Services.SP;
//using com.Sconit.Entity.SP.ORD;
using com.Sconit.Service;
using com.Sconit.PrintModel.ORD;

namespace PubTest
{
    class Program
    {
        IPublishing _proxy;

        static void Main(string[] args)
        {
            var program = new Program();
            program.CreateProxy();

            for (int i = 0; ; i++)
            {
                Console.WriteLine("Enter send a message");
                string topicName  = Console.ReadLine();
                if (topicName != "exit")
                {
                    program.SendEvent(int.Parse(topicName));
                }
                else
                {
                    break;
                }
            }

            Console.ReadLine();
            
        }

        private void CreateProxy()
        {
            string endpointAddressInString = ConfigurationManager.AppSettings["EndpointAddress"];
            EndpointAddress endpointAddress = new EndpointAddress(endpointAddressInString);
            //WSDualHttpBinding wsDualHttpBinding = new WSDualHttpBinding();
            //_proxy = ChannelFactory<IPublishing>.CreateChannel(wsDualHttpBinding, endpointAddress);
            NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.None);
            _proxy = ChannelFactory<IPublishing>.CreateChannel(netTcpBinding, endpointAddress);
        }

        void SendEvent(int i)
        {
            try
            {
                //string topicName = "Bangladesh";
                //if (string.IsNullOrEmpty(topicName))
                //{
                //    Console.WriteLine("Please Enter a Topic Name");
                //    return;
                //}
                
                PrintOrderMaster alertData = PrepareEvent(i);
                _proxy.Publish(alertData);
                
            }
            catch { }
        }

        private PrintOrderMaster PrepareEvent(int i)
        {
            PrintOrderMaster orderMaster = new PrintOrderMaster();
            if (i == 1)
            {
                orderMaster.OrderNo = "a12345678";
                orderMaster.CreateDate = DateTime.Now;
                orderMaster.PartyFrom = "partfrom";
                orderMaster.PartyTo = "vv";
                orderMaster.WindowTime = DateTime.Now;
                orderMaster.StartTime = DateTime.Now;
                orderMaster.ReleaseDate = DateTime.Now;
                //orderMaster.LastModifyDate = DateTime.Now;
                //orderMaster.EffDate = DateTime.Now;
                orderMaster.CreateUserId = 1;
                //orderMaster.LastModifyUserId = 1;
                //orderMaster.OrderStrategy = com.Sconit.CodeMaster.FlowStrategy.ANDON;
                orderMaster.Flow = "aa";
                orderMaster.Type = 1;
                //orderMaster.SubType = com.Sconit.CodeMaster.OrderSubType.Normal;
                orderMaster.Priority = 0;     
            }
            else if (i == 2)
            {
                orderMaster.OrderNo = "b12345678";
                orderMaster.CreateDate = DateTime.Now;
                orderMaster.PartyFrom = "partfrom";
                orderMaster.PartyTo = "sd";
                orderMaster.WindowTime = DateTime.Now;
                orderMaster.StartTime = DateTime.Now;
                orderMaster.ReleaseDate = DateTime.Now;
                //orderMaster.LastModifyDate = DateTime.Now;
                //orderMaster.EffDate = DateTime.Now;
                orderMaster.CreateUserId = 1;
                //orderMaster.LastModifyUserId = 1;
                orderMaster.OrderStrategy = 1;
                orderMaster.Flow = "aasdsd";
                orderMaster.Type = 2;
                orderMaster.SubType = 3;
                orderMaster.Priority = 0; 
 
            }
            else if (i == 3)
            {
                orderMaster.OrderNo = "c12345678";
                orderMaster.CreateDate = DateTime.Now;
                orderMaster.PartyFrom = "partfrom";
                orderMaster.PartyTo = "2222";
                orderMaster.WindowTime = DateTime.Now;
                orderMaster.StartTime = DateTime.Now;
                orderMaster.ReleaseDate = DateTime.Now;
                //orderMaster.LastModifyDate = DateTime.Now;
                //orderMaster.EffDate = DateTime.Now;
                orderMaster.CreateUserId = 1;
                //orderMaster.LastModifyUserId = 1;
                orderMaster.OrderStrategy = 1;
                orderMaster.Flow = "111";
                orderMaster.Type = 2;
                orderMaster.SubType = 3;
                orderMaster.Priority = 0; 

            }
            return orderMaster;
        }

    }
}
