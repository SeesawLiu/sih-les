using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using NHibernate.Cfg;
using NHibernate;
using com.Sconit.Entity.SP.ORD;

namespace com.Sconit.Persistence.SP
{
    public class OrderMasterDao
    {
        private ISession _session;
        private SessionFactory _sessionfactory = new SessionFactory();

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="name">用户名称</param>
        /// <param name="description">用户描述</param>
        /// <param name="state">状态</param>
        /// <returns>True-操作成功|False-操作失败</returns>
        public bool AddOrderMaster(OrderMaster orderMaster)
        {
            if (!ExistOrderMaster(orderMaster.OrderNo))
            {
                //Model.Message userinfo = new Model.Message
                //{   
                //    Color = (Message.ColorEnum)color,
                //    Type = (Extention.TypeEnum)type,
                //    TopicName = topicName,
                //    EventData = eventData
                //    //Name = name,
                //    //Description = description,
                //    //State = state
                //};
                using (_session = _sessionfactory.Session)
                {
                    _session.Save(orderMaster);
                    _session.Flush();
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// 检查用户是否存在
        /// </summary>
        /// <param name="name">用户名称</param>
        /// <returns>True-用户存在|False-用户不存在</returns>
        public bool ExistOrderMaster(string orderNo)
        {
            bool result = false;
            string hql = "select count(*) from OrderMaster where OrderNo=:orderNo";
            using (_session = _sessionfactory.Session)
            {

                IQuery query = _session.CreateQuery(hql);
                query.SetString("orderNo", orderNo);
                result = (int.Parse(query.UniqueResult().ToString()) == 0) ? false : true;
            }
            return result;
        }
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="name">用户名称</param>
        /// <param name="description">用户描述</param>
        /// <param name="state">状态</param>
        /// <returns>True-操作成功|False-操作失败</returns>
        public bool UpdateOrderMaster(string name, string description, string state)
        {
            bool result = false;
            if (ExistOrderMaster(name))
            {
                string hql = "update UserInfo set Description=:description,State=:state where Name=:name";
                using (_session = _sessionfactory.Session)
                {
                    IQuery query = _session.CreateQuery(hql);
                    query.SetString("name", name);
                    query.SetString("description", description);
                    query.SetString("state", state);
                    result = (query.ExecuteUpdate() > 0) ? true : false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
