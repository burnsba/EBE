using System;
using System.Collections.Generic;

namespace EBE.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <inheritdoc/>
        public void Add(T item)
        {
            using(var context = new EBEContext())
            {
                context.TransactSave(item);
            }
        }

        /// <inheritdoc/>
        public void Update(T item)
        {
            using(var context = new EBEContext())
            {
                context.TransactUpdate(item);
            }
        }

        /// <inheritdoc/>
        public void Remove(T item)
        {
            using(var context = new EBEContext())
            {
                context.TransactDelete(item);
            }
        }

        /// <inheritdoc/>
        public T FindById(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<T>(id);
            }
        }

        /// <inheritdoc/>
        public bool Exists(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<T>(id) != null;
            }
        }

        /// <inheritdoc/>
        public List<T> Find(string propertyName, object value)
        {
            using(var context = new EBEContext())
            {
                return context.TransactFind<T>(propertyName, value);
            }
        }

        /// <inheritdoc/>
        public List<T> Find(string propertyName, object value, string propertyName2, object value2)
        {
            using(var context = new EBEContext())
            {
                return context.TransactFind2<T>(propertyName, value, propertyName2, value2);
            }
        }
    }
}

