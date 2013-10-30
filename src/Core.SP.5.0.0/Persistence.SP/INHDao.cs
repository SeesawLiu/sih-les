using NHibernate.Type;

namespace com.Sconit.Persistence.SP
{
    /// <summary>
    /// Summary description for INHDaoBase.
    /// </summary>

    public interface INHDao : IDao, INHQueryDao
    {

        void MergeUpdate(object instance);

        void Delete(string hqlString);

        //void Delete(string hqlString, object value);

        void Delete(string hqlString, object value, IType type);

        //void Delete(string hqlString, object[] values);

        void Delete(string hqlString, object[] values, IType[] type);

        int ExecuteUpdateWithCustomQuery(string queryString);

        int ExecuteUpdateWithCustomQuery(string queryString, object value);

        int ExecuteUpdateWithCustomQuery(string queryString, object value, IType type);

        int ExecuteUpdateWithCustomQuery(string queryString, object[] values);

        int ExecuteUpdateWithCustomQuery(string queryString, object[] values, IType[] types);

        void InitializeLazyProperties(object instance);

        void InitializeLazyProperty(object instance, string propertyName);

        void FlushSession();

        void CleanSession();        
    }
}
