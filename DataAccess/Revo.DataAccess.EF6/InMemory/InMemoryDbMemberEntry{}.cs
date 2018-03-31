using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.InMemory;

namespace Revo.DataAccess.EF6.InMemory
{
    public class InMemoryDbMemberEntry<TEntity, TProperty> : IDbMemberEntry<TEntity, TProperty>
        where TEntity : class
    {
        public InMemoryDbMemberEntry(Expression<Func<TEntity, TProperty>> navigationProperty,
            InMemoryCrudRepository.EntityEntry entityEntry)
        {
            NavigationProperty = navigationProperty;
            EntityEntry = entityEntry;

            PropertyInfo = (NavigationProperty.Body as MemberExpression)?.Member as PropertyInfo;
            if (PropertyInfo == null)
            {
                throw new ArgumentException($"Cannot retrieve property info from entity member expression: {navigationProperty}");
            }
        }

        public string Name => PropertyInfo.Name;

        public TProperty CurrentValue
        {
            get => (TProperty)PropertyInfo.GetValue(EntityEntry.Instance);

            set => PropertyInfo.SetValue(EntityEntry.Instance, value);
        }

        protected Expression<Func<TEntity, TProperty>> NavigationProperty { get; }
        protected PropertyInfo PropertyInfo { get; }
        protected InMemoryCrudRepository.EntityEntry EntityEntry { get; }
    }
}
