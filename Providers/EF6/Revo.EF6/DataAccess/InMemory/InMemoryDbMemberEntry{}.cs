using System;
using System.Linq.Expressions;
using System.Reflection;
using Revo.DataAccess.InMemory;
using Revo.EF6.DataAccess.Entities;

namespace Revo.EF6.DataAccess.InMemory
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
