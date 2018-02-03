using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Security;

namespace Revo.Platforms.AspNet.Security
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

                    Guid contextId = permission.ContextId ?? Guid.Empty;
                    Resources resources;
                    if (!contextsToResources.TryGetValue(contextId, out resources))
                    {
                        resources = new Resources();
                        contextsToResources[contextId] = resources;
                    }

                    resources.Add(permission.ResourceId ?? Guid.Empty);
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
            
            if (requiredPermission.ContextId.HasValue)
            {
                Resources resources;
                if (!contextsToResources.TryGetValue(requiredPermission.ContextId, out resources)
                    && !contextsToResources.TryGetValue(Guid.Empty, out resources))
                {
                    return false;
                }

                if (requiredPermission.ResourceId.HasValue)
                {
                    if (!resources.Contains(requiredPermission.ResourceId.Value)
                    && !resources.Contains(Guid.Empty))
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

                        if (requiredPermission.ResourceId.HasValue)
                        {
                            if (!resources.Contains(requiredPermission.ResourceId.Value)
                            && !resources.Contains(Guid.Empty))
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

        public class ContextsToResources : Dictionary<Guid?, Resources>
        {
        }

        public class Resources : HashSet<Guid?>
        {
        }
    }
}
