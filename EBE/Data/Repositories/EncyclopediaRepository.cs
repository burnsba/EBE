using System;
using System.Collections.Generic;

namespace EBE.Data
{
    public class EncyclopediaRepository : IEncyclopediaRepository
    {
        public void Add(Encyclopedia e)
        {
            using(var context = new EBEContext())
            {
                context.TransactSave(e);
            }
        }

        public void Update(Encyclopedia e)
        {
            using(var context = new EBEContext())
            {
                context.TransactUpdate(e);
            }
        }

        public void Remove(Encyclopedia e)
        {
            using(var context = new EBEContext())
            {
                context.TransactDelete(e);
            }
        }

        public Encyclopedia FindById(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<Encyclopedia>(id);
            }
        }

        public bool Exists(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<Encyclopedia>(id) != null;
            }
        }

        public List<Encyclopedia> Find(string propertyName, object value)
        {
            using(var context = new EBEContext())
            {
                return context.TransactFind<Encyclopedia>(propertyName, value);
            }
        }
    }
}

