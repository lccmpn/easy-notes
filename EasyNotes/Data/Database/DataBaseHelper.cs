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

        private const string CREATE_SIMPLE_NOTES_NOTIFICATION_TABLE = "CREATE TABLE IF NOT EXISTS SIMPLENOTESNOTIFICATIONS (" +
                "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "DATE_TIME VARCHAR(23)," +
                "NOTIFICATION_ID VARCHAR(16)," +
                "LAST_MODIFIED VARCHAR(23)" +
                ");";

        private const string CREATE_TODO_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS TODONOTES (" +
               "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
               "TITLE VARCHAR(100)," +
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
        #endregion

        public static void CreateDatabase()
        {
            dbConn = new SQLiteConnection("EasyNotesData.db");
            CreateTables();
        }

        private static void CreateTables()
        {
            dbConn.Prepare(CREATE_SIMPLE_NOTES_TABLE).Step();
            dbConn.Prepare(CREATE_SIMPLE_NOTES_NOTIFICATION_TABLE).Step();
            dbConn.Prepare(CREATE_TODO_NOTES_TABLE).Step();
            dbConn.Prepare(CREATE_TODO_NOTES_CONTENT_TABLE).Step();
        }

        public class SimpleNoteHelper : INoteManager
        {
            private const string SELECT_ALL_SIMPLE_NOTES = "SELECT * FROM SIMPLENOTES;";
            private const string SELECT_SIMPLE_NOTE_BY_ID = "SELECT * FROM SIMPLENOTES WHERE ID = ?;";
            private const string SELECT_LAST_NOTE_INSERT_ID = "SELECT ID FROM SIMPLENOTES ORDER BY datetime(Last_Modified) DESC LIMIT 1;";
            private const string SELECT_LAST_NOTIFICATION_INSERT_ID = "SELECT ID FROM SIMPLENOTESNOTIFICATIONS ORDER BY datetime(LAST_MODIFIED) DESC LIMIT 1;";
            private const string SELECT_SIMPLE_NOTE_NOTIFICATION = "SELECT ID, NOTIFICATION_ID, DATE_TIME " +
                                                                   "FROM SIMPLENOTESNOTIFICATIONS WHERE ID = ?;";
            private const string SELECT_SIMPLE_NOTE_NOTIFICATION_BY_NOTE_ID = "SELECT SIMPLENOTESNOTIFICATIONS.ID, SIMPLENOTESNOTIFICATIONS.NOTIFICATION_ID, SIMPLENOTESNOTIFICATIONS.DATE_TIME " +
                                                       "FROM SIMPLENOTESNOTIFICATIONS INNER JOIN SIMPLENOTES ON SIMPLENOTESNOTIFICATIONS.ID = SIMPLENOTES.NOTIFICATION_ID WHERE SIMPLENOTES.ID = ?;";

            private const string INSERT_SIMPLE_NOTE = "INSERT INTO SIMPLENOTES(Title, Content, Last_Modified, NOTIFICATION_ID) VALUES(?, ?, ?, ?);";
            private const string INSERT_SIMPLE_NOTE_NOTIFICATION = "INSERT INTO SIMPLENOTESNOTIFICATIONS(DATE_TIME, NOTIFICATION_ID, LAST_MODIFIED) VALUES(?, ?, ?);";

            private const string DELETE_SIMPLE_NOTE = "DELETE FROM SIMPLENOTES WHERE ID = ?;";
            private const string DELETE_SIMPLE_NOTE_NOTIFICATION = "DELETE FROM SIMPLENOTESNOTIFICATIONS WHERE ID = ?;";
            private const string DELETE_SIMPLE_NOTE_NOTIFICATION_WITH_NOTE_ID = "DELETE FROM SIMPLENOTESNOTIFICATIONS WHERE NOTE_ID = ?;";

            private const string UPDATE_SIMPLE_NOTE = "UPDATE SIMPLENOTES SET TITLE = ?, CONTENT = ?, LAST_MODIFIED = ?, NOTIFICATION_ID = ? WHERE ID = ?;";
            private const string UPDATE_NOTIFICATION = "UPDATE SIMPLENOTESNOTIFICATIONS SET DATE_TIME = ?, NOTIFICATION_ID = ?, LAST_MODIFIED = ? WHERE ID = ?;";


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
                //ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE_NOTIFICATION);
                //statement.Bind(1, TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                //statement.Bind(2, notificationId);
                //statement.Bind(3, TimeUtil.GetStringTimestamp());
                //statement.Step();
                //statement.Reset();
                //statement.ClearBindings();
                //statement = dbConn.Prepare(SELECT_LAST_NOTIFICATION_INSERT_ID);
                //statement.Step();
                //long notificationID = (long)statement[0];
                //statement.Reset();
                //statement.ClearBindings();
                long notificationId =  AddNotification(schedulingId, dateTime);
                ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, notificationId);
                statement.Step();
                //statement.Reset();
                //statement.ClearBindings();
                //statement = dbConn.Prepare(SELECT_LAST_NOTE_INSERT_ID);
                //statement.Step();
                //long noteID = (long)statement[0];
                //statement.Reset();
                //statement.ClearBindings();

            }

            public long AddNotification(string schedulingId, DateTimeOffset dateTime)
            {
                ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE_NOTIFICATION);
                statement.Bind(1, TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                statement.Bind(2, schedulingId);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                Debug.WriteLine(statement.Step());
                Debug.WriteLine("Last inserted notifcation id: " + GetLastInsertedNotificationId());
                return GetLastInsertedNotificationId();
            }

            private long GetLastInsertedNotificationId()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_LAST_NOTIFICATION_INSERT_ID);
                statement.Step();
                return (long)statement[0];
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
                        notification = GetNotificationById((long)statement[4]);
                        note.ScheduledNotification = notification;
                    }
                }
                return note;
            }

            private ScheduledNotification GetNotificationById(long notificationId)
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

            public void DeleteNotificationByNoteId(string id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_SIMPLE_NOTE_NOTIFICATION);
                statement.Bind(1, id);
                statement.Step();
            }

            public void DeleteNotificationByNoteId(long noteId)
            {
                ScheduledNotification notification = GetNotificationByNoteId(noteId);
                if (notification != null)
                {
                    Debug.WriteLine("Deleting notification from database");
                    ISQLiteStatement statment = dbConn.Prepare(DELETE_SIMPLE_NOTE_NOTIFICATION);
                    statment.Bind(1, notification.DataBaseId);
                    statment.Step();
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

            private void UpdateNotification(long id, string schedulingId, DateTimeOffset dateTime)
            {
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_NOTIFICATION);
                statement.Bind(1, TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                Debug.WriteLine("Updating notification: data to string " + TimeUtil.ConvertDateTimeOffsetToString(dateTime));
                statement.Bind(2, schedulingId);
                statement.Bind(3, TimeUtil.GetStringTimestamp());
                statement.Bind(4, id);
                statement.Step();
            }

            public void UpdateNoteAndNotification(long noteId, string title, string content, string schedulingId, DateTimeOffset dateTime)
            {
                SimpleNote note = (SimpleNote) GetNoteById(noteId);
                long notificationId;
                if (note.ScheduledNotification != null)
                {
                    Debug.WriteLine("Updating notification with id : " + note.ScheduledNotification.DataBaseId);
                    UpdateNotification(note.ScheduledNotification.DataBaseId, schedulingId, dateTime);
                    notificationId = note.ScheduledNotification.DataBaseId;
                }
                else
                {
                    notificationId = AddNotification(schedulingId, dateTime);
                }
                UpdateNote(noteId, title, content, notificationId);
            }



        }

        public class TodoNoteDataHelper : INoteManager
        {
            private const string SELECT_TODO_NOTE_CONTENT = "SELECT TODONOTES.Id, TODONOTES.Title, TODONOTESCONTENT.Id, TODONOTESCONTENT.Content, TODONOTESCONTENT.Is_Done " +
                    "FROM TODONOTES INNER JOIN TODONOTESCONTENT ON TODONOTES.ID = TODONOTESCONTENT.NOTE_ID WHERE TODONOTES.ID = ?;";
            private const string SELECT_ALL_TODO_NOTES = "SELECT * FROM TODONOTES;";
            private const string SELECT_LAST_NOTE_INSERT_ID = "SELECT ID FROM TODONOTES ORDER BY datetime(Last_Modified) DESC LIMIT 1;";
            private const string INSERT_TODO_NOTE = "INSERT INTO TODONOTES(Title, Last_Modified) VALUES(?, ?);";
            private const string INSERT_TODO_NOTE_CONTENT = "INSERT INTO TODONOTESCONTENT(Content, Is_Done, Note_Id, Last_Modified) VALUES(?, ?, ?, ?);";
            private const string UPDATE_TODO_NOTE = "UPDATE TODONOTES SET TITLE = ?, LAST_MODIFIED = ? WHERE ID = ?;";
            private const string UPDATE_TODO_NOTE_CONTENT = "UPDATE TODONOTESCONTENT SET CONTENT = ?, IS_DONE = ?, LAST_MODIFIED = ? WHERE ID = ?;";
            private const string DELETE_TODO_NOTE = "DELETE FROM TODONOTES WHERE ID = ?;";
            private const string DELETE_TODO_NOTE_CONTENT = "DELETE FROM TODONOTESCONTENT WHERE ID = ?;";

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

            public void DeleteNote(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                TodoNote todoNote = (TodoNote)GetNoteById(id);
                foreach (TodoNote.ToDoEntry entry in todoNote.TodoEntries)
                {
                    statement.Bind(1, entry.ID);
                    statement.Step();
                    statement.Reset();
                    statement.ClearBindings();
                }
                statement = dbConn.Prepare(DELETE_TODO_NOTE);
                statement.Bind(1, id);
                statement.Step();
            }

            public void AddNote(string title, ObservableCollection<TodoNote.ToDoEntry> entries){
                ISQLiteStatement statement = dbConn.Prepare(INSERT_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, TimeUtil.GetStringTimestamp());
                statement.Step();
                
                foreach(TodoNote.ToDoEntry entry in entries){
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

            public void UpdateNote(long id, string title, ObservableCollection<TodoNote.ToDoEntry> TodoEntries)
            {
                string timeStamp = TimeUtil.GetStringTimestamp();
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, timeStamp);
                statement.Bind(3, id);
                statement.Step();
                TodoNote todoNote = (TodoNote)GetNoteById(id);
                // Delete all todo note contents
                foreach (TodoNote.ToDoEntry entry in ((TodoNote)GetNoteById(id)).TodoEntries)
                {
                    statement.Reset();
                    statement.ClearBindings();
                    statement = dbConn.Prepare(DELETE_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.ID);
                    statement.Step();
                }
                // Add all todo note new contents 
                foreach (TodoNote.ToDoEntry entry in TodoEntries){
                    statement.Reset();
                    statement.ClearBindings();
                    statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                    statement.Bind(1, entry.Content);
                    statement.Bind(2, ConvertBoolToInt(entry.IsDone));
                    statement.Bind(3, id);
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
                ObservableCollection<TodoNote.ToDoEntry> entries = new ObservableCollection<TodoNote.ToDoEntry>();
                while (statement.Step() == SQLiteResult.ROW)
                {
                    noteId = (long)statement[0];
                    title = (string) statement[1];
                    bool isDone = (long)statement[4] == 0 ? false : true;
                    entries.Add(new TodoNote.ToDoEntry((long)statement[2], (string)statement[3], isDone));
                }
                return new TodoNote(noteId, title, null, entries);
            }

            private int ConvertBoolToInt(bool boolean){
                return boolean == true ? 1 : 0;
            }
        }
    }
}