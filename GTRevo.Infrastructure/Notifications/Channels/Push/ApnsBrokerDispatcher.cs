using System;
using System.Collections.Generic;
using GTRevo.Core.Core.Lifecycle;
using NLog;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Push
{
    public class ApnsBrokerDispatcher : IApnsBrokerDispatcher, IApplicationStartListener, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ApnsConfiguration apnsConfiguration;
        private ApnsServiceBroker apnsServiceBroker; // TODO might pool multiple instances?

        public ApnsBrokerDispatcher(ApnsConfiguration apnsConfiguration)
        {
            this.apnsConfiguration = apnsConfiguration;
        }

        public void OnApplicationStarted()
        {
            if (apnsConfiguration != null)
            {
                apnsServiceBroker = new ApnsServiceBroker(apnsConfiguration);
                apnsServiceBroker.OnNotificationSucceeded += ApnsServiceBrokerNotificationSucceeded;
                apnsServiceBroker.OnNotificationFailed += ApnsServiceBrokeNotificationFailed;
                apnsServiceBroker.Start();
            }
            else
            {
                Logger.Info("No Apple Push Notification Service connection configured");
            }
        }

        public void QueueNotification(ApnsNotification notification)
        {
            if (apnsServiceBroker != null)
            {
                lock (apnsServiceBroker)
                {
                    apnsServiceBroker.QueueNotification(notification);
                }
            }
        }

        public void QueueNotifications(IEnumerable<ApnsNotification> notifications)
        {
            if (apnsServiceBroker != null)
            {
                lock (apnsServiceBroker)
                {
                    foreach (ApnsNotification notification in notifications)
                    {
                        apnsServiceBroker.QueueNotification(notification);
                    }
                }
            }
        }

        public void Dispose()
        {
            apnsServiceBroker?.Stop();
            apnsServiceBroker = null;
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
