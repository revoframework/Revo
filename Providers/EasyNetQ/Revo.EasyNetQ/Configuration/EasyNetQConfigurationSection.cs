using System;
using EasyNetQ.DI;
using Revo.Core.Configuration;

namespace Revo.EasyNetQ.Configuration
{
    public class EasyNetQConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
        public EasyNetQConnectionConfiguration Connection { get; set; } = new EasyNetQConnectionConfiguration("host=localhost");
        public EasyNetQSubscriptionsConfiguration Subscriptions { get; set; } = new EasyNetQSubscriptionsConfiguration();
        public EasyNetQEventTransportsConfiguration EventTransports { get; set; } = new EasyNetQEventTransportsConfiguration();
        public Action<IServiceRegister> RegisterServices { get; set; }
    }
}
