using System;
using System.Collections.Generic;

namespace Revo.Infrastructure.Sagas
{
    public interface ISagaTypeConfiguration
    {
        Type SagaType { get; }
        IEnumerable<SagaEventRegistration> EventRegistrations { get; }
    }
}
