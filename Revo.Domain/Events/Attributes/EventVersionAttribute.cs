using System;

namespace Revo.Domain.Events.Attributes
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
