using Microsoft.AspNet.OData.Query;
using Revo.Core.Configuration;

namespace Revo.EFCore.AspNetCoreOData.Configuration
{
    public class EFCoreAspNetCoreODataConfigurationSection : IRevoConfigurationSection
    {
        /// <summary>
        /// Workaround for EF Core bug #12849 - fails when additionally filtering on Select-mapped entity results (e.g. when exposing
        /// DTOs mapped by AutoMapper). When true, executes queries synchronously, which prevents the bug from manifesting.
        /// </summary>
        public bool DisableAsyncQueryableResolution { get; set; }

        public ODataQuerySettings ODataQuerySettings { get; set; } = new ODataQuerySettings();
    }
}
