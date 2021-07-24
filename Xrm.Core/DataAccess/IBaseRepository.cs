using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace Predica.Xrm.Core.DataAccess
{
    public interface IBaseRepository<T> 
        where T : Entity, new()
    {
        Guid Create(T entity);

        void Update(T entity);

        void Delete(T entity);

        T Retrieve(Guid id, params string[] columns);

        T Retrieve(string entityLogicalName, Guid id, params string[] columns);
    }
}