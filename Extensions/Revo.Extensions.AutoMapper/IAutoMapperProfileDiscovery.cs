using System.Collections.Generic;
using AutoMapper;

namespace Revo.Extensions.AutoMapper
{
    public interface IAutoMapperProfileDiscovery
    {
        void DiscoverProfiles();
        IReadOnlyCollection<Profile> GetProfiles();
    }
}