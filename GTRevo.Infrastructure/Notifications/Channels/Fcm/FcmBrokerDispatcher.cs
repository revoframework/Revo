using System;
using System.Collections.Generic;
using System.Linq;
using GTRevo.Core.Core.Lifecycle;
using MoreLinq;
using NLog;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmBrokerDispatcher : IFcmBrokerDispatcher, IApplicationStartListener, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, GcmServiceBroker> fcmServiceBrokers; // TODO might pool multiple instances?

        public FcmBrokerDispatcher(IEnumerable<FcmAppConfiguration> fcmAppConfigurations)
        {
            if (fcmAppConfigurations.DistinctBy(x => x.AppId).Count() != fcmAppConfigurations.Count())
            {
                throw new ArgumentException("Duplicate AppIds specified for FcmBrokerDispatcher");
            }

            fcmServiceBrokers = fcmAppConfigurations.ToDictionary(x => x.AppId,
                x =>
                {
                    var broker = fcmServiceBrokers[x.AppId] = new GcmServiceBroker(x.FcmConfiguration);
                    broker.OnNotificationSucceeded += FcmServiceBrokerNotificationSucceeded;
                    broker.OnNotificationFailed += FcmServiceBrokeNotificationFailed;
                    return broker;
                });
        }

        public void OnApplicationStarted()
        {
            foreach (var broker in fcmServiceBrokers)
            {
                broker.Value.Start();
            }

            Logger.Info($"Started {fcmServiceBrokers.Count} FCM brokers");
        }

        public void QueueNotification(WrappedFcmNotification notification)
        {
            if (fcmServiceBrokers.TryGetValue(notification.AppId, out var broker))
            {
                lock (broker)
                {
                    broker.QueueNotification(notification.Notification);
                }
            }
            else
            {
                Logger.Trace($"No FCM broker set-up for AppId {notification.AppId}, message won't be sent");
            }
        }

        public void QueueNotifications(IEnumerable<WrappedFcmNotification> notifications)
        {
            foreach (var perApp in notifications.GroupBy(x => x.AppId))
            {
                if (fcmServiceBrokers.TryGetValue(perApp.Key, out var broker))
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
            fcmServiceBrokers.ForEach(x => x.Value.Stop());
        }

        private void FcmServiceBrokeNotificationFailed(GcmNotification notification, AggregateException exception)
        {
            Logger.Error(exception, $"Failed to deliver FCM notification: {exception.Message}");
        }

        private void FcmServiceBrokerNotificationSucceeded(GcmNotification notification)
        {
        }
    }
}
