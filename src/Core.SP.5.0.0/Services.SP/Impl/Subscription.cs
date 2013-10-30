using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
//using com.Sconit.Entity.SP;
using com.Sconit.Service;
using com.Sconit.PrintModel;

namespace com.Sconit.Services.SP.Impl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Subscription : ISubscription
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("Log.PubSubSvc");

        #region ISubscription Members

        public void Subscribe(string GUID, string subType, string flow, string region, string userNm)
        {
            log.Info(string.Format("Subscribe Info: GUID = [{0}], subType = [{1}], flow = [{2}], region = [{3}], userNm = [{4}].", GUID, subType, flow, region, userNm));
            IPublishing subscriber = OperationContext.Current.GetCallbackChannel<IPublishing>();
            Filter.AddSubscriber(GUID, subType, flow, region, userNm, subscriber);
        }

        public void UnSubscribe(string GUID, string subType, string flow, string region, string userNm)
        {
            log.Info(string.Format("UnSubscribe Info: GUID = [{0}], subType = [{1}], flow = [{2}], region = [{3}], userNm = [{4}].", GUID, subType, flow, region, userNm));
            IPublishing subscriber = OperationContext.Current.GetCallbackChannel<IPublishing>();
            Filter.RemoveSubscriber(GUID, subType, flow, region, userNm, subscriber);
        }

        #endregion
    }

    public class Filter
    {
        //static Dictionary<SubCondition, List<IPublishing>> _subscribersList = new Dictionary<SubCondition, List<IPublishing>>();
        static Dictionary<string, SubscriptionDetail> _subscribersList = new Dictionary<string, SubscriptionDetail>();
        static Dictionary<string, SubscriptionDetail> _heartBeatList = new Dictionary<string, SubscriptionDetail>();

        static public Dictionary<string, SubscriptionDetail> HeartBeatList
        {
            get
            {
                lock (typeof(Filter))
                {
                    return _heartBeatList;
                }
            }
        }

        static public Dictionary<string, SubscriptionDetail> SubscribersList
        {
            get
            {
                lock (typeof(Filter))
                {
                    return _subscribersList;
                }
            }
        }

        static public List<IPublishing> GetSubscribers(string subType, string flow, string region, string userNm)
        {
            List<IPublishing> subscribers = new List<IPublishing>();
            lock (typeof(Filter))
            {
                if (subType != "HuClone")
                {
                    foreach (var item in SubscribersList)
                    {
                        if (item.Value.SubType == subType && (item.Value.Flow == flow || item.Value.Region == region))
                        {
                            subscribers.Add(item.Value.CallBackInstance);
                        }
                    }
                }
                else
                {
                    foreach (var item in SubscribersList)
                    {
                        if (item.Value.UserNm == userNm)
                        {
                            subscribers.Add(item.Value.CallBackInstance);
                        }
                    }
                }
            }
            return subscribers;
        }

        static public void AddSubscriber(string GUID, string subType, string flow, string region, string userNm, IPublishing subscriberCallbackReference)
        {
            SubscriptionDetail subscriptionDetail = new SubscriptionDetail();
            subscriptionDetail.SubType = subType;
            subscriptionDetail.Flow = flow;
            subscriptionDetail.Region = region;
            subscriptionDetail.UserNm = userNm;
            subscriptionDetail.CallBackInstance = subscriberCallbackReference;

            if (subType != "")
            {
                lock (typeof(Filter))
                {
                    if (!SubscribersList.ContainsKey(GUID))
                    {
                        lock (SubscribersList)
                        {
                            SubscribersList.Add(GUID, subscriptionDetail);
                        }
                    }
                }
            }
            else
            {
                if (!_heartBeatList.ContainsKey(GUID))
                {
                    _heartBeatList.Add(GUID, subscriptionDetail);
                }
            }
        }

        static public void RemoveSubscriber(string GUID, string subType, string flow, string region, string userNm, IPublishing subscriberCallbackReference)
        {
            SubscriptionDetail subscriptionDetail = new SubscriptionDetail();
            subscriptionDetail.SubType = subType;
            subscriptionDetail.Flow = flow;
            subscriptionDetail.Region = region;
            subscriptionDetail.UserNm = userNm;
            subscriptionDetail.CallBackInstance = subscriberCallbackReference;

            lock (typeof(Filter))
            {
                if (subType == "")
                {
                    HeartBeatList.Remove(GUID);
                }
                else
                {
                    if (SubscribersList.ContainsKey(GUID))
                    {
                        lock (SubscribersList)
                        {
                            SubscribersList.Remove(GUID);
                        }
                    }
                }
            }
        }

        static public void RemoveOneClientSubscriber(string GUID)
        {
            lock (typeof(Filter))
            {
                var loseHeartBeatSubscribersList = new Dictionary<string, SubscriptionDetail>(); //SubscribersList.Where(p => p.Key.Contains(GUID));
                foreach (var item in SubscribersList)
                {
                    if (item.Key.Contains(GUID))
                    {
                        loseHeartBeatSubscribersList.Add(item.Key, item.Value);
                    }
                }
                foreach (var item in loseHeartBeatSubscribersList)
                {
                    SubscribersList.Remove(item.Key);
                }
            }
        }
    }

    public class SubscriptionDetail
    {
        public string SubType { get; set; }

        public string Flow { get; set; }

        public string Region { get; set; }

        public string UserNm { get; set; }

        public int Count { get; set; }

        public IPublishing CallBackInstance { get; set; }
    }
}