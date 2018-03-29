using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Revo.Infrastructure.Sagas
{
    public class SagaMetadata
    {
        public SagaMetadata(ImmutableDictionary<string, ImmutableList<string>> keys, Guid classId)
        {
            Keys = keys;
            ClassId = classId;
        }
        
        public ImmutableDictionary<string, ImmutableList<string>> Keys { get; }
        public Guid ClassId { get; }
    }
}
