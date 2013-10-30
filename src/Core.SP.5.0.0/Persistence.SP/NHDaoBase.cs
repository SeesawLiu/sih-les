// -----------------------------------------------------------------------
// <copyright file="NHDaoBase.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace com.Sconit.Persistence.SP
{
    using NHibernate;
    using com.Sconit.Persistence.SP;
    using NHibernate.Cfg;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NHDaoBase
    {
        private static ISessionFactory _factory;
        private static object obj = new object();

        #region protected methods
        protected ISession GetSession()
        {
            if (_factory == null)
            {
                lock (obj)
                {
                    if (_factory == null)
                    {
                        Configuration cfg = new Configuration().Configure();
                        _factory = cfg.BuildSessionFactory();
                    }
                }
            }
            return _factory.OpenSession();
        }
        #endregion
    }
}
