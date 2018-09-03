using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Revo.DataAccess.Entities;

namespace Revo.EF6.DataAccess.Model
{
    public class PrivateCollectionMapping
    {
        public void MapPrivateCollections(DbModelBuilder modelBuilder)
        {
            modelBuilder.Types().Configure(x => MapPrivateCollections(modelBuilder, x.ClrType, x.ClrType));
        }

        private void MapPrivateCollections(DbModelBuilder modelBuilder,
            Type entityType, Type concreteType)
        {
            var entityMethod = modelBuilder.GetType().GetMethod("Entity");

            if (entityType.IsClass)
            {
                var collectionProps = entityType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(x => x.PropertyType.IsGenericType
                                && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                                && x.PropertyType.GenericTypeArguments[0].IsClass
                                &&
                                x.PropertyType.GenericTypeArguments[0].GetCustomAttributes(
                                    typeof(DatabaseEntityAttribute), true).Length > 0);

                foreach (var collectionProp in collectionProps)
                {
                    var entityConf = entityMethod.MakeGenericMethod(concreteType)
                        .Invoke(modelBuilder, new object[] {});
                    var entityTypeConf = entityConf.GetType().GetField("_entityTypeConfiguration",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entityConf);
                    IDictionary navigationPropertyConfigurations = (IDictionary) entityTypeConf.GetType().GetField(
                        "_navigationPropertyConfigurations",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entityTypeConf);

                    if (navigationPropertyConfigurations.Contains(collectionProp))
                    {
                        continue;
                    }

                    var childType = collectionProp.PropertyType.GenericTypeArguments[0];
                    
                    ParameterExpression xParameterExpression = Expression.Parameter(concreteType, "x");
                    Expression collectionPropertyExpression = Expression.Property(xParameterExpression, collectionProp);
                    Type collectionType = typeof(ICollection<>).MakeGenericType(childType);
                    Expression icollectionCastExpression = Expression.Convert(collectionPropertyExpression, collectionType);
                    Expression memberLambda = Expression.Lambda(icollectionCastExpression, xParameterExpression);

                    var hasMany = entityConf.GetType().GetMethod("HasMany")
                        .MakeGenericMethod(childType)
                        .Invoke(entityConf, new object[] { memberLambda });
                }
                
                if (entityType.BaseType != null &&
                    (entityType.BaseType.IsConstructedGenericType))
                {
                    //MapPrivateCollections(modelBuilder, entityType.BaseType, concreteType);
                }
            }
        }
    }
}
