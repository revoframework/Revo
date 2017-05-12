using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6
{
    public class PrivateCollectionMapping
    {
        public void MapPrivateCollections(DbModelBuilder modelBuilder)
        {
            modelBuilder.Types().Configure(x => MapPrivateCollections(modelBuilder, x.ClrType));
        }

        private void MapPrivateCollections(DbModelBuilder modelBuilder,
            Type entityType)
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
                    var entityConf = entityMethod.MakeGenericMethod(entityType)
                        .Invoke(modelBuilder, new object[] {});

                    var childType = collectionProp.PropertyType.GenericTypeArguments[0];
                    
                    ParameterExpression xParameterExpression = Expression.Parameter(entityType, "x");
                    Expression collectionPropertyExpression = Expression.Property(xParameterExpression, collectionProp);
                    Type collectionType = typeof(ICollection<>).MakeGenericType(childType);
                    Expression icollectionCastExpression = Expression.Convert(collectionPropertyExpression, collectionType);
                    Expression memberLambda = Expression.Lambda(icollectionCastExpression, xParameterExpression);

                    var hasMany = entityConf.GetType().GetMethod("HasMany")
                        .MakeGenericMethod(childType)
                        .Invoke(entityConf, new object[] { memberLambda });
                }
            }
        }
    }
}
