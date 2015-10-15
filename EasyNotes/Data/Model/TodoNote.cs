using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace EasyNotes.Data.Model
{
    public class TodoNote : BaseNote
    {
        private ObservableCollection<TodoEntry> todoEntries = new ObservableCollection<TodoEntry>();
        public ObservableCollection<TodoEntry> TodoEntries
        {
            get
            {
                return this.todoEntries;
            }
            set
            {
                this.todoEntries = value;
            }
        }

        public TodoNote(long id, string title, ScheduledNotification notificationDateTime, ObservableCollection<TodoEntry> toDoEntries)
            : base(id, title, notificationDateTime)
        {
            this.todoEntries = toDoEntries;
        }

        public TodoNote() : base()
        {
            
        }
        
        public override string ToString()
        {
            string entries = "";
            foreach(TodoEntry entry in todoEntries){
                entries = entries + entry.ToString();
            }
            return base.ToString() + " [Content]: " + todoEntries.ToString();
        }

        public void AddEntry(string content, bool isDone){
            TodoEntries.Add(new TodoEntry(content, isDone));
        }

        public class TodoEntry : BaseModel
        {
            private string content;
            private bool isDone;

            public TodoEntry(long id, string content, bool isDone)
                : base(id)
            {
                this.Id = id;
                this.Content = content;
                this.IsDone = isDone;
            }

            public TodoEntry(string content, bool isDone)
            {
                this.Content = content;
                this.IsDone = isDone;
            }

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
                    OnPropertyChanged("Brush");
                }
            }

            public SolidColorBrush Brush
            {
                get
                {
                    if (isDone)
                    {
                        return new SolidColorBrush(Colors.Gray);
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.White);
                    }
                }
            }
            
            public override string ToString()
            {
                return " [Content]: " + this.Content + " [IsDone]: " + this.IsDone;
            }
        }
    }
}
