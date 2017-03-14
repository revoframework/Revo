using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GTRevo.Platform.IO.Resources
{
    public interface IResourceManager
    {
        IDictionary<string, List<EmbeddedResource>> Resources { get; }
        Func<EmbeddedResource, bool> UseLocalIfAvailable { get; set; }
        Func<EmbeddedResource, bool> UseResource { get; set; }

        void AddAsembly(Assembly assembly, string projectSourcePath = null, params KeyValuePair<string, string>[] pathMappings);
        Stream TryCreateReadStream(string virtualPath);
        Stream CreateReadStream(string virtualPath);
        bool FileExists(string virtualPath);
        string GetResoucePathFromVirtualPath(string virtualPath);
        EmbeddedResource GetResourceFromVirtualPath(string virtualPath);
    }
}