using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.WriteLine("Date " + date);
            ScheduledToastNotification customAlarmScheduledToast = new ScheduledToastNotification(toastXml, date);
            toastNotifier.AddToSchedule(customAlarmScheduledToast);
            return customAlarmScheduledToast.Id;
        }

        public static void DeleteScheduledNotification(string id)
        {
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
            IReadOnlyList<ScheduledToastNotification> scheduled = toastNotifier.GetScheduledToastNotifications();
            foreach (ScheduledToastNotification notify in scheduled)
            {
                if (notify.Id == id)
                {
                    toastNotifier.RemoveFromSchedule(notify);
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
