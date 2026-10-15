using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Revo.Core.Types;

namespace Revo.Core.Security;

public class PermissionTypeIndexer : IPermissionTypeIndexer, IHostedLifecycleService
{
    private readonly IPermissionTypeRegistry permissionTypeRegistry;
    private readonly ITypeExplorer typeExplorer;
    private readonly HashSet<Type> registeredCatalogTypes = new HashSet<Type>();

    public PermissionTypeIndexer(ITypeExplorer typeExplorer,
        IPermissionTypeRegistry permissionTypeRegistry)
    {
        this.typeExplorer = typeExplorer;
        this.permissionTypeRegistry = permissionTypeRegistry;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Index();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void EnsureIndexed()
    {
        if (registeredCatalogTypes.Count == 0)
        {
            Index();
        }
    }

    private void Index()
    {
        foreach (Type catalogType in typeExplorer.GetAllTypes()
                     .Where(x => x.GetCustomAttributes(typeof(PermissionTypeCatalogAttribute), false).Any()))
        {
            if (!catalogType.IsClass
                || !catalogType.IsAbstract /* static in C# = abstract + sealed */
                || !catalogType.IsSealed)
            {
                throw new InvalidOperationException($"Cannot register permission catalog {catalogType.FullName}: all catalogs must be of a static class type");
            }

            if (!registeredCatalogTypes.Contains(catalogType))
            {
                RegisterCatalog(catalogType);
                registeredCatalogTypes.Add(catalogType);
            }
        }
    }

    private void RegisterCatalog(Type catalogType)
    {
        PermissionTypeCatalogAttribute catalogAttribute = (PermissionTypeCatalogAttribute)catalogType
            .GetCustomAttributes(typeof(PermissionTypeCatalogAttribute), false)
            .First();

        foreach (var field in catalogType.GetFields(
                     System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))
        {
            if (!field.IsStatic || !field.IsLiteral || field.IsInitOnly  /* exclude readonly */
                || field.FieldType != typeof(string))
            {
                throw new InvalidOperationException($"Invalid permission catalog field {catalogType.FullName}.{field.Name}: permission catalogs must contain only static const string fields");
            }

            try
            {
                PermissionType permissionType = new PermissionType(
                    Guid.Parse((string)field.GetValue(null)),
                    catalogAttribute.CatalogName + "." + field.Name);
                permissionTypeRegistry.RegisterPermissionType(permissionType);
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException)
            {
                throw new InvalidOperationException("Permission type definition is not a valid GUID string: " + field.ToString());
            }
        }
    }
}
