using System;

namespace com.Sconit.Persistence.SP
{
    /// <summary>
    /// Summary description for IDaoBase.
    /// </summary>

    public interface IDao : IQueryDao
    {
        object Create(object instance);

        void Update(object instance);

        void Delete(object instance);

        void DeleteAll(Type type);

        void Save(object instance);
    }
}
