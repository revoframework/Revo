using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace GTRevo.Platform.Web
{
    public class HttpControllerTypeCache
    {
        private readonly HttpConfiguration _configuration;
        private readonly Lazy<Dictionary<string, ILookup<string, Type>>> _cache;

        public HttpControllerTypeCache(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _configuration = configuration;
            _cache = new Lazy<Dictionary<string, ILookup<string, Type>>>(InitializeCache);
        }

        internal Dictionary<string, ILookup<string, Type>> Cache
        {
            get { return _cache.Value; }
        }

        public ICollection<Type> GetControllerTypes(string controllerName)
        {
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentException("controllerName");
            }

            HashSet<Type> matchingTypes = new HashSet<Type>();

            ILookup<string, Type> namespaceLookup;
            if (_cache.Value.TryGetValue(controllerName, out namespaceLookup))
            {
                foreach (var namespaceGroup in namespaceLookup)
                {
                    matchingTypes.UnionWith(namespaceGroup);
                }
            }

            return matchingTypes;
        }

        private string GetControllerNameWithoutSuffix(Type type)
        {
            if (type.Name.EndsWith(ApiControllerSelector.ControllerSuffix))
            {
                return type.Name.Substring(0, type.Name.Length - ApiControllerSelector.ControllerSuffix.Length);
            }
            /*else if (type.Name.EndsWith(ApiControllerSelector.ServiceSuffix))
            {
                return type.Name.Substring(0, type.Name.Length - ApiControllerSelector.ServiceSuffix.Length);
            }*/
            else
            {
                return type.Name;
            }
        }

        private Dictionary<string, ILookup<string, Type>> InitializeCache()
        {
            IAssembliesResolver assembliesResolver = _configuration.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllersResolver = _configuration.Services.GetHttpControllerTypeResolver();

            ICollection<Type> controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);
            var groupedByName = controllerTypes.GroupBy(
                t => GetControllerNameWithoutSuffix(t),
                StringComparer.OrdinalIgnoreCase);

            return groupedByName.ToDictionary(
                g => g.Key,
                g => g.ToLookup(t => t.Namespace ?? String.Empty, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
