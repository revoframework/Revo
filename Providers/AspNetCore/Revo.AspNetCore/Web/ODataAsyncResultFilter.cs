using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Ninject;
using Revo.AspNetCore.IO.OData;
using Revo.Core.IO.OData;

namespace Revo.AspNetCore.Web
{
    public class ODataAsyncResultFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var originalContent = context.Result as ObjectResult;

            if (originalContent?.Value != null
                && originalContent.DeclaredType.IsConstructedGenericType 
                && originalContent.DeclaredType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                Type entityType = originalContent.DeclaredType.GenericTypeArguments[0];
                ODataQueryOptions queryOptions = GetQueryOptions(context, entityType);

                bool includeCount = queryOptions.Count?.Value ?? false;

                var odataResultConverters =
                    ((IKernel) context.HttpContext.RequestServices.GetRequiredService(typeof(IKernel)))
                    .GetAll<IQueryableToODataResultConverter>();

                IQueryable queryable = (IQueryable) originalContent.Value;
                IQueryableToODataResultConverter converter =
                    odataResultConverters.FirstOrDefault(x => x.Supports(queryable))
                    ?? DefaultConverter.Instance;

                object result;
                if (includeCount)
                {
                    var method = GetType().GetMethod(nameof(GetResultWithCount), BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(new[] { entityType });
                    result = await (Task<object>)method.Invoke(this, new object[] { queryable, queryOptions, converter });
                }
                else
                {
                    var method = GetType().GetMethod(nameof(GetResult), BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(new[] { entityType });
                    result = await (Task<object>)method.Invoke(this, new object[] { queryable, queryOptions, converter });
                }

                context.Result = new ObjectResult(result);
            }
            
            await next();
        }
        
        private async Task<object> GetResult<T>(IQueryable<T> queryable, ODataQueryOptions<T> queryOptions,
            IQueryableToODataResultConverter converter)
        {
            return await converter.ToListAsync(queryable, queryOptions, default(CancellationToken));
        }

        private async Task<object> GetResultWithCount<T>(IQueryable<T> queryable, ODataQueryOptions<T> queryOptions,
            IQueryableToODataResultConverter converter)
        {
            return await converter.ToListWithCountAsync(queryable, queryOptions, default(CancellationToken));
        }

        private ODataQueryOptions GetQueryOptions(ResultExecutingContext context, Type entityType)
        {
            var method = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(x => x.Name == nameof(GetQueryOptions) && x.IsGenericMethodDefinition)
                .MakeGenericMethod(entityType);
            return (ODataQueryOptions) method.Invoke(this, new object[] { context });
        }

        private ODataQueryOptions<T> GetQueryOptions<T>(ResultExecutingContext context)
        {
            IEdmModel userModel = context.HttpContext.Request.GetModel();
            IEdmModel model = userModel != EdmCoreModel.Instance
                ? userModel
                : GetEdmModel(context.ActionDescriptor, context.HttpContext.Request, (typeof(T)));
            ODataQueryContext entitySetContext = new ODataQueryContext(model, typeof(T), context.HttpContext.Request.ODataFeature().Path);

            return new ODataQueryOptions<T>(entitySetContext, context.HttpContext.Request);
        }
        
        private static IEdmModel GetEdmModel(ActionDescriptor actionDescriptor, HttpRequest request, Type entityClrType)
        {
            Type actionDescriptorExtensions = typeof(EnableQueryAttribute).Assembly.GetTypes().First(x =>
                x.FullName == "Microsoft.AspNet.OData.Extensions.ActionDescriptorExtensions");
            var method = actionDescriptorExtensions.GetMethod("GetEdmModel", BindingFlags.NonPublic | BindingFlags.Static);
            return (IEdmModel) method.Invoke(null, new object[] { actionDescriptor, request, entityClrType });
        }

        public class DefaultConverter : IQueryableToODataResultConverter
        {
            public static readonly DefaultConverter Instance = new DefaultConverter();

            public bool Supports(IQueryable queryable)
            {
                return true;
            }

            public async Task<ODataResult<T>> ToListAsync<T>(IQueryable<T> queryable,
                ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
            {
                var filtered = queryable.ApplyOptions(queryOptions);
                var list = /*filtered is IAsyncEnumerable<T> asyncEnumerable
                    ? await asyncEnumerable.WithCancellation(cancellationToken).ToList()
                    :*/ filtered.ToList();
                
                return new ODataResult<T>(list);
            }

            public async Task<ODataResultWithCount<T>> ToListWithCountAsync<T>(IQueryable<T> queryable,
                ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
            {
                var filtered = queryable.ApplyOptions(queryOptions);
                var list = /*filtered is IAsyncEnumerable<T> asyncFiltered
                    ? await asyncFiltered.WithCancellation(cancellationToken).ToList()
                    :*/ filtered.ToList();

                var filteredUnranged = ((IQueryable<T>) queryOptions
                    .ApplyTo(queryable, AllowedQueryOptions.Skip | AllowedQueryOptions.Top));
                long count = /*filteredUnranged is IAsyncEnumerable<T> asyncCounted
                    ? await asyncCounted.WithCancellation(cancellationToken).LongCount()
                    :*/ filteredUnranged.LongCount();

                return new ODataResultWithCount<T>(list, count);
            }
        }
    }
}
