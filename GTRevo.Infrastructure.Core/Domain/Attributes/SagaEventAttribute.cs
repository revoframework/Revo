using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Core.Domain.Attributes
{
    public class SagaEventAttribute : Attribute
    {
        public bool IsAlwaysStarting { get; set; }
        public bool IsStartingIfSagaNotFound { get; set; }
        public string SagaKey { get; set; }
        public string EventKey { get; set; }
    }
}
