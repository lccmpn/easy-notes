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
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace EasyNotes.Data.NoteManager
{
    class PhotoNoteManager : INoteManager
    {
        private DatabaseHelper.PhotoNoteHelper databaseHelper = new DatabaseHelper.PhotoNoteHelper();
        public const string PHOTO_PATH_KEY = "photo_path";

        public void AddNote(string title, string photoPath, string content)
        {
            databaseHelper.AddNote(title, content, photoPath);
        }

        public void AddNoteAndNotification(string title, string photoPath, string content, string notificationTitle, DateTimeOffset dateTime)
        {
            string schedulingId = ScheduleNotification(notificationTitle, dateTime);
            databaseHelper.AddNote(title, content, photoPath, schedulingId, dateTime);
        }

        private string ScheduleNotification(string notificationTitle, DateTimeOffset dateTime)
        {
            XmlDocument notificationStructure = NotificationBuilder.BuildNoTitleNotification(notificationTitle);
            return NotificationScheduler.ScheduleNotification(notificationStructure, dateTime);
        }

        public ObservableCollection<BaseNote> GetAllNotes()
        {
            return databaseHelper.GetAllNotes();
        }

        public BaseNote GetNoteById(long id)
        {
            PhotoNote photoNote = databaseHelper.GetNoteById(id) as PhotoNote;
            if (photoNote.ScheduledNotification != null && photoNote.ScheduledNotification.Date.Add(photoNote.ScheduledNotification.Time) < DateTimeOffset.Now)
            {
                DeleteNotification(id);
                photoNote.ScheduledNotification = null;
            }
            return databaseHelper.GetNoteById(id);
        }

        public void UpdateNote(long id, string title, string photoPath, string content)
        {
            PhotoNote photoNote = databaseHelper.GetNoteById(id) as PhotoNote;
            ScheduledNotification notification = photoNote.ScheduledNotification;
            if (notification != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            if (photoNote.PhotoPath != photoPath)
            {
                DeleteImageFromLocalFolder(photoNote.PhotoPath);
            }
            databaseHelper.UpdateNoteAndDeleteNotification(id, title, content, photoPath);
        }

        public void UpdateNoteAndNotification(long id, string title, string photoPath, string content, string notificationTitle, DateTimeOffset dateTime)
        {
            ScheduledNotification notification;
            if ((notification = databaseHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            PhotoNote photoNote = databaseHelper.GetNoteById(id) as PhotoNote;
            if (photoNote.PhotoPath != photoPath)
            {
                DeleteImageFromLocalFolder(photoNote.PhotoPath);
            }
            string schedulingId = ScheduleNotification(notificationTitle, dateTime);
            databaseHelper.UpdateNoteAndNotification(id, title, content, photoPath, schedulingId, dateTime);
        }

        public void DeleteNote(long id)
        {
            ScheduledNotification notification;
            if ((notification = databaseHelper.GetNotificationByNoteId(id)) != null)
            {
                Debug.WriteLine("Deleting scheduled notification");
                NotificationScheduler.DeleteScheduledNotification(notification.SchedulingId);
            }
            DeleteImageFromLocalFolder(id);
            databaseHelper.DeleteNote(id);
        }

        public void DeleteNotification(long noteId){
            ScheduledNotification schedulednotification = databaseHelper.GetNotificationByNoteId(noteId);
            NotificationScheduler.DeleteScheduledNotification(schedulednotification.SchedulingId);
            databaseHelper.DeleteNotificationByNoteId(noteId);
        }
        
        private async void DeleteImageFromLocalFolder(long id)
        {
            PhotoNote note = (PhotoNote)GetNoteById(id);
            Debug.WriteLine("Image to delete " + note.PhotoPath);
            StorageFile file = await StorageFile.GetFileFromPathAsync(note.PhotoPath);
            await file.DeleteAsync();
        }

        public async void DeleteImageFromLocalFolder(string path)
        {
            Debug.WriteLine("Image to delete " + path);
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            await file.DeleteAsync();
        }

    }
}
