using EasyNotes.Data.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Database
{
    interface INoteManager
    {
        //void AddNote(BaseNote note);
        ObservableCollection<BaseNote> GetAllNotes();
        BaseNote GetNoteById(long id);
        void DeleteNote(long id);
    }
}
