// -----------------------------------------------------------------------
// <copyright file="QueryMgrImpl.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace com.Sconit.Service.Impl
{
    using System.Collections;
    using System.Collections.Generic;
    using com.Sconit.Persistence;
    using NHibernate.Criterion;
    using NHibernate.Type;
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QueryMgrImpl : BaseMgr, IQueryMgr
    {
        public INHQueryDao queryDao { get; set; }
        public ISqlDao sqlDao { get; set; }

        public T FindById<T>(object id)
        {
            return queryDao.FindById<T>(id);
        }

        public IList<T> FindAll<T>()
        {
            return queryDao.FindAll<T>();
        }

        public IList<T> FindAll<T>(int firstRow, int maxRows)
        {
            return queryDao.FindAll<T>(firstRow, maxRows);
        }

        public IList FindAll(string hql)
        {
            return queryDao.FindAllWithCustomQuery(hql);
        }

        public IList FindAll(string hql, object value)
        {
            return queryDao.FindAllWithCustomQuery(hql, value);
        }

        public IList FindAll(string hql, object value, IType type)
        {
            return queryDao.FindAllWithCustomQuery(hql, value, type);
        }

        public IList FindAll(string hql, object[] values)
        {
            return queryDao.FindAllWithCustomQuery(hql, values);
        }

        public IList FindAll(string hql, object[] values, IType[] types)
        {
            return queryDao.FindAllWithCustomQuery(hql, values, types);
        }

        public IList FindAll(string hql, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery(hql, firstRow, maxRows);
        }

        public IList FindAll(string hql, object value, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery(hql, value, firstRow, maxRows);
        }

        public IList FindAll(string hql, object value, IType type, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery(hql, value, type, firstRow, maxRows);
        }

        public IList FindAll(string hql, object[] values, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery(hql, values, firstRow, maxRows);
        }

        public IList FindAll(string hql, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery(hql, values, types, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql);
        }

        public IList<T> FindAll<T>(string hql, object value)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, value);
        }

        public IList<T> FindAll<T>(string hql, object value, IType type)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, value, type);
        }

        public IList<T> FindAll<T>(string hql, object[] values)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, values);
        }

        public IList<T> FindAll<T>(string hql, object[] values, IType[] types)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, values, types);
        }

        public IList<T> FindAll<T>(string hql, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object value, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, value, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object value, IType type, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, value, type, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object[] values, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, values, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, values, types, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(string hql, IDictionary<string, object> param)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, param);
        }

        public IList<T> FindAll<T>(string hql, IDictionary<string, object> param, IType[] types)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, param, types);
        }

        public IList<T> FindAll<T>(string hql, IDictionary<string, object> param, IType[] types, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithCustomQuery<T>(hql, param, types, firstRow, maxRows);
        }

        public IList FindAll(DetachedCriteria criteria)
        {
            return queryDao.FindAll(criteria);
        }

        public IList FindAll(DetachedCriteria criteria, int firstRow, int maxRows)
        {
            return queryDao.FindAll(criteria, firstRow, maxRows);
        }

        public IList<T> FindAll<T>(DetachedCriteria criteria)
        {
            return queryDao.FindAll<T>(criteria);
        }

        public IList<T> FindAll<T>(DetachedCriteria criteria, int firstRow, int maxRows)
        {
            return queryDao.FindAll<T>(criteria, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, value);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value, IType type)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, value, type);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, values);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values, IType[] types)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, values, types);
        }

        public IList FindAllWithNamedQuery(string namedQuery, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, value, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object value, IType type, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, value, type, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, values, firstRow, maxRows);
        }

        public IList FindAllWithNamedQuery(string namedQuery, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery(namedQuery, values, types, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, value);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value, IType type)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, value, type);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, values);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values, IType[] types)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, values, types);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, value, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object value, IType type, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, value, type, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, values, firstRow, maxRows);
        }

        public IList<T> FindAllWithNamedQuery<T>(string namedQuery, object[] values, IType[] types, int firstRow, int maxRows)
        {
            return queryDao.FindAllWithNamedQuery<T>(namedQuery, values, types, firstRow, maxRows);
        }

        public IList FindAllWithNativeSql(string sql)
        {
            return queryDao.FindAllWithNativeSql(sql);
        }

        public IList FindAllWithNativeSql(string sql, object value)
        {
            return queryDao.FindAllWithNativeSql(sql, value);
        }

        public IList FindAllWithNativeSql(string sql, object value, IType type)
        {
            return queryDao.FindAllWithNativeSql(sql, value, type);
        }

        public IList FindAllWithNativeSql(string sql, object[] values)
        {
            return queryDao.FindAllWithNativeSql(sql, values);
        }

        public IList FindAllWithNativeSql(string sql, object[] values, IType[] types)
        {
            return queryDao.FindAllWithNativeSql(sql, values, types);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql)
        {
            return queryDao.FindAllWithNativeSql<T>(sql);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object value)
        {
            return queryDao.FindAllWithNativeSql<T>(sql, value);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object value, IType type)
        {
            return queryDao.FindAllWithNativeSql<T>(sql, value, type);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object[] values)
        {
            return queryDao.FindAllWithNativeSql<T>(sql, values);
        }

        public IList<T> FindAllWithNativeSql<T>(string sql, object[] values, IType[] types)
        {
            return queryDao.FindAllWithNativeSql<T>(sql, values, types);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql)
        {
            return queryDao.FindEntityWithNativeSql<T>(sql);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object value)
        {
            return queryDao.FindEntityWithNativeSql<T>(sql, value);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object value, IType type)
        {
            return queryDao.FindEntityWithNativeSql<T>(sql, value, type);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object[] values)
        {
            return queryDao.FindEntityWithNativeSql<T>(sql, values);
        }

        public IList<T> FindEntityWithNativeSql<T>(string sql, object[] values, IType[] types)
        {
            return queryDao.FindEntityWithNativeSql<T>(sql, values, types);
        }

        public DataSet GetDatasetBySql(string commandText, SqlParameter[] commandParameters)
        {
            return sqlDao.GetDatasetBySql(commandText, commandParameters);
        }
    }
}
