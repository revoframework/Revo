using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Newtonsoft.Json;

namespace Revo.Platforms.AspNet.Web
{
    public class ODataActionFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            var originalContent = actionExecutedContext.Response?.Content as ObjectContent;

            if (originalContent != null && originalContent.ObjectType.IsConstructedGenericType
                && originalContent.ObjectType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                Type entityType = originalContent.ObjectType.GenericTypeArguments[0];

                bool includeCount = actionExecutedContext.Request.RequestUri.Query.Contains("%24count=true"); // TODO improve
                Type wrapperType;
                IODataQueryableWrapper wrappedObject;

                if (includeCount)
                {
                    wrapperType = typeof(ODataQueryableWrapperWithCount<>).MakeGenericType(entityType);
                }
                else
                {
                    wrapperType = typeof(ODataQueryableWrapper<>).MakeGenericType(entityType);
                }
                
                wrappedObject = (IODataQueryableWrapper) wrapperType.GetConstructors()[0].Invoke(new object[] { });
                wrappedObject.Value = originalContent.Value;

                if (includeCount)
                {
                    /*dynamic enumerable = wrappedObject.Value.GetType().GetProperty("Enumerable", 
                        BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(wrappedObject.Value); //is TruncatedCollection<>

                    long count = enumerable.IsTruncated
                        ? (enumerable.TotalCount ?? enumerable.Count)
                        : enumerable.Count;*/

                    //weirdly enough, ODataCountMediaTypeMapping.IsCountRequest is always returns false, so using this hack instead
                    var odataProps = System.Web.OData.Extensions.HttpRequestMessageExtensions.ODataProperties(actionExecutedContext.Request);
                    long count = odataProps.TotalCount ?? 0; // TODO 
                    ((IODataQueryableWrapperWithCount) wrappedObject).Count = count;
                }

                actionExecutedContext.Response.Content = (HttpContent) typeof(ObjectContent<>)
                    .MakeGenericType(wrapperType)
                    .GetConstructors().First(x => x.GetParameters().Length == 3)
                    .Invoke(new object[]
                    {
                        wrappedObject,
                        originalContent.Formatter,
                        originalContent.Headers.ContentType.MediaType
                    });
            }

        }

        protected interface IODataQueryableWrapper
        {
            object Value { get; set; }
        }

        protected interface IODataQueryableWrapperWithCount
        {
            long Count { get; set; }
        }

        protected class ODataQueryableWrapper<T> : IODataQueryableWrapper
        {
            [JsonProperty("value")]
            public IQueryable<T> Value { get; set; }

            object IODataQueryableWrapper.Value
            {
                get { return Value; }
                set { Value = (IQueryable<T>) value; }
            }
        }

        protected class ODataQueryableWrapperWithCount<T> : ODataQueryableWrapper<T>, IODataQueryableWrapperWithCount
        {
            [JsonProperty("@odata.count")]
            public long Count { get; set; }
        }
    }
}
