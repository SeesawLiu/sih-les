using System;
using System.Collections;
using NHibernate.Type;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace com.Sconit.Service
{
    public interface IGenericMgr : IQueryMgr
    {      
        void Save(object instance);

        void Create(object instance);

        void Update(object instance);

        void MergeUpdate(object instance);

        void Delete(object instance);

        void DeleteById<T>(object id);

        void Delete(IList instances);

        void Delete<T>(IList<T> instances);

        void DeleteAll(Type type);

        void FlushSession();

        void CleanSession();

        void Delete(string hqlString);

        //void Delete(string hqlString, object value);

        void Delete(string hqlString, object value, IType type);

        //void Delete(string hqlString, object[] values);

        void Delete(string hqlString, object[] values, IType[] types);

        int Update(string queryString);

        int Update(string queryString, object value);

        int Update(string queryString, object value, IType type);

        int Update(string queryString, object[] values);

        int Update(string queryString, object[] values, IType[] types);

        int UpdateWithNativeQuery(string queryString);

        int UpdateWithNativeQuery(string queryString, object value);

        int UpdateWithNativeQuery(string queryString, object value, IType type);

        int UpdateWithNativeQuery(string queryString, object[] values);

        int UpdateWithNativeQuery(string queryString, object[] values, IType[] types);

    }
}
