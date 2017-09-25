using System;
using System.Data.Entity;
using GTRevo.DataAccess.EF6.Model;

namespace GTRevo.Infrastructure.EF6.Sagas.Model
{
    public class SagaModelDefinition : IModelDefinition
    {
        public void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
