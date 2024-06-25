using System;
using System.Collections.Immutable;

namespace Revo.Infrastructure.Sagas
{
    public class SagaMetadata(ImmutableDictionary<string, ImmutableList<string>> keys, Guid classId)
    {
        public ImmutableDictionary<string, ImmutableList<string>> Keys { get; } = keys;
        public Guid ClassId { get; } = classId;
    }
}
