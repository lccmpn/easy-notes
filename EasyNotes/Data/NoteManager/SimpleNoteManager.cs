using EasyNotes.Data.Database;
using EasyNotes.Data.Model;
using EasyNotes.Database;
using EasyNotes.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace EasyNotes.Data
{
    public class SimpleNoteManager : INoteManager
    {
        DatabaseHelper.SimpleNoteHelper simpleNoteHelper = new DatabaseHelper.SimpleNoteHelper();

        public void DeleteNote(long id)
        {
            Debug.WriteLine("Deleting notification");
            ScheduledNotification notification;
            if ((notification = simpleNoteHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            simpleNoteHelper.DeleteNote(id);
        }

        public void AddNote(string title, string content)
        {
            simpleNoteHelper.AddNote(title, content);
        }

        public void AddNoteAndNotification(string title, string content, string notificationTitle, DateTimeOffset dateTime)
        {
            Debug.WriteLine("Data inside AddNoteAndNotification" + dateTime);
            string notificationId = ScheduleNotification(notificationTitle, dateTime);
            simpleNoteHelper.AddNote(title, content, notificationId, dateTime);
        }

        public ObservableCollection<Model.BaseNote> GetAllNotes()
        {
            return simpleNoteHelper.GetAllNotes();
        }

        public BaseNote GetNoteById(long id)
        {
            SimpleNote simpleNote = (SimpleNote) simpleNoteHelper.GetNoteById(id);
            if (simpleNote.ScheduledNotification != null && simpleNote.ScheduledNotification.Date.Add(simpleNote.ScheduledNotification.Time) < DateTimeOffset.Now)
            {
                Debug.WriteLine("Notification too old..deleting");
                this.DeleteNotification(id);
                simpleNote.ScheduledNotification = null;
            }
            return simpleNote;
        }

        public void UpdateNote(long id, string title, string content)
        {
            ScheduledNotification notification;
            if ((notification = simpleNoteHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            simpleNoteHelper.UpdateNoteAndDeleteNotification(id, title, content);
        }

        public void UpdateNoteAndNotification(long id, string title, string content, string notificationTitle, DateTimeOffset dateTime)
        {
            ScheduledNotification notification;
            if ((notification = simpleNoteHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            string notificationId = ScheduleNotification(notificationTitle, dateTime);
            simpleNoteHelper.UpdateNoteAndNotification(id, title, content, notificationId, dateTime);

        }

        private void DeleteNotification(long noteID)
        {
            ScheduledNotification schedulednotification = simpleNoteHelper.GetNotificationByNoteId(noteID);
            NotificationScheduler.DeleteScheduledNotification(schedulednotification.SchedulingId);
            simpleNoteHelper.DeleteNotificationByNoteId(noteID);
        }

        private string ScheduleNotification(string notificationTitle, DateTimeOffset dateTime)
        {
            XmlDocument notificationStructure = NotificationBuilder.BuildNoTitleNotification(notificationTitle);
            return NotificationScheduler.ScheduleNotification(notificationStructure, dateTime);
        }

    }
}
