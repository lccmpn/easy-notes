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

namespace EasyNotes.Database
{
    class DataManager
    {
        static private SQLiteConnection dbConn;

        #region Queries for creating tables
        private const string CREATE_SIMPLE_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS SIMPLENOTES (" +
                "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                "TITLE VARCHAR(100)," +
                "CONTENT VARCHAR(3000)," +
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
            dbConn.Prepare(CREATE_TODO_NOTES_TABLE).Step();
            dbConn.Prepare(CREATE_TODO_NOTES_CONTENT_TABLE).Step();
            //ObservableCollection<TodoNote.ToDoEntry> list = new ObservableCollection<TodoNote.ToDoEntry>();
            //list.Add(new TodoNote.ToDoEntry("spesasssss", false));
            //list.Add(new TodoNote.ToDoEntry("affarikjnef okj  djdddd  d d  d d d  d dkjnsj d ddd", true));
            //TodoNoteData.AddNote("cose da fare", list);
        }

        public class SimpleNoteData
        {
            private const string SELECT_ALL_SIMPLE_NOTES = "SELECT * FROM SIMPLENOTES;";
            private const string SELECT_SIMPLE_NOTE_BY_ID = "SELECT * FROM SIMPLENOTES WHERE ID = ?;";
            private const string INSERT_SIMPLE_NOTE = "INSERT INTO SIMPLENOTES(Title, Content, Last_Modified) VALUES(?, ?, ?);";
            private const string DELETE_SIMPLE_NOTE = "DELETE FROM SIMPLENOTES WHERE ID = ?;";
            private const string UPDATE_SIMPLE_NOTE = "UPDATE SIMPLENOTES SET TITLE = ?, CONTENT = ?, LAST_MODIFIED = ? WHERE ID = ?;";

            public static void AddNote(string title, string content)
            {
                ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetTimestamp());
                statement.Step();
            }

            public static ObservableCollection<BaseNote> GetAllNotes()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_SIMPLE_NOTES);
                ObservableCollection<BaseNote> result = new ObservableCollection<BaseNote>();
                while (statement.Step() == SQLiteResult.ROW)
                {
                    result.Add(new BaseNote((long)statement[0], (String)statement[1]));
                }
                return result;
            }

            public static SimpleNote GetNoteById(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_SIMPLE_NOTE_BY_ID);
                statement.Bind(1, id);
                SimpleNote note = null;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    note = new SimpleNote((long)statement[0], (String)statement[1], (String)statement[2]);
                    Debug.WriteLine("SimpleNoteData: " + note.ToString());
                }
                return note;
            }

            public static void DeleteNote(long id)
            {
                ISQLiteStatement statement = dbConn.Prepare(DELETE_SIMPLE_NOTE);
                statement.Bind(1, id);
                statement.Step();
            }

            public static void UpdateNote(long id, string title, string content)
            {
                ISQLiteStatement statement = dbConn.Prepare(UPDATE_SIMPLE_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, content);
                statement.Bind(3, TimeUtil.GetTimestamp());
                statement.Bind(4, id);
                statement.Step();
            }
        }

        public class TodoNoteData
        {
            private const string SELECT_TODO_NOTE_CONTENT = "SELECT TODONOTES.Id, TODONOTES.Title, TODONOTESCONTENT.Id, TODONOTESCONTENT.Content, TODONOTESCONTENT.Is_Done " +
                    "FROM TODONOTES INNER JOIN TODONOTESCONTENT ON TODONOTES.ID = TODONOTESCONTENT.NOTE_ID WHERE TODONOTES.ID = ?;";
            private const string SELECT_ALL_TODO_NOTES = "SELECT * FROM TODONOTES;";
            private const string INSERT_TODO_NOTE = "INSERT INTO TODONOTES(Title, Last_Modified) VALUES(?, ?);";
            private const string SELECT_LAST_NOTE_INSERT_ID = "SELECT ID FROM TODONOTES ORDER BY datetime(Last_Modified) DESC LIMIT 1;";
            private const string INSERT_TODO_NOTE_CONTENT = "INSERT INTO TODONOTESCONTENT(Content, Is_Done, Note_Id, Last_Modified) VALUES(?, ?, ?, ?);";

            public static ObservableCollection<BaseNote> GetAllNotes()
            {
                ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_TODO_NOTES);
                ObservableCollection<BaseNote> result = new ObservableCollection<BaseNote>();
                int i = 0;
                while (statement.Step() == SQLiteResult.ROW)
                {
                    Debug.WriteLine(i++);
                    result.Add(new BaseNote((long)statement[0], (string)statement[1]));
                }
                return result;
            }

            public static void AddNote(string title, ObservableCollection<TodoNote.ToDoEntry> entries){
                ISQLiteStatement statement = dbConn.Prepare(INSERT_TODO_NOTE);
                statement.Bind(1, title);
                statement.Bind(2, TimeUtil.GetTimestamp());
                statement.Step();
                statement = dbConn.Prepare(INSERT_TODO_NOTE_CONTENT);
                foreach(TodoNote.ToDoEntry entry in entries){
                    statement.Bind(1, entry.Content);
                    long done = entry.IsDone == true? 1 : 0;
                    statement.Bind(2, done);
                    ISQLiteStatement stat = dbConn.Prepare(SELECT_LAST_NOTE_INSERT_ID);
                    stat.Step();
                    long noteID = (long)stat[0];
                    statement.Bind(3, noteID);
                    statement.Bind(4, TimeUtil.GetTimestamp());
                    statement.Step();
                    statement.Reset();
                    statement.ClearBindings();
                }
            }

            public static TodoNote GetNoteById(long id)
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
                return new TodoNote(noteId, title, entries);
            }
        }
    }
}
