using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace EasyNotes.Utility
{
    class Scheduler
    {
        public static void ScheduleNotification(XmlDocument toastXml, DateTimeOffset date)
        {
            ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
            ScheduledToastNotification customAlarmScheduledToast = new ScheduledToastNotification(toastXml, date);
            toastNotifier.AddToSchedule(customAlarmScheduledToast);
        }
    }
}
