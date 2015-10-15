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

namespace EasyNotes.Data.NoteManager
{
    class TodoNoteManager : INoteManager
    {
        DatabaseHelper.TodoNoteDataHelper databaseHelper = new DatabaseHelper.TodoNoteDataHelper();

        public void AddNote(string title, ObservableCollection<TodoNote.TodoEntry> entries)
        {
            databaseHelper.AddNote(title, entries);
        }

        public void AddNoteAndNotification(string title, ObservableCollection<TodoNote.TodoEntry> entries, string notificationTitle, DateTimeOffset dateTime)
        {
            string schedulingId = ScheduleNotification(notificationTitle, dateTime);
            databaseHelper.AddNote(title, entries, schedulingId, dateTime);
        }

        public ObservableCollection<BaseNote> GetAllNotes()
        {
            return databaseHelper.GetAllNotes();
        }

        public BaseNote GetNoteById(long id)
        {
            TodoNote todoNote = (TodoNote)databaseHelper.GetNoteById(id);
            if (todoNote.ScheduledNotification != null && todoNote.ScheduledNotification.Date.Add(todoNote.ScheduledNotification.Time) < DateTimeOffset.Now)
            {
                Debug.WriteLine("Notification too old..deleting");
                this.DeleteNotification(id);
                todoNote.ScheduledNotification = null;
            }
            return databaseHelper.GetNoteById(id);
        }

        public void DeleteNote(long id)
        {
            ScheduledNotification notification;
            if ((notification = databaseHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            databaseHelper.DeleteNote(id);
        }

        public void UpdateNote(long id, string title, ObservableCollection<TodoNote.TodoEntry> entries)
        {
            ScheduledNotification notification;
            if((notification = databaseHelper.GetNotificationByNoteId(id)) != null){
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            databaseHelper.UpdateNoteAndDeleteNotification(id, title, entries);
        }

        public void UpdateNoteAndNotification(long id, string title, ObservableCollection<TodoNote.TodoEntry> entries, string notificationTitle, DateTimeOffset dateTime)
        {
            ScheduledNotification notification;
            if ((notification = databaseHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            string schedulingId = ScheduleNotification(notificationTitle, dateTime);
            databaseHelper.UpdateNoteAndNotification(id, title, entries, schedulingId, dateTime);
        }

        private string ScheduleNotification(string notificationTitle, DateTimeOffset dateTime)
        {
            XmlDocument notificationStructure = NotificationBuilder.BuildNoTitleNotification(notificationTitle);
            return NotificationScheduler.ScheduleNotification(notificationStructure, dateTime);
        }

        private void DeleteNotification(long noteID)
        {
            ScheduledNotification schedulednotification = databaseHelper.GetNotificationByNoteId(noteID);
            NotificationScheduler.DeleteScheduledNotification(schedulednotification.SchedulingId);
            databaseHelper.DeleteNotificationByNoteId(noteID);
        }
    }
}
