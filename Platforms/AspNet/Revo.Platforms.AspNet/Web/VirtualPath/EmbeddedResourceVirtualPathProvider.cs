using System;
using System.Collections;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using Revo.Platforms.AspNet.IO.Resources;

namespace Revo.Platforms.AspNet.Web.VirtualPath
{
    public class EmbeddedResourceVirtualPathProvider : VirtualPathProvider, IEnumerable
    {
        private IResourceManager resourceManager;

        public EmbeddedResourceVirtualPathProvider(IResourceManager resourceManager)
        {
            this.resourceManager = resourceManager;
            CacheControl = er => null;
        }
        
        public Func<EmbeddedResource, EmbeddedResourceCacheControl> CacheControl { get; set; }
        
        public override bool FileExists(string virtualPath)
        {
            return (base.FileExists(virtualPath) || resourceManager.FileExists(virtualPath));
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            //if (base.FileExists(virtualPath)) return base.GetFile(virtualPath);
            EmbeddedResource resource = resourceManager.GetResourceFromVirtualPath(virtualPath);
            if (resource != null)
                return new EmbeddedResourceVirtualFile(virtualPath, resource, CacheControl(resource));
            return base.GetFile(virtualPath);
        }

        public override string CombineVirtualPaths(string basePath, string relativePath)
        {
            var combineVirtualPaths = base.CombineVirtualPaths(basePath, relativePath);
            return combineVirtualPaths;
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            var fileHash = base.GetFileHash(virtualPath, virtualPathDependencies);
            return fileHash;
        }

        public override string GetCacheKey(string virtualPath)
        {
            var resource = resourceManager.GetResourceFromVirtualPath(virtualPath);
            if (resource != null)
            {
                return (virtualPath + resource.AssemblyName + resource.AssemblyLastModified.Ticks).GetHashCode().ToString();
            }
            return base.GetCacheKey(virtualPath);
        }
        
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            var resource = resourceManager.GetResourceFromVirtualPath(virtualPath);
            if (resource != null)
            {
                return resource.GetCacheDependency(utcStart);
            }

            var embeddedResourceDependencies = virtualPathDependencies.OfType<string>()
                .Select(x => new { path = x, resource = resourceManager.GetResourceFromVirtualPath(x) })
                .Where(x => x.resource != null)
                .ToList();

            if (embeddedResourceDependencies.Any())
            {
                virtualPathDependencies = virtualPathDependencies.OfType<string>()
                    .Except(embeddedResourceDependencies.Select(v => v.path))
                    .Concat(embeddedResourceDependencies.Select(v => $"/bin/{v.resource.AssemblyName}").Distinct());
            }

            if (DirectoryExists(virtualPath) || FileExists(virtualPath))
            {
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
            }

            return null;
        }

        private bool ShouldUsePrevious(string virtualPath, EmbeddedResource resource)
        {
            return base.FileExists(virtualPath) && resourceManager.UseLocalIfAvailable(resource);
        }

        
        //public override string GetCacheKey(string virtualPath)
        //{
        //    var resource = GetResourceFromVirtualPath(virtualPath);
        //    if (resource != null) return virtualPath + "blah";
        //    return base.GetCacheKey(virtualPath);
        //}

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException("Only got this so that we can use object collection initializer syntax");
        }
    }
}