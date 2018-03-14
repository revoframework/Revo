using System;
using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Security
{
    public class PermissionTree
    {
        public Dictionary<Guid, ContextsToResources> Permissions { get; private set; }
            = new Dictionary<Guid, ContextsToResources>();

        public void Initialize(IEnumerable<Permission> permissions)
        {
            Permissions.Clear();

            foreach (Permission permission in permissions)
            {
                ContextsToResources contextsToResources;
                if (!Permissions.TryGetValue(permission.PermissionType.Id, out contextsToResources))
                {
                    contextsToResources = new ContextsToResources();
                    Permissions[permission.PermissionType.Id] = contextsToResources;

                    string contextId = permission.ContextId ?? string.Empty;
                    Resources resources;
                    if (!contextsToResources.TryGetValue(contextId, out resources))
                    {
                        resources = new Resources();
                        contextsToResources[contextId] = resources;
                    }

                    resources.Add(permission.ResourceId ?? string.Empty);
                }
            }
        }

        public bool AuthorizePermission(Permission requiredPermission)
        {
            ContextsToResources contextsToResources;
            if (!Permissions.TryGetValue(requiredPermission.PermissionType.Id, out contextsToResources))
            {
                return false;
            }
            
            if (requiredPermission.ContextId?.Length > 0)
            {
                Resources resources;
                if (!contextsToResources.TryGetValue(requiredPermission.ContextId, out resources)
                    && !contextsToResources.TryGetValue(string.Empty, out resources))
                {
                    return false;
                }

                if (requiredPermission.ResourceId?.Length > 0)
                {
                    if (!resources.Contains(requiredPermission.ResourceId)
                    && !resources.Contains(string.Empty))
                    {
                        return false;
                    }
                }
                else //accept any resource ID
                {
                    if (resources.Count == 0)
                    {
                        return false;
                    }
                }
                
            }
            else //accept any context ID
            {
                if (!contextsToResources.Any(
                    resourcesKV =>
                    {
                        Resources resources = resourcesKV.Value;

                        if (requiredPermission.ResourceId?.Length > 0)
                        {
                            if (!resources.Contains(requiredPermission.ResourceId)
                            && !resources.Contains(string.Empty))
                            {
                                return false;
                            }
                        }
                        else //accept any resource ID
                        {
                            if (resources.Count == 0)
                            {
                                return false;
                            }
                        }

                        return true;
                    }))
                {
                    return false;
                }
            }

            return true;
        }

        public class ContextsToResources : Dictionary<string, Resources>
        {
        }

        public class Resources : HashSet<string>
        {
        }
    }
}
