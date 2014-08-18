using System;
using System.Collections.Generic;

namespace EBE.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public void Add(T item)
        {
            using(var context = new EBEContext())
            {
                context.TransactSave(item);
            }
        }

        public void Update(T item)
        {
            using(var context = new EBEContext())
            {
                context.TransactUpdate(item);
            }
        }

        public void Remove(T item)
        {
            using(var context = new EBEContext())
            {
                context.TransactDelete(item);
            }
        }

        public T FindById(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<T>(id);
            }
        }

        public bool Exists(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<T>(id) != null;
            }
        }

        public List<T> Find(string propertyName, object value)
        {
            using(var context = new EBEContext())
            {
                return context.TransactFind<T>(propertyName, value);
            }
        }

        public List<T> Find(string propertyName, object value, string propertyName2, object value2)
        {
            using(var context = new EBEContext())
            {
                return context.TransactFind2<T>(propertyName, value, propertyName2, value2);
            }
        }
    }
}

