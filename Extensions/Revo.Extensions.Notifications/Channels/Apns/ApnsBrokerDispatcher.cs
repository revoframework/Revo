using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NLog;
using PushSharp.Apple;
using Revo.Core.Lifecycle;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public class ApnsBrokerDispatcher : IApnsBrokerDispatcher, IApplicationStartListener, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, ApnsServiceBroker> apnsServiceBrokers; // TODO might pool multiple instances?
        
        public ApnsBrokerDispatcher(IEnumerable<ApnsAppConfiguration> apnsAppConfigurations)
        {
            if (apnsAppConfigurations.DistinctBy(x => x.AppId).Count() != apnsAppConfigurations.Count())
            {
                throw new ArgumentException("Duplicate AppIds specified for ApnsBrokerDispatcher");
            }

            apnsServiceBrokers = apnsAppConfigurations.ToDictionary(x => x.AppId,
                x =>
                {
                    var broker = new ApnsServiceBroker(x.ApnsConfiguration);
                    broker.OnNotificationSucceeded += ApnsServiceBrokerNotificationSucceeded;
                    broker.OnNotificationFailed += ApnsServiceBrokeNotificationFailed;
                    return broker;
                });
        }

        public void OnApplicationStarted()
        {
            foreach (var broker in apnsServiceBrokers)
            {
                broker.Value.Start();
            }

            Logger.Info($"Started {apnsServiceBrokers.Count} APNS brokers");
        }

        public void QueueNotification(WrappedApnsNotification notification)
        {
            if (apnsServiceBrokers.TryGetValue(notification.AppId, out var broker))
            {
                lock (broker)
                {
                    broker.QueueNotification(notification.Notification);
                }
            }
            else
            {
                Logger.Trace($"No APNS broker set-up for AppId {notification.AppId}, message won't be sent");
            }
        }

        public void QueueNotifications(IEnumerable<WrappedApnsNotification> notifications)
        {
            foreach (var perApp in notifications.GroupBy(x => x.AppId))
            {
                if (apnsServiceBrokers.TryGetValue(perApp.Key, out var broker))
                {
                    lock (broker)
                    {
                        foreach (var notification in perApp)
                        {
                            broker.QueueNotification(notification.Notification);
                        }
                    }
                }
                else
                {
                    Logger.Trace($"No APNS broker set-up for AppId {perApp.Key}, message(s) won't be sent");
                }
            }
        }

        public void Dispose()
        {
            apnsServiceBrokers.ForEach(x => x.Value.Stop());
        }

        private void ApnsServiceBrokeNotificationFailed(ApnsNotification notification, AggregateException exception)
        {
            Logger.Error(exception, $"Failed to deliver APNS notification: {exception.Message}");
        }

        private void ApnsServiceBrokerNotificationSucceeded(ApnsNotification notification)
        {
        }
    }
}
