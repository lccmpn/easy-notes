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

        public const string CREATE_NOTES_TABLE = "CREATE TABLE IF NOT EXISTS SIMPLENOTES (" +
               "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
               "Title VARCHAR(100)," +
               "Content VARCHAR(500)" +
               ");";

        public const string SELECT_ALL_NOTES = "SELECT * FROM SIMPLENOTES;";

        public const string SELECT_NOTE_BY_ID = "SELECT * FROM SIMPLENOTES WHERE ID = ?;";

        public static void CreateDatabase()
        {
            dbConn = new SQLiteConnection("EsayNotesData.db");
            CreateTables();
            ISQLiteStatement statement = dbConn.Prepare("INSERT INTO SIMPLENOTES(Title, Content) VALUES(?, ?)");
            statement.Bind(1, "Cose da fare");
            statement.Bind(2, "Devo fare molte cose");
            statement.Step();
            statement.Reset();
            statement.Bind(1, "Impegno");
            statement.Bind(2, "Impegnarsi di più");
            statement.Step();
        }

        private static void CreateTables()
        {
            dbConn.Prepare(CREATE_NOTES_TABLE).Step();
        }

        public static void InsertNote(AbstractNote note){

        }

        public static ObservableCollection<AbstractNote> GetAllNotes()
        {   
            ISQLiteStatement statement = dbConn.Prepare(SELECT_ALL_NOTES);
            ObservableCollection<AbstractNote> result = new ObservableCollection<AbstractNote>();
            while (statement.Step() == SQLiteResult.ROW)
            {
                result.Add(new SimpleNote((long)statement[0], (String)statement[1], (String)statement[2]));
            }
            return result;
        }

        public static AbstractNote GetNoteById(long id)
        {
            ISQLiteStatement statement = dbConn.Prepare(SELECT_NOTE_BY_ID);
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
