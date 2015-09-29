using EasyNotes.Data.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.ViewModel
{
    public class TodoNoteDetailViewModel : BaseViewModel
    {
        public TodoNote TodoNote { get; set; }

        public TodoNoteDetailViewModel(TodoNote todoNote)
        {
            this.TodoNote = todoNote;
        }
    }
}
