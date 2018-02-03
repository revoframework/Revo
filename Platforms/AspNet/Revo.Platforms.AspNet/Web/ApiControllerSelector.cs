using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace Revo.Platforms.AspNet.Web
{
    /// <summary>
    /// Courtesy of http://stackoverflow.com/a/34654994/117563
    /// </summary>
    public class ApiControllerSelector : IHttpControllerSelector
    {
        /*public override string GetControllerName(HttpRequestMessage request)
        {
            string name = base.GetControllerName(request).Replace("-", string.Empty);

            if (!this.GetControllerMapping().ContainsKey(name) && name.ToLower().EndsWith("controller"))
            {
                name = name.Substring(0, name.Length - "controller".Length);
            }

            return name;
        }*/

        public static readonly string ControllerSuffix = "Controller";
        public static readonly string ServiceSuffix = "Service";

        private const string ControllerKey = "controller";

        private readonly HttpConfiguration _configuration;
        private readonly HttpControllerTypeCache _controllerTypeCache;
        private readonly Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>> _controllerInfoCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpControllerSelector"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public ApiControllerSelector(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            _controllerInfoCache = new Lazy<ConcurrentDictionary<string, HttpControllerDescriptor>>(InitializeControllerInfoCache);
            _configuration = configuration;
            _controllerTypeCache = new HttpControllerTypeCache(_configuration);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public virtual HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IHttpRouteData routeData = request.GetRouteData();
            HttpControllerDescriptor controllerDescriptor;
            if (routeData != null)
            {
                controllerDescriptor = GetDirectRouteController(routeData);
                if (controllerDescriptor != null)
                {
                    return controllerDescriptor;
                }
            }

            string controllerName = GetControllerName(request);
            if (String.IsNullOrEmpty(controllerName))
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    //Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    string.Format("SRResources.ControllerNameNotFound", request.RequestUri)));
            }

            if (_controllerInfoCache.Value.TryGetValue(controllerName, out controllerDescriptor))
            {
                return controllerDescriptor;
            }

            ICollection<Type> matchingTypes = _controllerTypeCache.GetControllerTypes(controllerName);

            // ControllerInfoCache is already initialized.
            Contract.Assert(matchingTypes.Count != 1);

            if (matchingTypes.Count == 0)
            {
                // no matching types
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    //Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                    string.Format("SRResources.DefaultControllerFactory_ControllerNameNotFound", controllerName)));
            }
            else
            {
                // multiple matching types
                throw CreateAmbiguousControllerException(request.GetRouteData().Route, controllerName, matchingTypes);
            }
        }

        public virtual IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerInfoCache.Value.ToDictionary(c => c.Key, c => c.Value, StringComparer.OrdinalIgnoreCase);
        }

        public virtual string GetControllerName(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            IHttpRouteData routeData = request.GetRouteData();
            if (routeData == null)
            {
                return null;
            }

            // Look up controller in route data
            string controllerName = null;
            routeData.Values.TryGetValue(ControllerKey, out controllerName);
            return controllerName.Replace("-", string.Empty); //NOTE: ASP edit!
        }

        // If routeData is from an attribute route, get the controller that can handle it. 
        // Else return null. Throws an exception if multiple controllers match
        private static HttpControllerDescriptor GetDirectRouteController(IHttpRouteData routeData)
        {
            CandidateAction[] candidates = routeData.GetDirectRouteCandidates();
            if (candidates != null)
            {
                // Set the controller descriptor for the first action descriptor
                Contract.Assert(candidates.Length > 0);
                Contract.Assert(candidates[0].ActionDescriptor != null);

                HttpControllerDescriptor controllerDescriptor = candidates[0].ActionDescriptor.ControllerDescriptor;

                // Check that all other candidate action descriptors share the same controller descriptor
                for (int i = 1; i < candidates.Length; i++)
                {
                    CandidateAction candidate = candidates[i];
                    if (candidate.ActionDescriptor.ControllerDescriptor != controllerDescriptor)
                    {
                        // We've found an ambiguity (multiple controllers matched)
                        throw CreateDirectRouteAmbiguousControllerException(candidates);
                    }
                }

                return controllerDescriptor;
            }

            return null;
        }

        private static Exception CreateDirectRouteAmbiguousControllerException(CandidateAction[] candidates)
        {
            Contract.Assert(candidates != null);
            Contract.Assert(candidates.Length > 1);

            HashSet<Type> matchingTypes = new HashSet<Type>();
            for (int i = 0; i < candidates.Length; i++)
            {
                matchingTypes.Add(candidates[i].ActionDescriptor.ControllerDescriptor.ControllerType);
            }

            // we need to generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            return new InvalidOperationException(string.Format("SRResources.DirectRoute_AmbiguousController", typeList, Environment.NewLine));
        }

        private static Exception CreateAmbiguousControllerException(IHttpRoute route, string controllerName, ICollection<Type> matchingTypes)
        {
            Contract.Assert(route != null);
            Contract.Assert(controllerName != null);
            Contract.Assert(matchingTypes != null);

            // Generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes)
            {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            string errorMessage = string.Format("SRResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate", controllerName, route.RouteTemplate, typeList, Environment.NewLine);
            return new InvalidOperationException(errorMessage);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> InitializeControllerInfoCache()
        {
            var result = new ConcurrentDictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);
            var duplicateControllers = new HashSet<string>();
            Dictionary<string, ILookup<string, Type>> controllerTypeGroups = _controllerTypeCache.Cache;

            foreach (KeyValuePair<string, ILookup<string, Type>> controllerTypeGroup in controllerTypeGroups)
            {
                string controllerName = controllerTypeGroup.Key;

                foreach (IGrouping<string, Type> controllerTypesGroupedByNs in controllerTypeGroup.Value)
                {
                    foreach (Type controllerType in controllerTypesGroupedByNs)
                    {
                        if (result.Keys.Contains(controllerName))
                        {
                            duplicateControllers.Add(controllerName);
                            break;
                        }
                        else
                        {
                            result.TryAdd(controllerName, new HttpControllerDescriptor(_configuration, controllerName, controllerType));
                        }
                    }
                }
            }

            foreach (string duplicateController in duplicateControllers)
            {
                HttpControllerDescriptor descriptor;
                result.TryRemove(duplicateController, out descriptor);
            }

            return result;
        }
    }
}
