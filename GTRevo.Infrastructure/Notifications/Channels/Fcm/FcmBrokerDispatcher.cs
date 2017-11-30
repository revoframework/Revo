using System;
using System.Collections.Generic;
using GTRevo.Core.Core.Lifecycle;
using NLog;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmBrokerDispatcher : IFcmBrokerDispatcher, IApplicationStartListener, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly GcmConfiguration fcmConfiguration;
        private GcmServiceBroker fcmServiceBroker; // TODO might pool multiple instances?

        public FcmBrokerDispatcher(GcmConfiguration fcmConfiguration)
        {
            this.fcmConfiguration = fcmConfiguration;
        }

        public void OnApplicationStarted()
        {
            if (fcmConfiguration != null)
            {
                fcmServiceBroker = new GcmServiceBroker(fcmConfiguration);
                fcmServiceBroker.OnNotificationSucceeded += FcmServiceBrokerNotificationSucceeded;
                fcmServiceBroker.OnNotificationFailed += FcmServiceBrokeNotificationFailed;
                fcmServiceBroker.Start();
            }
            else
            {
                Logger.Info("No Google Cloud Messaging connection configured");
            }
        }

        public void QueueNotification(GcmNotification notification)
        {
            if (fcmServiceBroker != null)
            {
                lock (fcmServiceBroker)
                {
                    fcmServiceBroker.QueueNotification(notification);
                }
            }
        }

        public void QueueNotifications(IEnumerable<GcmNotification> notifications)
        {
            if (fcmServiceBroker != null)
            {
                lock (fcmServiceBroker)
                {
                    foreach (GcmNotification notification in notifications)
                    {
                        fcmServiceBroker.QueueNotification(notification);
                    }
                }
            }
        }

        public void Dispose()
        {
            fcmServiceBroker?.Stop();
            fcmServiceBroker = null;
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
