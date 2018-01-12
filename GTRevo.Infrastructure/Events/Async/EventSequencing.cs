using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Events.Async
{
    public struct EventSequencing
    {
        public string SequenceName { get; set; }
        public long? EventSequenceNumber { get; set; }
    }
}
