using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Filters;
using GTRevo.Platform.Globalization;
using Newtonsoft.Json;

namespace GTRevo.Platform.Web
{
    class PathItem
    {
        public PathItem(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    class CollectionPathItem : PathItem
    {
        public CollectionPathItem(string name) : base(name)
        {
        }
    }

    public class TranslateAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            //TODO: get actual locality from header
            string culture = "cs-CZ";
            var content = actionExecutedContext.Response.Content as ObjectContent;
            if (content != null)
            {
                var type = content.Value.GetType();
                foreach (var path in GetPathsToTranslatables(type))
                {
                    InjectCulture(culture, path, content.Value);
                }
            }
        }

        private void InjectCulture(string culture, List<PathItem> path, object target)
        {
            var currentType = target.GetType();
            object currentTarget = target;
            PropertyInfo currentProperty = null;
            for (int i = 0; i < path.Count; i++)
            {
                currentProperty = currentType.GetProperty(path[i].Name);
                currentType = currentProperty.PropertyType;
                if (path[i] is CollectionPathItem)
                {
                    var collection = (ICollection) currentProperty.GetValue(currentTarget);
                    foreach (var item in collection)
                    {
                        InjectCulture(culture,path.Skip(i+1).ToList(), item);
                    }
                    return;
                }
                else
                {
                    currentTarget = currentProperty.GetValue(currentTarget);
                }
            }
            if (currentProperty != null)
            {
                currentTarget.GetType().GetProperty(nameof(ITranslatable.Culture)).SetValue(currentTarget, culture);
            }
        }

        private static readonly Dictionary<Type, List<List<PathItem>>> TranslatablePathCache = new Dictionary<Type, List<List<PathItem>>>();
        
        internal List<List<PathItem>> GetPathsToTranslatables(Type type)
        {
            if (typeof(ITranslatable).IsAssignableFrom(type))
                return new List<List<PathItem>>{new List<PathItem>()};
            if (TranslatablePathCache.ContainsKey(type))
                return TranslatablePathCache[type];
            var result = new List<List<PathItem>>();
            foreach (
                var prop in
                type.GetProperties()
                    .Where(p => !p.GetCustomAttributes<JsonIgnoreAttribute>().Any() && p.PropertyType.IsClass))
            {
                if (typeof(ITranslatable).IsAssignableFrom(prop.PropertyType))
                {
                    result.Add(new List<PathItem> { new PathItem(prop.Name)});
                }
                else if (typeof(ICollection).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                {
                    var collectionType = prop.PropertyType.GenericTypeArguments.First();
                    foreach (var subpath in GetPathsToTranslatables(collectionType))
                    {
                        var newPath = new List<PathItem> { new CollectionPathItem(prop.Name) };
                        newPath.AddRange(subpath);
                        result.Add(newPath);
                    }
                }
                else
                {
                    foreach (var subpath in GetPathsToTranslatables(prop.PropertyType))
                    {
                        var newPath = new List<PathItem> {new PathItem(prop.Name)};
                        newPath.AddRange(subpath);
                        result.Add(newPath);
                    }
                }
            }
            TranslatablePathCache.Add(type,result);
            return result;
        }
    }
}
