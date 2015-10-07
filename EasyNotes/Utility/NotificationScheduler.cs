using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace EasyNotes.Utility
{
    class NotificationScheduler
    {
        public static string ScheduleNotification(XmlDocument toastXml, DateTimeOffset date)
        {
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
            ScheduledToastNotification customAlarmScheduledToast = new ScheduledToastNotification(toastXml, date);
            toastNotifier.AddToSchedule(customAlarmScheduledToast);
            return customAlarmScheduledToast.Id;
        }

        public static void DeleteScheduledNotification(string id)
        {
            IReadOnlyList<ScheduledToastNotification> scheduled = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            foreach (ScheduledToastNotification notify in scheduled)
            {
                if (notify.Id == id)
                {
                    ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(notify);
                }
            }
        }

        public static string ScheduleNotification(string id, XmlDocument toastXml, DateTimeOffset date)
        {
            DeleteScheduledNotification(id);
            return ScheduleNotification(toastXml, date);
        }

    }
}
