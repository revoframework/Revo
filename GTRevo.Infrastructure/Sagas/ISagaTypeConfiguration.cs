using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Sagas
{
    public interface ISagaTypeConfiguration
    {
        Type SagaType { get; }
        IEnumerable<SagaEventRegistration> EventRegistrations { get; }
    }
}
