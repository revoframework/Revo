using System;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Revo.EFCore.DataAccess.Query
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
    public class CustomQueryProvider : EntityQueryProvider
    {
        public CustomQueryProvider(IQueryCompiler queryCompiler, IServiceProvider serviceProvider) : base(queryCompiler)
        {
            ServiceProvider = serviceProvider;
        }
        
        public IServiceProvider ServiceProvider { get; }
    }
}
