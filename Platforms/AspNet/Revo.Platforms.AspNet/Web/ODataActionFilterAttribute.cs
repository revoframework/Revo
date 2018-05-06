using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Ninject;
using Revo.Core.IO.OData;
using Revo.Platforms.AspNet.IO.OData;

namespace Revo.Platforms.AspNet.Web
{
    public class ODataActionFilterAttribute : ActionFilterAttribute
    {
        private const string ModelKeyPrefix = "System.Web.OData.Model+";

        [Inject]
        public IQueryableToODataResultConverter[] ODataResultConverters { get; set; } = {};

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            var originalContent = actionExecutedContext.Response?.Content as ObjectContent;

            if (originalContent?.Value != null
                && originalContent.ObjectType.IsConstructedGenericType 
                && originalContent.ObjectType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                Type entityType = originalContent.ObjectType.GenericTypeArguments[0];
                bool includeCount = actionExecutedContext.Request.RequestUri.Query.Contains("$count=true")
                    || actionExecutedContext.Request.RequestUri.Query.Contains("%24count=true"); // TODO improve

                IQueryable queryable = (IQueryable) originalContent.Value;
                IQueryableToODataResultConverter converter =
                    ODataResultConverters.FirstOrDefault(x => x.Supports(queryable))
                    ?? DefaultConverter.Instance;

                object result;
                if (includeCount)
                {
                    var method = this.GetType().GetMethod(nameof(GetResultWithCount), BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(new[] { entityType });
                    result = await (Task<object>)method.Invoke(this, new object[] { queryable, actionExecutedContext.Request, converter });
                }
                else
                {
                    var method = this.GetType().GetMethod(nameof(GetResult), BindingFlags.Instance | BindingFlags.NonPublic)
                        .MakeGenericMethod(new[] { entityType });
                    result = await (Task<object>)method.Invoke(this, new object[] { queryable, actionExecutedContext.Request, converter });
                }
                
                actionExecutedContext.Response.Content = (HttpContent) typeof(ObjectContent<>)
                    .MakeGenericType(result.GetType())
                    .GetConstructors().First(x => x.GetParameters().Length == 3)
                    .Invoke(new object[]
                    {
                        result,
                        originalContent.Formatter,
                        originalContent.Headers.ContentType.MediaType
                    });
            }
        }

        private async Task<object> GetResult<T>(IQueryable<T> queryable, HttpRequestMessage httpRequestMessage,
            IQueryableToODataResultConverter converter)
        {
            var queryOptions = GetQueryOptions<T>(httpRequestMessage);
            return await converter.ToListAsync(queryable, queryOptions, default(CancellationToken));
        }

        private async Task<object> GetResultWithCount<T>(IQueryable<T> queryable, HttpRequestMessage httpRequestMessage,
            IQueryableToODataResultConverter converter)
        {
            var queryOptions = GetQueryOptions<T>(httpRequestMessage);
            return await converter.ToListWithCountAsync(queryable, queryOptions, default(CancellationToken));
        }

        private ODataQueryOptions<T> GetQueryOptions<T>(HttpRequestMessage httpRequestMessage)
        {
            IEdmModel userModel = httpRequestMessage.GetModel();
            IEdmModel model = userModel != EdmCoreModel.Instance ? userModel : GetEdmModel(httpRequestMessage.GetActionDescriptor(), (typeof(T)));
            ODataQueryContext entitySetContext = new ODataQueryContext(model, typeof(T), httpRequestMessage.ODataProperties().Path);

            return new ODataQueryOptions<T>(entitySetContext, httpRequestMessage);
        }

        // borrowed from WebApi/src/System.Web.OData/Extensions/HttpActionDescriptorExtensions.cs
        internal static IEdmModel GetEdmModel(HttpActionDescriptor actionDescriptor, Type entityClrType)
        {
            // save the EdmModel to the action descriptor
            return actionDescriptor.Properties.GetOrAdd(ModelKeyPrefix + entityClrType.FullName, _ =>
            {
                ODataConventionModelBuilder builder =
                    new ODataConventionModelBuilder(actionDescriptor.Configuration, isQueryCompositionMode: true);
                EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
                builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);
                IEdmModel edmModel = builder.GetEdmModel();
                Contract.Assert(edmModel != null);
                return edmModel;
            }) as IEdmModel;
        }

        public class DefaultConverter : IQueryableToODataResultConverter
        {
            public static readonly DefaultConverter Instance = new DefaultConverter();

            public bool Supports(IQueryable queryable)
            {
                return true;
            }

            public Task<ODataResult<T>> ToListAsync<T>(IQueryable<T> queryable,
                ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
            {
                return Task.FromResult(
                    new ODataResult<T>(queryable
                        .ApplyOptions(queryOptions)
                        .ToList()));
            }

            public Task<ODataResultWithCount<T>> ToListWithCountAsync<T>(IQueryable<T> queryable,
                ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken)
            {
                return Task.FromResult(
                    new ODataResultWithCount<T>(
                        queryable
                            .ApplyOptions(queryOptions)
                            .ToList(),
                        queryable.LongCount()));
            }
        }
    }
}
