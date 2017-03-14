using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;

namespace GTRevo.Platform.IO.Resources
{
    public class ResourceManager : IResourceManager, IWebActivatorExHooks
    {
        public const string CONFIGURATION_SECTION_NAME = "resourceManager";

        private readonly ResourcePathRegistration[] resourcePathRegistrations;
        private readonly IConfiguration configuration;
        private readonly ITypeExplorer typeExplorer;

        private Dictionary<string, List<EmbeddedResource>> resources
            = new Dictionary<string, List<EmbeddedResource>>();
        private Dictionary<Assembly, string> assembliesToProjectPaths = new Dictionary<Assembly, string>();

        public ResourceManager(ResourcePathRegistration[] resourcePathRegistrations,
            IConfiguration configuration,
            ITypeExplorer typeExplorer)
        {
            this.resourcePathRegistrations = resourcePathRegistrations;
            this.configuration = configuration;
            this.typeExplorer = typeExplorer;

            UseResource = er => true;
            UseLocalIfAvailable = resource => false;
        }

        public Func<EmbeddedResource, bool> UseResource { get; set; }
        public Func<EmbeddedResource, bool> UseLocalIfAvailable { get; set; }
        public IDictionary<string, List<EmbeddedResource>> Resources { get { return resources; } }
        
        public void OnPreApplicationStart()
        {
        }

        public void OnPostApplicationStart()
        {
            var assemblies = typeExplorer.GetAllReferencedAssemblies();

            var configSection = configuration.GetSection<ResourceManagerConfigurationSection>(CONFIGURATION_SECTION_NAME);

            for (int i = 0; i < configSection.PathConfiguration.Count; i++)
            {
                var pathConfig = configSection.PathConfiguration[i];
                Assembly assembly = assemblies.FirstOrDefault(
                    x => x.GetName().Name == pathConfig.AssemblyName);
                if (assembly != null)
                {
                    assembliesToProjectPaths[assembly] = pathConfig.ProjectPath;
                }
            };

            foreach (var registration in resourcePathRegistrations)
            {
                Assembly assembly = assemblies.First(x => x.GetName().Name == registration.AssemblyName);
                string projectPath = null;
                assembliesToProjectPaths.TryGetValue(assembly, out projectPath);

                AddAsembly(assembly, projectPath ?? registration.ProjectSourcePath,
                    registration.PathMappings.ToArray());
            }
        }

        public void OnApplicationShutdown()
        {
        }

        public void AddAsembly(Assembly assembly, string projectSourcePath = null,
            params KeyValuePair<string, string>[] pathMappings)
        {
            var assemblyName = assembly.GetName().Name;
            foreach (var resourcePath in assembly.GetManifestResourceNames().Where(r => r.StartsWith(assemblyName)))
            {
                var key = resourcePath.ToUpperInvariant().Substring(assemblyName.Length).TrimStart('.');

                foreach (var mapping in pathMappings)
                {
                    if (key.StartsWith(mapping.Key.ToUpperInvariant()))
                    {
                        key = mapping.Value.ToUpperInvariant() + key.Substring(mapping.Key.Length);
                    }
                }

                if (!resources.ContainsKey(key))
                {
                    resources[key] = new List<EmbeddedResource>();
                }

                resources[key].Insert(0, new EmbeddedResource(assembly, resourcePath, projectSourcePath));
            }
        }

        public bool FileExists(string virtualPath)
        {
            return GetResourceFromVirtualPath(virtualPath) != null;
        }

        public Stream TryCreateReadStream(string virtualPath)
        {
            //if (base.FileExists(virtualPath)) return base.GetFile(virtualPath);
            var resource = GetResourceFromVirtualPath(virtualPath);
            if (resource != null)
            {
                return resource.GetStream();
            }
            else
            {
                return null; //TODO: throw
            }
        }
        
        public string GetResoucePathFromVirtualPath(string virtualPath)
        {
            var path = VirtualPathUtility.ToAppRelative(virtualPath).TrimStart('~', '/');
            var index = path.LastIndexOf("/");
            if (index != -1)
            {
                var folder = path.Substring(0, index).Replace("-", "_"); //embedded resources with "-"in their folder names are stored as "_".
                path = folder + path.Substring(index);
            }
            var cleanedPath = path.Replace('/', '.');
            var key = (cleanedPath).ToUpperInvariant();

            return key;
        }

        public EmbeddedResource GetResourceFromVirtualPath(string virtualPath)
        {
            var key = GetResoucePathFromVirtualPath(virtualPath);
            if (resources.ContainsKey(key))
            {
                var resource = resources[key].FirstOrDefault(UseResource);
                if (resource != null)
                {
                    return resource;
                }
            }
            return null;
        }

        public Stream CreateReadStream(string virtualPath)
        {
            Stream stream = TryCreateReadStream(virtualPath);
            if (stream == null)
            {
                throw new ArgumentException("Resource not found: " + virtualPath);
            }

            return stream;
        }
    }
}
