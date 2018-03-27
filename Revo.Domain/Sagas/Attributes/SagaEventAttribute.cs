using System;

namespace Revo.Domain.Sagas.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SagaEventAttribute : Attribute
    {
        public bool IsAlwaysStarting { get; set; }
        public bool IsStartingIfSagaNotFound { get; set; }
        public string SagaKey { get; set; }
        public string EventKey { get; set; }
    }
}
