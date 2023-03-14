using System;

namespace Revo.Infrastructure.Sagas
{
    public struct SagaMatch
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
    }
}
