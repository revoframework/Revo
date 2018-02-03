using System;

namespace Revo.Domain.Sagas.Attributes
{
    public class SagaEventAttribute : Attribute
    {
        public bool IsAlwaysStarting { get; set; }
        public bool IsStartingIfSagaNotFound { get; set; }
        public string SagaKey { get; set; }
        public string EventKey { get; set; }
    }
}
