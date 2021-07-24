using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;


namespace Predica.Xrm.Core.DataAccess
{
    public class BaseRepository<T> :IBaseRepository<T>
        where T : Entity, new()
    {
        protected IOrganizationService service;

        public BaseRepository(IOrganizationService service)
        {
            this.service = service;
        }

        public Guid Create(T entity)
        {
            return this.service.Create(entity);
        }

        public void Update(T entity)
        {
            this.service.Update(entity);
        }

        public void Delete(T entity)
        {
            this.service.Delete(entity.LogicalName, entity.Id);
        }

        public T Retrieve(Guid id, params string[] columns)
        {
            var temp = new T();
            return Retrieve(temp.LogicalName, id, columns);
        }

        public T Retrieve(string entityLogicalName, Guid id, params string[] columns)
        {
            return this.service.Retrieve(
                entityLogicalName,
                id,
                columns != null && columns.Length > 0 ? new ColumnSet(columns) : new ColumnSet(true))
                .ToEntity<T>();
        }       
    }
}