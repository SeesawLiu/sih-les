using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using AutoMapper;
using com.Sconit.Persistence.SP;
using com.Sconit.PrintModel;
using com.Sconit.Service;
using com.Sconit.PrintModel.ORD;
using System.Threading;
using com.Sconit.PrintModel.INV;
//using com.Sconit.Entity.SP;

namespace com.Sconit.Services.SP.Impl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Publishing : IPublishing
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.PubSubSvc");

        #region IPublishing Members
        public void Publish(PrintBase o)
        {
            string subType;
            //NHDao Dao = new NHDao();
            List<IPublishing> subscribers = new List<IPublishing>();
            if (o.GetType() == typeof(PrintOrderMaster))
            {
                PrintOrderMaster orderMaster = (PrintOrderMaster)o;
                if (orderMaster.Type == 1)
                {
                    subType = "ORD_Procurement";
                    log.Debug(string.Format("Publish Order: subType = [{0}], Flow = [{1}], Region = [{2}], UserNm = [{3}].", subType, orderMaster.Flow, orderMaster.PartyTo, orderMaster.CreateUserName));
                    subscribers = Filter.GetSubscribers(subType, orderMaster.Flow, orderMaster.PartyTo, orderMaster.CreateUserName);
                }
                else if (orderMaster.Type == 2)
                {
                    subType = "ORD_Transfer";
                    log.Debug(string.Format("Publish Order: subType = [{0}], Flow = [{1}], Region = [{2}], UserNm = [{3}].", subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName));
                    subscribers = Filter.GetSubscribers(subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName);
                }
                else if (orderMaster.Type == 3)
                {
                    subType = "ORD_Distribution";
                    log.Debug(string.Format("Publish Order: subType = [{0}], Flow = [{1}], Region = [{2}], UserNm = [{3}].", subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName));
                    subscribers = Filter.GetSubscribers(subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName);
                }
                else if (orderMaster.Type == 4)
                {
                    subType = "ORD_Production";
                    log.Debug(string.Format("Publish Order: subType = [{0}], Flow = [{1}], Region = [{2}], UserNm = [{3}].", subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName));
                    subscribers = Filter.GetSubscribers(subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName);
                }
                else if (orderMaster.Type == 5)
                {
                    subType = "ORD_SubContract";
                    log.Debug(string.Format("Publish Order: subType = [{0}], Flow = [{1}], Region = [{2}], UserNm = [{3}].", subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName));
                    subscribers = Filter.GetSubscribers(subType, orderMaster.Flow, orderMaster.PartyFrom, orderMaster.CreateUserName);
                }
                else if (orderMaster.Type == 6)
                {
                    subType = "ORD_CustomerGoods";
                    log.Debug(string.Format("Publish Order: subType = [{0}], Flow = [{1}], Region = [{2}], UserNm = [{3}].", subType, orderMaster.Flow, orderMaster.PartyTo, orderMaster.CreateUserName));
                    subscribers = Filter.GetSubscribers(subType, orderMaster.Flow, orderMaster.PartyTo, orderMaster.CreateUserName);
                }
                //Dao.Create(orderMaster);
                //if (orderMaster.OrderDetails.Count > 0)
                //{
                //    foreach (var item in orderMaster.OrderDetails)
                //    {
                //        Dao.Create(item);
                //    }
                //}
            }
            else if (o.GetType() == typeof(PrintIpMaster))
            {
                PrintIpMaster ipMaster = (PrintIpMaster)o;
                log.Debug(string.Format("Publish Ip: Region = [{0}], UserNm = [{1}].", ipMaster.PartyFrom, ipMaster.CreateUserName));
                subType = "ASN";
                subscribers = Filter.GetSubscribers(subType, "", ipMaster.PartyFrom, ipMaster.CreateUserName);
            }
            else if (o.GetType() == typeof(PrintPickListMaster))
            {
                PrintPickListMaster plMaster = (PrintPickListMaster)o;
                log.Debug(string.Format("Publish PickList: Region = [{0}], UserNm = [{1}].", plMaster.PartyFrom, plMaster.CreateUserName));
                subType = "PIK";
                subscribers = Filter.GetSubscribers(subType, "", plMaster.PartyFrom, plMaster.CreateUserName);
            }
            else if (o.GetType() == typeof(PrintReceiptMaster))
            {
                PrintReceiptMaster recMaster = (PrintReceiptMaster)o;
                log.Debug(string.Format("Publish Receipt: Region = [{0}], Flow = [{1}], UserNm = [{2}].", recMaster.PartyTo, recMaster.Flow, recMaster.CreateUserName));
                subType = "REC";
                subscribers = Filter.GetSubscribers(subType, recMaster.Flow, recMaster.PartyTo, recMaster.CreateUserName);
            }
            else if (o.GetType() == typeof(PrintSequenceMaster))
            {
                PrintSequenceMaster seqMaster = (PrintSequenceMaster)o;
                log.Debug(string.Format("Publish Sequence: Flow = [{0}], Region = [{1}], UserNm = [{2}].", seqMaster.Flow, seqMaster.PartyFrom, seqMaster.CreateUserName));
                subType = "SEQ";
                subscribers = Filter.GetSubscribers(subType, seqMaster.Flow, seqMaster.PartyFrom, seqMaster.CreateUserName);
            }
            else if (o.GetType() == typeof(PrintHu))
            {
                PrintHu hu = (PrintHu)o;
                log.Debug(string.Format("Publish Hu: UserNm = [{0}].", hu.CreateUserName));
                subType = "CloneHu";
                subscribers = Filter.GetSubscribers(subType, "", "", hu.CreateUserName);
            }

            if (subscribers == null) 
                return;

            Type type = typeof(IPublishing);
            MethodInfo publishMethodInfo = type.GetMethod("Publish");

            foreach (IPublishing subscriber in subscribers)
            {
                try
                {
                    publishMethodInfo.Invoke(subscriber, new object[] { o });
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }
        }

        public void HeartBeatLink()
        {
            PrintBase heartBeatObj = new PrintBase();
            var loseHeartBeatClientList = new Dictionary<string, SubscriptionDetail>();
            Type type = typeof(IPublishing);
            MethodInfo publishMethodInfo = type.GetMethod("Publish");
            while (true)
            {
                loseHeartBeatClientList = new Dictionary<string, SubscriptionDetail>();
                Thread.Sleep(6000);
                foreach (var item in Filter.HeartBeatList)
	            {
                    try
                    {
                        publishMethodInfo.Invoke(item.Value.CallBackInstance, new object[] { heartBeatObj });
                    }
                    catch (Exception ex)
                    {
                        item.Value.Count++;
                        log.Error(ex);
                    }
                    if (item.Value.Count > 3)
                    {
                        Filter.RemoveOneClientSubscriber(item.Key);
                        loseHeartBeatClientList.Add(item.Key, item.Value);
                        //remove this client subscriber
                    }
	            }
                //var loseHeartBeatClientList = Filter.HeartBeatList.Where(p => p.Value.Count > 3);
                foreach (var item in loseHeartBeatClientList)
                {
                    lock (Filter.HeartBeatList)
                    {
                        log.Info(string.Format("Subscriber time out, item key = [item.Key].", item.Key));
                        Filter.HeartBeatList.Remove(item.Key);
                    }
                }
            }
        }

        #endregion
    }
}

