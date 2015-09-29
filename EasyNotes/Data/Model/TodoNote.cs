using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EasyNotes.Data.Model
{
    public class TodoNote : BaseNote
    {
        private ObservableCollection<ToDoEntry> todoEntries = new ObservableCollection<ToDoEntry>();
        public ObservableCollection<ToDoEntry> TodoEntries
        {
            get
            {
                return this.todoEntries;
            }
            set
            {
                this.todoEntries = value;
                base.OnPropertyChanged("TodoEntries");
            }
        }

        public TodoNote(long id, string title, ObservableCollection<ToDoEntry> toDoEntries)
            : base(id, title)
        {
            this.todoEntries = toDoEntries;
        }

        public TodoNote() : base()
        {
            
        }
        
        public override string ToString()
        {
            string entries = "";
            foreach(ToDoEntry entry in todoEntries){
                entries = entries + entry.ToString();
            }
            return base.ToString() + " [Content]: " + todoEntries.ToString();
        }

        public void AddEntry(string content, bool isDone){
            TodoEntries.Add(new ToDoEntry(content, isDone));
        }

        public class ToDoEntry : BaseModel
        {
            private string content;
            private bool isDone;

            public string Content
            {
                get
                {
                    return this.content;
                }
                set
                {
                    this.content = value;
                    OnPropertyChanged("Content");
                }
            }
            public bool IsDone { 
                get
                {
                    return this.isDone;
                }
                set
                {
                    this.isDone = value;
                    OnPropertyChanged("IsDone");
                }
            }

            public ToDoEntry(string content, bool isDone)
            {
                this.Content = content;
                this.IsDone = isDone;
            } 

            public ToDoEntry(long id, string content, bool isDone)
                : base(id)
            {
                this.ID = id;
                this.Content = content;
                this.IsDone = isDone;
            } 
            
            public override string ToString()
            {
                return " [Content]: " + this.Content + " [IsDone]: " + this.IsDone;
            }
        }
    }
}
