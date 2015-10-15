using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using SQLitePCL;
using EasyNotes.Utility;
using EasyNotes.Data.Model;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EasyNotes.Data.Database;
using Windows.Storage;

namespace EasyNotes.Database
{
    class DatabaseHelper
    {
        static private SQLiteConnection dbConn;

        #region Queries for creating tables
        private const string CREATE_SIMPLE_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS SIMPLENOTES (" +
                "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "TITLE VARCHAR(100)," +
                "CONTENT VARCHAR(3000)," +
                "LAST_MODIFIED VARCHAR(23)," +
                "NOTIFICATION_ID INTEGER" +
                ");";

        private const string CREATE_TODO_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS TODONOTES (" +
               "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
               "TITLE VARCHAR(100)," +
               "LAST_MODIFIED VARCHAR(23)," +
               "NOTIFICATION_ID INTEGER" +
               ");";

        private const string CREATE_NOTES_NOTIFICATION_TABLE = "CREATE TABLE IF NOT EXISTS NOTIFICATIONS (" +
                "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "DATE_TIME VARCHAR(23)," +
                "NOTIFICATION_ID VARCHAR(16)," +
                "LAST_MODIFIED VARCHAR(23)" +
                ");";

        private const string CREATE_TODO_NOTES_CONTENT_TABLE = "CREATE TABLE IF NOT EXISTS TODONOTESCONTENT (" +
               "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
               "CONTENT VARCHAR(500)," +
               "IS_DONE INTEGER," +
               "NOTE_ID INTEGER," +
               "LAST_MODIFIED VARCHAR(23)," +
               "FOREIGN KEY(NOTE_ID) REFERENCES TODONOTES(ID)" +
               ");";

        private const string CREATE_PHOTO_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS PHOTONOTES (" +
                "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "TITLE VARCHAR(100)," +
                "CONTENT VARCHAR(3000)," +
                "PHOTO_ADRESS VARCHAR(30)," +
                "LAST_MODIFIED VARCHAR(23)," +
                "NOTIFICATION_ID INTEGER" +
                ");";

        #endregion

        public static void CreateDatabase()
        {
            dbConn = new SQLiteConnection("EasyNotesData.db");
            CreateTables();
        }

        private static void CreateTables()
        {
            dbConn.Prepare(CREATE_SIMPLE_NOTES_TABLE).Step();
            dbConn.Prepare(CREATE_NOTES_NOTIFICATION_TABLE).Step();
            dbConn.Prepare(CREATE_TODO_NOTES_TABLE).Step();
            dbConn.Prepare(CREATE_TODO_NOTES_CONTENT_TABLE).Step();
            dbConn.Prepare(CREATE_PHOTO_NOTES_TABLE).Step();
        }

        public class NotificationHelper
        {
            protected const string SELECT_SIMPLE_NOTE_NOTIFICATION = "SELECT ID, NOTIFICATION_ID, DATE_TIME " +
                                                                    "FROM NOTIFICATIONS WHERE ID = ?;";

            private const string INSERT_SIMPLE_NOTE_NOTIFICATION = "INSERT INTO NOTIFICATIONS(DATE_TIME, NOTIFICATION_ID, LAST_MODIFIED) VALUES(?, ?, ?);";

            private const string UPDATE_NOTIFICATION = "UPDATE NOTIFICATIONS SET DATE_TIME = ?, NOTIFICATION_ID = ?, LAST_MODIFIED = ? WHERE ID = ?;";

            private const string DELETE_SIMPLE_NOTE_NOTIFICATION = "DELETE FROM NOTIFICATIONS WHERE ID = ?;";
            private const string DELETE_SIMPLE_NOTE_NOTIFICATION_WITH_NOTE_ID = "DELETE FROM NOTIFICATIONS WHERE NOTE_ID = ?;";
            private const string SELECT_LAST_NOTIFICATION_INSERT_ID = "SELECT ID FROM NOTIFICATIONS ORDER BY datetime(LAST_MODIFIED) DESC LIMIT 1;";

            public static long AddNotification(string schedulingId, DateTimeOffset dateTime)
           
            {
                Debug.WriteLine("Adding notificaiton");
                ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE_NOTIFICATION);
                statement.Bind(1, TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                statement.Bind(2, schedulingId);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                Debug.WriteLine(statement.Step());
                Debug.WriteLine("Last inserted notification id: " + GetLastInsertedNotificationId());
                return GetLastInsertedNotificationId();
            }

            private static long GetLastInsertedNotificationId()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_LAST_NOTIFICATION_INSERT_ID);
                statement.Step();
                return (long)statement[0];
            }

            public static ScheduledNotification GetNotificationById(long notificationId)
            {

                ISQLiteStatement statement = dbConn.Prepare(SELECT_SIMPLE_NOTE_NOTIFICATION);
                statement.Bind(1, notificationId);
                ScheduledNotification notification = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    string dateTime = (String)statement[2];
                    Debug.WriteLine("Getting notification from database..");
                    Debug.WriteLine("Data before parsing " + dateTime);
                    Debug.WriteLine("Data parsed " + DateTimeOffset.Parse(dateTime));
                    notification = new ScheduledNotification((long)statement[0], (String)statement[1], DateTimeOffset.Parse(dateTime));
                }
                if (notification == null)
                {
                    Debug.WriteLine("notificaiton not found");
                }
                return notification;
            }

            public static void UpdateNotification(long id, string schedulingId, DateTimeOffset dateTime)
            {
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_NOTIFICATION);
                statement.Bind(1, TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                Debug.WriteLine("Updating notification: data to string " + TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                statement.Bind(2, schedulingId);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, id);
                statement.Step();
            }

            public static void DeleteNotificationById(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_SIMPLE_NOTE_NOTIFICATION);
                statement.Bind(1, id);
                statement.Step();
            }
        }

        public class SimpleNoteHelper : INoteManager
        {
            #region Query strings
            private const string SELECT_ALL_SIMPLE_NOTES = "SELECT * FROM SIMPLENOTES;";
            private const string SELECT_SIMPLE_NOTE_BY_ID = "SELECT * FROM SIMPLENOTES WHERE ID = ?;";
            private const string SELECT_LAST_NOTE_INSERT_ID = "SELECT ID FROM SIMPLENOTES ORDER BY datetime(Last_Modified) DESC LIMIT 1;";
            private const string SELECT_SIMPLE_NOTE_NOTIFICATION_BY_NOTE_ID = "SELECT NOTIFICATIONS.ID, NOTIFICATIONS.NOTIFICATION_ID, NOTIFICATIONS.DATE_TIME " +
                                                       "FROM NOTIFICATIONS INNER JOIN SIMPLENOTES ON NOTIFICATIONS.ID = SIMPLENOTES.NOTIFICATION_ID WHERE SIMPLENOTES.ID = ?;";
            private const string INSERT_SIMPLE_NOTE = "INSERT INTO SIMPLENOTES(Title, Content, Last_Modified, NOTIFICATION_ID) VALUES(?, ?, ?, ?);";
            private const string DELETE_SIMPLE_NOTE = "DELETE FROM SIMPLENOTES WHERE ID = ?;";
            private const string UPDATE_SIMPLE_NOTE = "UPDATE SIMPLENOTES SET TITLE = ?, CONTENT = ?, LAST_MODIFIED = ?, NOTIFICATION_ID = ? WHERE ID = ?;"; 
            #endregion

            public void AddNote(string title, string content)
            {
                ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, null);
                statement.Step();
            }

            public void AddNote(string title, string content, string schedulingId, DateTimeOffset dateTime)
            {
                long notificationId = NotificationHelper.AddNotification(schedulingId, dateTime);
                ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, notificationId);
                statement.Step();
            }

            public ObservableCollection<BaseNote> GetAllNotes()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_SIMPLE_NOTES);
                ObservableCollection<BaseNote> result = new ObservableCollection<BaseNote>();
                while (statement.Step() == SQLiteResult.ROW)
                {
                    long noteId = (long)statement[0];
                    result.Add(new BaseNote(noteId, (String)statement[1], null));
                }
                return result;
            }

            public BaseNote GetNoteById(long id)
            {
                Debug.WriteLine("Searching notification with note_id " + id);
                ISQLiteStatement statement = dbConn.Prepare(SELECT_SIMPLE_NOTE_BY_ID);
                statement.Bind(1, id);
                SimpleNote note = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    note = new SimpleNote((long)statement[0], (String)statement[1], null, (String)statement[2]);
                    ScheduledNotification notification;
                    if (statement[4] != null)
                    {
                        long notificationId = (long)statement[4];
                        notification = NotificationHelper.GetNotificationById((long)statement[4]);
                        note.ScheduledNotification = notification;
                    }
                }
                return note;
            }

            public ScheduledNotification GetNotificationByNoteId(long noteId)
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_SIMPLE_NOTE_NOTIFICATION_BY_NOTE_ID);
                statement.Bind(1, noteId);
                ScheduledNotification notification = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    notification = new ScheduledNotification((long)statement[0], (string)statement[1], DateTimeOffset.Parse((string)statement[2]));
                }
                return notification;
            }

            public void DeleteNotificationByNoteId(long noteId)
            {
                ScheduledNotification notification = GetNotificationByNoteId(noteId);
                if (notification != null)
                {
                    Debug.WriteLine("Deleting notification from database");
                    NotificationHelper.DeleteNotificationById(notification.DataBaseId);
                }
            }

            public void DeleteNote(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_SIMPLE_NOTE);
                statement.Bind(1, id);
                statement.Step();
                statement.Reset();
                statement.ClearBindings();
                DeleteNotificationByNoteId(id);
            }

            public void UpdateNoteAndDeleteNotification(long id, string title, string content)
            {
                DeleteNotificationByNoteId(id);
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, null);
                statement.Bind(5, id);
                statement.Step();
            }

            private void UpdateNote(long id, string title, string content, long notificationId)
            {
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, notificationId);
                statement.Bind(5, id);
                statement.Step();
            }

            public void UpdateNoteAndNotification(long noteId, string title, string content, string schedulingId, DateTimeOffset dateTime)
            {
                SimpleNote note = (SimpleNote)GetNoteById(noteId);
                long notificationId;
                if (note.ScheduledNotification != null)
                {
                    Debug.WriteLine("Updating notification with id : " + note.ScheduledNotification.DataBaseId);
                    NotificationHelper.UpdateNotification(note.ScheduledNotification.DataBaseId, schedulingId, dateTime);
                    notificationId = note.ScheduledNotification.DataBaseId;
                }
                else
                {
                    notificationId = NotificationHelper.AddNotification(schedulingId, dateTime);
                }
                UpdateNote(noteId, title, content, notificationId);
            }
        }

        public class TodoNoteDataHelper : INoteManager
        {
            #region Query strings
            private const string SELECT_TODO_NOTE_CONTENT = "SELECT TODONOTES.Id, TODONOTES.Title, TODONOTESCONTENT.Id, TODONOTESCONTENT.Content, TODONOTESCONTENT.Is_Done " +
            "FROM TODONOTES INNER JOIN TODONOTESCONTENT ON TODONOTES.ID = TODONOTESCONTENT.NOTE_ID WHERE TODONOTES.ID = ?;";
            private const string SELECT_ALL_TODO_NOTES = "SELECT * FROM TODONOTES;";
            private const string SELECT_NOTIFICATION_BY_NOTE_ID = "SELECT NOTIFICATIONS.ID, NOTIFICATIONS.NOTIFICATION_ID, NOTIFICATIONS.DATE_TIME " +
                                                       "FROM NOTIFICATIONS INNER JOIN TODONOTES ON NOTIFICATIONS.ID = TODONOTES.NOTIFICATION_ID WHERE TODONOTES.ID = ?;";
            private const string SELECT_LAST_NOTE_INSERT_ID = "SELECT ID FROM TODONOTES ORDER BY datetime(Last_Modified) DESC LIMIT 1;";
            private const string INSERT_TODO_NOTE = "INSERT INTO TODONOTES(Title, Last_Modified, NOTIFICATION_ID) VALUES(?, ?, ?);";
            private const string INSERT_TODO_NOTE_CONTENT = "INSERT INTO TODONOTESCONTENT(Content, Is_Done, Note_Id, Last_Modified) VALUES(?, ?, ?, ?);";
            private const string UPDATE_TODO_NOTE = "UPDATE TODONOTES SET TITLE = ?, LAST_MODIFIED = ?, NOTIFICATION_ID = ? WHERE ID = ?;";
            private const string UPDATE_TODO_NOTE_CONTENT = "UPDATE TODONOTESCONTENT SET CONTENT = ?, IS_DONE = ?, LAST_MODIFIED = ? WHERE ID = ?;";
            private const string DELETE_TODO_NOTE = "DELETE FROM TODONOTES WHERE ID = ?;";
            private const string DELETE_TODO_NOTE_CONTENT = "DELETE FROM TODONOTESCONTENT WHERE ID = ?;"; 
            #endregion

            public ObservableCollection<BaseNote> GetAllNotes()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_TODO_NOTES);
                ObservableCollection<BaseNote> result = new ObservableCollection<BaseNote>();
                while (statement.Step() == SQLiteResult.ROW)
                {
                    result.Add(new BaseNote((long)statement[0], (string)statement[1], null));
                }
                return result;
            }

            public ScheduledNotification GetNotificationByNoteId(long noteId)
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_NOTIFICATION_BY_NOTE_ID);
                statement.Bind(1, noteId);
                ScheduledNotification notification = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    notification = new ScheduledNotification((long)statement[0], (string)statement[1], DateTimeOffset.Parse((string)statement[2]));
                }
                return notification;
            }

            public void DeleteNote(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                TodoNote todoNote = (TodoNote)GetNoteById(id);
                foreach (TodoNote.TodoEntry entry in todoNote.TodoEntries)
                {
                    statement.Bind(1, entry.Id);
                    statement.Step();
                    statement.Reset();
                    statement.ClearBindings();
                }
                statement = dbConn.Prepare(DELETE_TODO_NOTE);
                statement.Bind(1, id);
                statement.Step();
                DeleteNotificationByNoteId(id);
            }

            public void DeleteNotificationByNoteId(long noteId)
            {
                ScheduledNotification notification = GetNotificationByNoteId(noteId);
                if (notification != null)
                {
                    Debug.WriteLine("Deleting notification from database");
                    NotificationHelper.DeleteNotificationById(notification.DataBaseId);
                }
            }

            public void DeleteEntry(long entryId)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                statement.Bind(1, entryId);
                statement.Step();
            }

            public void AddNote(string title, ObservableCollection<TodoNote.TodoEntry> entries)
            {
                ISQLiteStatement statement = dbConn.Prepare(INSERT_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, TimeUtil.GetStringTimestamp());
                statement.Bind(3, null);
                statement.Step();
                foreach (TodoNote.TodoEntry entry in entries)
                {
                    statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Content);
                    int done = ConvertBoolToInt(entry.IsDone);
                    statement.Bind(2, done);
                    ISQLiteStatement stat = dbConn.Prepare(SELECT_LAST_NOTE_INSERT_ID);
                    stat.Step();
                    long noteID = (long)stat[0];
                    statement.Bind(3, noteID);
                    statement.Bind(4, TimeUtil.GetStringTimestamp());
                    statement.Step();
                    statement.Reset();
                    statement.ClearBindings();
                }
            }

            public void AddNote(string title, ObservableCollection<TodoNote.TodoEntry> entries, string schedulingId, DateTimeOffset dateTime)
            {
                long notificationId = NotificationHelper.AddNotification(schedulingId, dateTime);
                ISQLiteStatement statement = dbConn.Prepare(INSERT_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, TimeUtil.GetStringTimestamp());
                statement.Bind(3, notificationId);
                statement.Step();
                foreach (TodoNote.TodoEntry entry in entries)
                {
                    statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Content);
                    int done = ConvertBoolToInt(entry.IsDone);
                    statement.Bind(2, done);
                    ISQLiteStatement stat = dbConn.Prepare(SELECT_LAST_NOTE_INSERT_ID);
                    stat.Step();
                    long noteID = (long)stat[0];
                    statement.Bind(3, noteID);
                    statement.Bind(4, TimeUtil.GetStringTimestamp());
                    statement.Step();
                    statement.Reset();
                    statement.ClearBindings();
                }
            }

            public void UpdateNoteAndDeleteNotification(long id, string title, ObservableCollection<TodoNote.TodoEntry> TodoEntries)
            {
                DeleteNotificationByNoteId(id);
                UpdateNote(id, title, TodoEntries);
                //string timeStamp = TimeUtil.GetStringTimestamp();
                //ISQLiteStatement statement = dbConn.Prepare(UPDATE_TODO_NOTE);
                //statement.Bind(1, title);
                //statement.Bind(2, timeStamp);
                //statement.Bind(3, null);
                //statement.Bind(4, id);
                //statement.Step();
                //TodoNote todoNote = (TodoNote)GetNoteById(id);
                //// Delete all todo note contents
                //foreach (TodoNote.TodoEntry entry in ((TodoNote)GetNoteById(id)).TodoEntries)
                //{
                //    statement.Reset();
                //    statement.ClearBindings();
                //    statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                //    statement.Bind(1, entry.ID);
                //    statement.Step();
                //}
                //// Add all todo note new contents 
                //foreach (TodoNote.TodoEntry entry in TodoEntries)
                //{
                //    statement.Reset();
                //    statement.ClearBindings();
                //    statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                //    statement.Bind(1, entry.Content);
                //    statement.Bind(2, ConvertBoolToInt(entry.IsDone));
                //    statement.Bind(3, id);
                //    statement.Bind(4, timeStamp);
                //    statement.Step();
                //}
            }

            public void UpdateNoteAndNotification(long noteId, string title, ObservableCollection<TodoNote.TodoEntry> TodoEntries, string schedulingId, DateTimeOffset dateTime)
            {
                TodoNote note = (TodoNote)GetNoteById(noteId);
                long notificationId;
                if (note.ScheduledNotification != null)
                {
                    Debug.WriteLine("Updating notification with id : " + note.ScheduledNotification.DataBaseId);
                    NotificationHelper.UpdateNotification(note.ScheduledNotification.DataBaseId, schedulingId, dateTime);
                    notificationId = note.ScheduledNotification.DataBaseId;
                }
                else
                {
                    notificationId = NotificationHelper.AddNotification(schedulingId, dateTime);
                }
                UpdateNote(noteId, title, TodoEntries, notificationId);
            }

            private void UpdateNote(long noteId, string title, ObservableCollection<TodoNote.TodoEntry> TodoEntries, long notificaitnId)
            {
                string timeStamp = TimeUtil.GetStringTimestamp();
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, timeStamp);
                statement.Bind(3, notificaitnId);
                statement.Bind(4, noteId);
                statement.Step();
                TodoNote todoNote = (TodoNote)GetNoteById(noteId);
                // Delete all todo note contents
                foreach (TodoNote.TodoEntry entry in ((TodoNote)GetNoteById(noteId)).TodoEntries)
                {
                    statement.Reset();
                    statement.ClearBindings();
                    statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Id);
                    statement.Step();
                }
                // Add all todo note new contents 
                foreach (TodoNote.TodoEntry entry in TodoEntries)
                {
                    statement.Reset();
                    statement.ClearBindings();
                    statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Content);
                    statement.Bind(2, ConvertBoolToInt(entry.IsDone));
                    statement.Bind(3, noteId);
                    statement.Bind(4, timeStamp);
                    statement.Step();
                }
            }

            private void UpdateNote(long noteId, string title, ObservableCollection<TodoNote.TodoEntry> TodoEntries)
            {
                string timeStamp = TimeUtil.GetStringTimestamp();
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, timeStamp);
                statement.Bind(3, null);
                statement.Bind(4, noteId);
                statement.Step();
                TodoNote todoNote = (TodoNote)GetNoteById(noteId);
                // Delete all todo note contents
                foreach (TodoNote.TodoEntry entry in ((TodoNote)GetNoteById(noteId)).TodoEntries)
                {
                    statement.Reset();
                    statement.ClearBindings();
                    statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Id);
                    statement.Step();
                }
                // Add all todo note new contents 
                foreach (TodoNote.TodoEntry entry in TodoEntries)
                {
                    statement.Reset();
                    statement.ClearBindings();
                    statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Content);
                    statement.Bind(2, ConvertBoolToInt(entry.IsDone));
                    statement.Bind(3, noteId);
                    statement.Bind(4, timeStamp);
                    statement.Step();
                }
            }

            public BaseNote GetNoteById(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_TODO_NOTE_CONTENT);
                statement.Bind(1, id);
                string title = "";
                long noteId = 0;
                ObservableCollection<TodoNote.TodoEntry> entries = new ObservableCollection<TodoNote.TodoEntry>();
                while (statement.Step() == SQLiteResult.ROW)
                {
                    noteId = (long)statement[0];
                    title = (string)statement[1];
                    bool isDone = (long)statement[4] == 0 ? false : true;
                    entries.Add(new TodoNote.TodoEntry((long)statement[2], (string)statement[3], isDone));
                }
                return new TodoNote(noteId, title, GetNotificationByNoteId(noteId), entries);
            }

            private int ConvertBoolToInt(bool boolean)
            {
                return boolean == true ? 1 : 0;
            }
        }

        public class PhotoNoteHelper : INoteManager
        {
            #region Query strings
            private const string SELECT_ALL_PHOTO_NOTES = "SELECT * FROM PHOTONOTES;";
            private const string SELECT_PHOTO_NOTE_BY_ID = "SELECT * FROM PHOTONOTES WHERE ID = ?;";
            private const string INSERT_PHOTO_NOTE = "INSERT INTO PHOTONOTES(Title, Content, PHOTO_ADRESS, Last_Modified, NOTIFICATION_ID) VALUES(?, ?, ?, ?, ?);";
            private const string DELETE_PHOTO_NOTE = "DELETE FROM PHOTONOTES WHERE ID = ?;";
            private const string SELECT_PHOTO_NOTE_NOTIFICATION_BY_NOTE_ID = "SELECT NOTIFICATIONS.ID, NOTIFICATIONS.NOTIFICATION_ID, NOTIFICATIONS.DATE_TIME " +
                                                      "FROM NOTIFICATIONS INNER JOIN PHOTONOTES ON NOTIFICATIONS.ID = PHOTONOTES.NOTIFICATION_ID WHERE PHOTONOTES.ID = ?;";
            private const string UPDATE_PHOTO_NOTE = "UPDATE PHOTONOTES SET TITLE = ?, CONTENT = ?, PHOTO_ADRESS = ?, LAST_MODIFIED = ?, NOTIFICATION_ID = ? WHERE ID = ?;";
            #endregion

            public void AddNote(string title, string content, string photoPath)
            {
                ISQLiteStatement statement = dbConn.Prepare(INSERT_PHOTO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, photoPath);
                statement.Bind(4, TimeUtil.GetStringTimestamp());
                statement.Bind(5, null);
                statement.Step();
            }

            public void AddNote(string title, string content, string photoPath, string schedulingId, DateTimeOffset dateTime)
            {
                long notificationId = NotificationHelper.AddNotification(schedulingId, dateTime);
                ISQLiteStatement statement = dbConn.Prepare(INSERT_PHOTO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, photoPath);
                statement.Bind(4, TimeUtil.GetStringTimestamp());
                statement.Bind(5, notificationId);
                statement.Step();
            }

            private void UpdateNote(long id, string title, string content, string photoPath)
            {
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_PHOTO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, photoPath);
                statement.Bind(4, TimeUtil.GetStringTimestamp());
                statement.Bind(5, null);
                statement.Bind(6, id);
                statement.Step();
            }

            private void UpdateNote(long id, string title, string content, string photoPath, long notificationId)
            {
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_PHOTO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, photoPath);
                statement.Bind(4, TimeUtil.GetStringTimestamp());
                statement.Bind(5, notificationId);
                statement.Bind(6, id);
                statement.Step();
            }

            public void UpdateNoteAndNotification(long noteId, string title, string content, string photoPath, string schedulingId, DateTimeOffset dateTime)
            {
                PhotoNote note = (PhotoNote)GetNoteById(noteId);
                long notificationId;
                if (note.ScheduledNotification != null)
                {
                    Debug.WriteLine("Updating notification with id : " + note.ScheduledNotification.DataBaseId);
                    NotificationHelper.UpdateNotification(note.ScheduledNotification.DataBaseId, schedulingId, dateTime);
                    notificationId = note.ScheduledNotification.DataBaseId;
                }
                else
                {
                    notificationId = NotificationHelper.AddNotification(schedulingId, dateTime);
                }
                UpdateNote(noteId, title, content, photoPath, notificationId);
            }

            public void UpdateNoteAndDeleteNotification(long noteId, string title, string content, string photoPath)
            {
                DeleteNotificationByNoteId(noteId);
                UpdateNote(noteId, title, content, photoPath);
            }

            public ObservableCollection<BaseNote> GetAllNotes()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_PHOTO_NOTES);
                ObservableCollection<BaseNote> result = new ObservableCollection<BaseNote>();
                while (statement.Step() == SQLiteResult.ROW)
                {
                    result.Add(new BaseNote((long)statement[0], (string)statement[1], null));
                }
                return result;
            }

            public BaseNote GetNoteById(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_PHOTO_NOTE_BY_ID);
                statement.Bind(1, id);
                PhotoNote note = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    note = new PhotoNote((long)statement[0], (String)statement[1], null, (String)statement[2], (String)statement[3]);
                    ScheduledNotification notification;
                    if (statement[5] != null)
                    {
                        long notificationId = (long)statement[5];
                        notification = NotificationHelper.GetNotificationById(notificationId);
                        note.ScheduledNotification = notification;
                    }
                }
                return note;
            }

            public ScheduledNotification GetNotificationByNoteId(long noteId)
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_PHOTO_NOTE_NOTIFICATION_BY_NOTE_ID);
                statement.Bind(1, noteId);
                ScheduledNotification notification = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    notification = new ScheduledNotification((long)statement[0], (string)statement[1], DateTimeOffset.Parse((string)statement[2]));
                }
                return notification;
            }

            public void DeleteNote(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_PHOTO_NOTE);
                statement.Bind(1, id);
                statement.Step();
                statement.Reset();
                statement.ClearBindings();
                DeleteNotificationByNoteId(id);
            }

            public void DeleteNotificationByNoteId(long noteId)
            {
                ScheduledNotification notification = GetNotificationByNoteId(noteId);
                if (notification != null)
                {
                    NotificationHelper.DeleteNotificationById(notification.DataBaseId);
                }
            }

        }
    }
}