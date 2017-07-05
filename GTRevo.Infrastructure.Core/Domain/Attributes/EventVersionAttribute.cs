using System;

namespace GTRevo.Infrastructure.Core.Domain.Attributes
{
    public class EventVersionAttribute : Attribute
    {
        public EventVersionAttribute(int version)
        {
            Version = version;
        }

        public int Version { get; private set; }
    }
}
