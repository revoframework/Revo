using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.OData.Query;
using MoreLinq;
using WebGrease.Css.Extensions;

namespace GTRevo.Platform.Web.JSBridge
{
    public class JSServiceWrapperGenerator
    {
        private readonly IApiExplorer apiExplorer;

        public JSServiceWrapperGenerator(IApiExplorer apiExplorer)
        {
            this.apiExplorer = apiExplorer;
        }

        public string Generate()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(
@"(function () {
  'use strict';
  var module = angular.module('bc.bridge.wrapperServices', []);

");

            var controllers = apiExplorer.ApiDescriptions
                .DistinctBy(x => x.ActionDescriptor.ControllerDescriptor.ControllerName + "." + x.ActionDescriptor.ActionName) //if more than 1 HTTP method allowed, use just one
                .GroupBy(x => x.ActionDescriptor.ControllerDescriptor)
                .ToList();

            foreach (var controller in controllers)
            {
                string serviceName = GetServiceName(controller.Key);
                sb.AppendLine(
$@"  module.service('{serviceName}', {serviceName});");
            }

            sb.AppendLine();

            foreach (var controller in controllers)
            {
                AppendServiceWrapper(controller, sb);
            }

            sb.AppendLine(
                @"
})();");

            return sb.ToString();
        }

        private string GetServiceName(HttpControllerDescriptor controllerKey)
        {
            return ConvertPascalToCamelCase(controllerKey.ControllerName);
        }

        private void AppendServiceWrapper(
            IGrouping<HttpControllerDescriptor, ApiDescription> apiDescription,
            StringBuilder sb)
        {
            sb.AppendLine(
$@"  function {GetServiceName(apiDescription.Key)}($http) {{");
            
            foreach (var method in apiDescription)
            {
                string[] parameterNames = GetMethodParameterNames(method);

                sb.AppendLine(
$@"    this.{GetMethodName(method)} = function({(parameterNames.Length > 0 ? "parameters" : "")}) {{
      return $http({{");

                if (!method.HttpMethod.IsSafe())
                {
                    sb.AppendLine(
$@"        'xsrfCookieName': '{AntiForgeryConsts.CookieFormTokenName}',
        'xsrfHeaderName': '{AntiForgeryConsts.HeaderTokenName}', ");
                }

                if (parameterNames.Length > 0)
                {
                    if (method.HttpMethod.HasBody())
                    {
                        sb.AppendLine("        'data': parameters,");
                    }
                    else
                    {
                        if (parameterNames.Length == 1
                            && method.ParameterDescriptions[0].Source == ApiParameterSource.FromUri
                            && (method.ParameterDescriptions[0].ParameterDescriptor.ParameterType.IsClass
                                || method.ParameterDescriptions[0].ParameterDescriptor.ParameterType.IsValueType
                                    && !method.ParameterDescriptions[0].ParameterDescriptor.ParameterType.IsEnum)
                            && (!method.ParameterDescriptions[0].ParameterDescriptor.ParameterType.AssemblyQualifiedName?.StartsWith("System.") ?? true))
                        {
                            sb.AppendLine("        'params': parameters,");
                        }
                        else
                        {
                            sb.AppendLine("        'params': {");

                            for (int i = 0; i < parameterNames.Length; i++)
                            {
                                sb.AppendLine(
                                    $@"          '{parameterNames[i]}': parameters.{parameterNames[i]}{(i != parameterNames.Length - 1
                                        ? ","
                                        : "")}");
                            }

                            sb.AppendLine("        },");
                        }
                    }
                }

                sb.AppendLine(
$@"        'method': '{method.HttpMethod}',
        'url': '{method.ActionDescriptor.ControllerDescriptor.Configuration.VirtualPathRoot}{SanitizeUrl(method.RelativePath)}'
      }}).then(function (response) {{
        return response.data;
      }});
    }}");
            }

            sb.AppendLine(@"  }");
        }

        private string SanitizeUrl(string url)
        {
            int queryPos = url.IndexOf("?", StringComparison.Ordinal);
            string newUrl = queryPos > 0 ? url.Substring(0, queryPos) : url;
            string[] parts = newUrl.Split(new[] {'/'});

            if (parts.Length >= 3 && url.IndexOf(".", StringComparison.Ordinal) == -1)
            {
                return parts[0] + "/" + ConvertPascalCaseToDashes(parts[1]).ToLowerInvariant()
                   + "/" + ConvertPascalCaseToDashes(parts[2]).ToLowerInvariant();
            }
            else
            {
                return newUrl;
            }
        }

        private string[] GetMethodParameterNames(ApiDescription apiDescription)
        {
            var parameters = apiDescription.ActionDescriptor.GetParameters();
            return parameters
                .Where(x => !typeof(ODataQueryOptions).IsAssignableFrom(x.ParameterType))
                .Select(x => ConvertPascalToCamelCase(x.ParameterName))
                .ToArray();
        }

        private string GetMethodName(ApiDescription apiDescription)
        {
            return ConvertPascalToCamelCase(apiDescription.ActionDescriptor.ActionName);
        }

        private string ConvertPascalToCamelCase(string name)
        {
            int i = 0;
            string newName = Char.ToLowerInvariant(name[i]).ToString();
            i++;

            for (; i + 1 < name.Length && Char.IsUpper(name[i + 1]); i++)
            {
                newName += Char.ToLowerInvariant(name[i]);
            }

            return newName + name.Substring(i);
        }

        private string ConvertPascalCaseToDashes(string name)
        {
            return Regex.Replace(name, "([A-Z])([A-Z][a-z])|([a-z0-9])(?=[A-Z])", "$1$3-$2");
        }
    }
}
