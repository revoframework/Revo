using System.Collections.Generic;
using AutoMapper;

namespace Revo.Core.IO
{
    public interface IAutoMapperProfileDiscovery
    {
        void DiscoverProfiles();
        IReadOnlyCollection<Profile> GetProfiles();
    }
}