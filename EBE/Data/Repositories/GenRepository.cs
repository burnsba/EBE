using System;

namespace EBE.Data
{
    public class GenRepository : IGenRepository
    {
        public void Add(Gen g)
        {
            using(var context = new EBEContext())
            {
                context.TransactSave(g);
            }
        }

        public void Update(Gen g)
        {
            using(var context = new EBEContext())
            {
                context.TransactUpdate(g);
            }
        }

        public void Remove(Gen g)
        {
            using(var context = new EBEContext())
            {
                context.TransactDelete(g);
            }
        }

        public Gen FindById(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<Gen>(id);
            }
        }

        public bool Exists(Guid id)
        {
            using(var context = new EBEContext())
            {
                return context.TransactGet<Gen>(id) != null;
            }
        }
    }
}

