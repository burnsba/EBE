using System;
using System.Collections.Generic;

namespace EBE.Data
{
    public interface IRepository<T> where T : class
        {
            void Add(T item);
            void Update(T item);
            void Remove(T item);

            T FindById(Guid id);
            bool Exists(Guid id);

            List<T> Find(string propertyName, object value);
            List<T> Find(string propertyName, object value, string propertyName2, object value2);
        }
}
