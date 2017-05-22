using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace GTRevo.Platform.Core
{
    public interface IAutoMapperDefinition
    {
        void Configure(IMapperConfigurationExpression config);
    }
}
