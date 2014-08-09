using System;
using System.Collections.Generic;

namespace EBE.Data
{
    public interface IGenRepository
    {
        void Add(Gen g);
        void Update(Gen g);
        void Remove(Gen g);

        Gen FindById(Guid id);
        bool Exists(Guid id);

        //List<Gen> Find(string propertyName, object value);
    }
}

