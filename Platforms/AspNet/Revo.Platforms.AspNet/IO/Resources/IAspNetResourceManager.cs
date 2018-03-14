using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Revo.Core.IO;
using Revo.Core.IO.Resources;

namespace Revo.Platforms.AspNet.IO.Resources
{
    public interface IAspNetResourceManager : IResourceManager
    {
        IDictionary<string, List<EmbeddedResource>> Resources { get; }
        Func<EmbeddedResource, bool> UseLocalIfAvailable { get; set; }
        Func<EmbeddedResource, bool> UseResource { get; set; }

        void AddAsembly(Assembly assembly, string projectSourcePath = null, params KeyValuePair<string, string>[] pathMappings);
        bool FileExists(string virtualPath);
        string GetResoucePathFromVirtualPath(string virtualPath);
        EmbeddedResource GetResourceFromVirtualPath(string virtualPath);
    }
}