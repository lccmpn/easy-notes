using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using EasyNotes.Data.Model;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.ViewModel
{
    public class AddTodoNoteViewModel : BaseViewModel
    {
        public TodoNote TodoNote { get; set; }

        public AddTodoNoteViewModel()
        {
            ObservableCollection<TodoNote.ToDoEntry> entries = new ObservableCollection<TodoNote.ToDoEntry>();
            TodoNote = new TodoNote();
        }

        public AddTodoNoteViewModel(TodoNote todoNote)
        {
            ObservableCollection<TodoNote.ToDoEntry> entries = new ObservableCollection<TodoNote.ToDoEntry>();
            TodoNote = todoNote;
        }

    }
}
