using System;
using System.Collections.Generic;

namespace EBE.Data
{
    public interface IEncyclopediaRepository
    {
        void Add(Encyclopedia e);
        void Update(Encyclopedia e);
        void Remove(Encyclopedia e);

        Encyclopedia FindById(Guid id);
        bool Exists(Guid id);

        //List<Encyclopedia> Find(string propertyName, object value);
    }
}

