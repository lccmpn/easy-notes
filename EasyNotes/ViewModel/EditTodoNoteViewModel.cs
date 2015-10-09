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
    public class EditTodoNoteViewModel : BaseViewModel
    {
        private TodoNote TodoNote { get; set; }

        public EditTodoNoteViewModel(TodoNote todoNote)
        {
            this.TodoNote = todoNote;
            //foreach(TodoNote.TodoEntry entry in TodoNote.TodoEntries){
            //    this.todoEntries.Add(new TodoEntry(entry.ID, entry.Content, entry.IsDone));
            //}
        }
        
        public EditTodoNoteViewModel()
        {
            this.TodoNote = new TodoNote();
        }

        public long Id
        {
            get
            {
                return this.TodoNote.ID;
            }
        }

        public string Title
        {
            get
            {
                return TodoNote.Title;
            }
            set
            {
                TodoNote.Title = value;
                OnPropertyChanged("Title");
            }
        }

        public void AddEntry(string content, bool isDone)
        {
            this.TodoNote.TodoEntries.Add(new TodoNote.TodoEntry(content, isDone));
        }

        public ObservableCollection<TodoNote.TodoEntry> TodoEntries
        {
            get
            {
                return this.TodoNote.TodoEntries;
            }
            set
            {
                this.TodoNote.TodoEntries = value;
                OnPropertyChanged("TodoEntries");
            }
        }
       
    }
}
