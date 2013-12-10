

namespace com.Sconit.Service.Impl
{
    #region retrive
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Castle.Services.Transaction;
    using com.Sconit.Entity;
    using com.Sconit.Entity.ACC;
    using com.Sconit.Entity.SAP;
    using com.Sconit.Persistence;
    using NHibernate.Criterion;
    using NHibernate.Type;
    using System.Linq;
    using System.Text;

    #endregion

    /// <summary>
    /// 
    /// </summary>
    [Transactional]
    public class GenericMgrImpl : BaseMgr, IGenericMgr, IQueryMgr
    {
        public GenericMgrImpl(INHDao dao)
        {
            this.dao = dao;
        }
        /// <summary>
        /// NHibernate数据获取对象
        /// </summary>
        private INHDao dao { get; set; }
        public ISqlDao sqlDao { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        [Transaction(TransactionMode.Requires)]
        public void Save(object instance)
        {
            IAuditable auditable = instance as IAuditable;
            if (auditable != null)
            {
                DateTime dateTimeNow = DateTime.Now;
                User user = SecurityContextHolder.Get();
                if (auditable.CreateUserName == null)
                {
                    auditable.CreateUserId = user.Id;
                    auditable.CreateUserName = user.FullName;
                    auditable.CreateDate = dateTimeNow;
                }

                auditable.LastModifyUserId = user.Id;
                auditable.LastModifyUserName = user.FullName;
                auditable.LastModifyDate = dateTimeNow;
            }
            dao.Save(instance);
        }

        [Transaction(TransactionMode.Requires)]
        public void Create(object instance)
        {
            IAuditable auditable = instance as IAuditable;

            if (auditable != null)
            {
                DateTime dateTimeNow = DateTime.Now;
                User user = SecurityContextHolder.Get();
                if (user != null)
                {
                    auditable.CreateUserId = user.Id;
                    auditable.CreateUserName = user.FullName;
                    auditable.LastModifyUserId = user.Id;
                    auditable.LastModifyUserName = user.FullName;
                }
                auditable.CreateDate = dateTimeNow;
                auditable.LastModifyDate = dateTimeNow;
            }

            ITraceable traceable = instance as ITraceable;
            if (traceable != null)
            {
                DateTime dateTimeNow = DateTime.Now;
                traceable.Status = StatusEnum.Pending;
                traceable.ErrorCount = 0;
                traceable.CreateDate = dateTimeNow;
                traceable.LastModifyDate = dateTimeNow;
            }

            dao.Create(instance);
        }

        [Transaction(TransactionMode.Requires)]
        public void Update(object instance)
        {
            IAuditable auditable = instance as IAuditable;
            if (auditable != null)
            {
                DateTime dateTimeNow = DateTime.Now;
                User user = SecurityContextHolder.Get();
                auditable.LastModifyUserId = user.Id;
                auditable.LastModifyUserName = user.FullName;
                auditable.LastModifyDate = dateTimeNow;
            }

            ITraceable traceable = instance as ITraceable;
            if (traceable != null)
            {
                traceable.LastModifyDate = DateTime.Now;
            }
            dao.Update(instance);
        }

        [Transaction(TransactionMode.Requires)]
        public void MergeUpdate(object instance)
        {
            IAuditable auditable = instance as IAuditable;
            if (auditable != null)
            {
                DateTime dateTimeNow = DateTime.Now;
                User user = SecurityContextHolder.Get();
                auditable.LastModifyUserId = user.Id;
                auditable.LastModifyUserName = user.FullName;
                auditable.LastModifyDate = dateTimeNow;
            }

            ITraceable traceable = instance as ITraceable;
            if (traceable != null)
            {
                traceable.LastModifyDate = DateTime.Now;
            }
            dao.MergeUpdate(instance);
        }

        [Transaction(TransactionMode.Requires)]
        public int Update(string queryString)
        {
            return dao.ExecuteUpdateWithCustomQuery(queryString, (object[])null, (IType[])null);
        }

        [Transaction(TransactionMode.Requires)]
        public int Update(string queryString, object value)
        {
            return dao.ExecuteUpdateWithCustomQuery(queryString, new object[] { value }, (IType[])null);
        }

        [Transaction(TransactionMode.Requires)]
        public int Update(string queryString, object value, IType type)
        {
            return dao.ExecuteUpdateWithCustomQuery(queryString, new object[] { value }, new IType[] { type });
        }

        [Transaction(TransactionMode.Requires)]
        public int Update(string queryString, object[] values)
        {
            return dao.ExecuteUpdateWithCustomQuery(queryString, values, (IType[])null);
        }

        [Transaction(TransactionMode.Requires)]
        public int Update(string queryString, object[] values, IType[] types)
        {
            return dao.ExecuteUpdateWithCustomQuery(queryString, values, types);
        }

        [Transaction(TransactionMode.Requires)]
        public void Delete(object instance)
        {
            dao.Delete(instance);
        }

        [Transaction(TransactionMode.Requires)]
        public virtual void DeleteById<T>(object id)
        {
            object instance = this.FindById<T>(id);
            dao.Delete(instance);
        }

        [Transaction(TransactionMode.Requires)]
        public void Delete(IList instances)
        {
            if (instances != null && instances.Count > 0)
            {
                foreach (object inst in instances)
                {
                    dao.Delete(inst);
                }
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void Delete<T>(IList<T> instances)
        {
            if (instances != null && instances.Count > 0)
            {
                foreach (object inst in instances)
                {
                    dao.Delete(inst);
                }
            }
        }

        [Transaction(TransactionMode.Requires)]
        public void DeleteAll(Type type)
        {
            dao.DeleteAll(type);
        }

        public void FlushSession()
        {
            this.dao.FlushSession();
        }

        public void CleanSession()
        {
            dao.CleanSession();
        }

        [Transaction(TransactionMode.Requires)]
        public void Delete(string hqlString)
        {
            dao.Delete(hqlString);
        }

        //public void Delete(string hqlString, object value)
        //{
        //    dao.Delete(hqlString, value);
        //}

        [Transaction(TransactionMode.Requires)]
        public void Delete(string hqlString, object value, IType type)
        {
            dao.Delete(hqlString, value, type);
        }

        //public void Delete(string hqlString, object[] values)
        //{
        //    dao.Delete(hqlString, values);
        //}

        [Transaction(TransactionMode.Requires)]
        public void Delete(string hqlString, object[] values, IType[] types)
        {
            dao.Delete(hqlString, values, types);
        }

        public T FindById<T>(object id)
        {
            return dao.FindById<T>(id);
        }

        public IList<T> FindAll<T>()
        {
            return dao.FindAll<T>();
        }

        public IList<T> FindAll<T>(int firstRow, int maxRows)
        {
            return dao.FindAll<T>(firstRow, maxRows);
        }

        public IList FindAll(string hql)
        {
            return dao.FindAllWithCustomQuery(hql);
        }

        public IList FindAll(string hql, object value)
        {
            return dao.FindAllWithCustomQuery(hql, value);
        }

        public IList FindAll(string hql, object value, IType type)
        {
            return dao.FindAllWithCustomQuery(hql, value, type);
        }

        public IList FindAll(string hql, object[] values)
        {
            return dao.FindAllWithCustomQuery(hql, values);
        }

        public IList FindAll(string hql, object[] values, IType[] types)
        {
            return dao.FindAllWithCustomQuery(hql, values, types);
        }

        public IList FindAll(string hql, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery(hql, firstRow, maxRows);
        }

        public IList FindAll(string hql, object value, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery(hql, value, firstRow, maxRows);
        }

        public IList FindAll(string hql, object value, IType type, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery(hql, value, type, firstRow, maxRows);
        }

        public IList FindAll(string hql, object[] values, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery(hql, values, firstRow, maxRows);
        }

        public IList FindAll(string hql, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery(hql, values, types, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql)
        {
            return dao.FindAllWithCustomQuery<T>(hql);
        }

        public IList<T> FindAll<T>(string hql, object value)
        {
            return dao.FindAllWithCustomQuery<T>(hql, value);
        }

        public IList<T> FindAll<T>(string hql, object value, IType type)
        {
            return dao.FindAllWithCustomQuery<T>(hql, value, type);
        }

        public IList<T> FindAll<T>(string hql, object[] values)
        {
            return dao.FindAllWithCustomQuery<T>(hql, values);
        }

        public IList<T> FindAll<T>(string hql, object[] values, IType[] types)
        {
            return dao.FindAllWithCustomQuery<T>(hql, values, types);
        }

        public IList<T> FindAll<T>(string hql, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery<T>(hql, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object value, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery<T>(hql, value, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object value, IType type, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery<T>(hql, value, type, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object[] values, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery<T>(hql, values, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery<T>(hql, values, types, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, IDictionary<string, object> param)
        {
            return dao.FindAllWithCustomQuery<T>(hql, param);
        }

        public IList<T> FindAll<T>(string hql, IDictionary<string, object> param, IType[] types)
        {
            return dao.FindAllWithCustomQuery<T>(hql, param, types);
        }

        public IList<T> FindAll<T>(string hql, IDictionary<string, object> param, IType[] types, int firstRow, int maxRows)
        {
            return dao.FindAllWithCustomQuery<T>(hql, param, types, firstRow, maxRows);
        }

        public IList FindAll(DetachedCriteria criteria)
        {
            return dao.FindAll(criteria);
        }

        public IList FindAll(DetachedCriteria criteria, int firstRow, int maxRows)
        {
            return dao.FindAll(criteria, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(DetachedCriteria criteria)
        {
            return dao.FindAll<T>(criteria);
        }

        public IList<T> FindAll<T>(DetachedCriteria criteria, int firstRow, int maxRows)
        {
            return dao.FindAll<T>(criteria, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery)
        {
            return dao.FindAllWithNamedQuery(namedQuery);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value)
        {
            return dao.FindAllWithNamedQuery(namedQuery, value);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value, IType type)
        {
            return dao.FindAllWithNamedQuery(namedQuery, value, type);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values)
        {
            return dao.FindAllWithNamedQuery(namedQuery, values);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values, IType[] types)
        {
            return dao.FindAllWithNamedQuery(namedQuery, values, types);
        }

        public IList FindAllWithNamedQuery(string namedQuery, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery(namedQuery, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery(namedQuery, value, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value, IType type, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery(namedQuery, value, type, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery(namedQuery, values, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery(namedQuery, values, types, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, value);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value, IType type)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, value, type);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, values);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values, IType[] types)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, values, types);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, value, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value, IType type, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, value, type, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, values, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return dao.FindAllWithNamedQuery<T>(namedQuery, values, types, firstRow, maxRows);
        }

        public IList FindAllWithNativeSql(string sql)
        {
            return dao.FindAllWithNativeSql(sql);
        }

        public IList FindAllWithNativeSql(string sql, object value)
        {
            return dao.FindAllWithNativeSql(sql, value);
        }

        public IList FindAllWithNativeSql(string sql, object value, IType type)
        {
            return dao.FindAllWithNativeSql(sql, value, type);
        }

        public IList FindAllWithNativeSql(string sql, object[] values)
        {
            return dao.FindAllWithNativeSql(sql, values);
        }

        public IList FindAllWithNativeSql(string sql, object[] values, IType[] types)
        {
            return dao.FindAllWithNativeSql(sql, values, types);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql)
        {
            return dao.FindAllWithNativeSql<T>(sql);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object value)
        {
            return dao.FindAllWithNativeSql<T>(sql, value);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object value, IType type)
        {
            return dao.FindAllWithNativeSql<T>(sql, value, type);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object[] values)
        {
            return dao.FindAllWithNativeSql<T>(sql, values);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object[] values, IType[] types)
        {
            return dao.FindAllWithNativeSql<T>(sql, values, types);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql)
        {
            return dao.FindEntityWithNativeSql<T>(sql);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object value)
        {
            return dao.FindEntityWithNativeSql<T>(sql, value);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object value, IType type)
        {
            return dao.FindEntityWithNativeSql<T>(sql, value, type);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object[] values)
        {
            return dao.FindEntityWithNativeSql<T>(sql, values);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object[] values, IType[] types)
        {
            return dao.FindEntityWithNativeSql<T>(sql, values, types);
        }

        public DataSet GetDatasetBySql(string commandText, SqlParameter[] commandParameters)
        {
            return sqlDao.GetDatasetBySql(commandText, commandParameters);
        }

        public int UpdateWithNativeQuery(string queryString)
        {
            return dao.ExecuteUpdateWithNativeQuery(queryString);
        }

        public int UpdateWithNativeQuery(string queryString, object value)
        {
            return dao.ExecuteUpdateWithNativeQuery(queryString, value);
        }

        public int UpdateWithNativeQuery(string queryString, object value, IType type)
        {
            return dao.ExecuteUpdateWithNativeQuery(queryString, value, type);
        }

        public int UpdateWithNativeQuery(string queryString, object[] values)
        {
            return dao.ExecuteUpdateWithNativeQuery(queryString, values);
        }

        public int UpdateWithNativeQuery(string queryString, object[] values, IType[] types)
        {
            return dao.ExecuteUpdateWithNativeQuery(queryString, values, types);
        }

        public IList<T> FindAllIn<T>(string hql, IEnumerable<object> inParam, IEnumerable<object> param = null)
        {
            if (inParam == null || inParam.Count() == 0)
            {
                return null;
            }
            List<T> tList = new List<T>();

            int inParamCount = 2000;//每次最多2000
            if (param != null)
            {
                inParamCount -= param.Count();
            }

            int skipCount = 0;
            while (true)
            {
                var hqlStr = new StringBuilder(hql);
                var paramList = new List<object>();
                if (param != null) //其他的查询参数
                {
                    paramList.AddRange(param);
                }

                var batchinParam = inParam.Skip(skipCount).Take(inParamCount).ToList();//得到in参数
                if (batchinParam.Count() == 0)
                {
                    break;
                }
                skipCount += inParamCount;

                for (int i = 0; i < batchinParam.Count(); i++)
                {
                  hqlStr.Append("?,");
                }
                hqlStr = hqlStr.Remove(hqlStr.Length - 1, 1);
                hqlStr.Append(")");
                paramList.AddRange(batchinParam);
                tList.AddRange(dao.FindAllWithCustomQuery<T>(hqlStr.ToString(), paramList.ToArray()));
            }
            return tList;
        }

        public IList<T> FindEntityWithNativeSqlIn<T>(string sql, IEnumerable<object> inValues, IEnumerable<object> values = null)
        {
            if (inValues == null || inValues.Count() == 0)
            {
                return null;
            }
            List<T> tList = new List<T>();

            int inParamCount = 2000;
            if (values != null)
            {
                inParamCount -= values.Count();
            }

            int skipCount = 0;
            while (true)
            {
                var sqlStr = new StringBuilder(sql);
                List<object> paramValue = new List<object>();
                var batchinParam = inValues.Skip(skipCount).Take(inParamCount).ToList();
                if (batchinParam.Count() == 0)
                {
                    break;
                }
                skipCount += inParamCount;

                for (int i = 0; i < batchinParam.Count(); i++)
                {
                    sqlStr.Append("?,");
                }
                sqlStr = sqlStr.Remove(sqlStr.Length - 1, 1);
                sqlStr.Append(")");

                if (values != null)
                {
                    paramValue.AddRange(values);
                }
                paramValue.AddRange(batchinParam);
                tList.AddRange(dao.FindEntityWithNativeSql<T>(sqlStr.ToString(), paramValue.ToArray()));
            }
            return tList;
        }

        public IList<T> FindAllWithNativeSqlIn<T>(string sql, IEnumerable<object> inValues, IEnumerable<object> values = null)
        {
            if (inValues == null || inValues.Count() == 0)
            {
                return null;
            }
            List<T> tList = new List<T>();

            int inParamCount = 1000;
            if (values != null)
            {
                inParamCount -= values.Count();
            }

            int skipCount = 0;

            while (true)
            {
                List<object> paramValue = new List<object>();
                var sqlStr = new StringBuilder(sql);
                var batchinParam = inValues.Skip(skipCount).Take(inParamCount);
                if (batchinParam.Count() == 0)
                {
                    break;
                }
                skipCount += inParamCount;

                for (int i = 0; i < batchinParam.Count(); i++)
                {
                    sqlStr.Append("?,");
                }
                sqlStr = sqlStr.Remove(sqlStr.Length - 1, 1);
                sqlStr.Append(")");

                if (values != null)
                {
                    paramValue.AddRange(values);
                }
                paramValue.AddRange(batchinParam);
                tList.AddRange(dao.FindAllWithNativeSql<T>(sqlStr.ToString(), paramValue.ToArray()));
            }
            return tList;
        }
    }
}
