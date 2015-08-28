using System;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using SQLitePCL;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace EasyNotes.DataModel
{
    class DataManager
    {
        static private SQLiteConnection dbConn;

        private const string CREATE_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS SIMPLENOTES (" +
               "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
               "Title VARCHAR(100)," +
               "Content VARCHAR(3000)" +
               ");";

        private const string SELECT_ALL_SIMPLE_NOTES = "SELECT * FROM SIMPLENOTES;";

        private const string SELECT_SIMPLE_NOTE_BY_ID = "SELECT * FROM SIMPLENOTES WHERE ID = ?;";

        private const string INSERT_SIMPLE_NOTE = "INSERT INTO SIMPLENOTES(Title, Content) VALUES(?, ?)";

        public static void CreateDatabase()
        {
            dbConn = new SQLiteConnection("EsayNotesData.db");
            CreateTables();
        }

        private static void CreateTables()
        {
            dbConn.Prepare(CREATE_NOTES_TABLE).Step();
        }

        public static void AddSimpleNote(string title, string content)
        {
            ISQLiteStatement statement = dbConn.Prepare(INSERT_SIMPLE_NOTE);
            statement.Bind(1, title);
            statement.Bind(2, content);
            statement.Step();
        }

        public static ObservableCollection<AbstractNote> GetAllNotes()
        {
            ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_SIMPLE_NOTES);
            ObservableCollection<AbstractNote> result = new ObservableCollection<AbstractNote>();
            while (statement.Step() == SQLiteResult.ROW)
            {
                result.Add(new SimpleNote((long)statement[0], (String)statement[1], (String)statement[2]));
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
                Debug.WriteLine("SimpleNote: " + note.ToString());
            }
            return note;
        }
    }
}
