using Microsoft.EntityFrameworkCore.Query.Internal;
using Revo.Core.Core;

namespace Revo.EFCore.DataAccess.Query
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public class CustomQueryProvider : EntityQueryProvider
    {
        public CustomQueryProvider(IQueryCompiler queryCompiler, IServiceLocator serviceLocator) : base(queryCompiler)
        {
            ServiceLocator = serviceLocator;
        }
        
        public IServiceLocator ServiceLocator { get; }
    }
}