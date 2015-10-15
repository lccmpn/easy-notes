using EasyNotes.Data.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

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

        public bool IsNotificationDateVisible
        {
            get
            {
                return TodoNote.ScheduledNotification != null;
            }
        }

        public long Id
        {
            get
            {
                return this.TodoNote.Id;
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

        public DateTimeOffset NotificationDate
        {
            get
            {
                if (TodoNote.ScheduledNotification == null)
                {
                    this.TodoNote.ScheduledNotification = new ScheduledNotification();
                }
                return this.TodoNote.ScheduledNotification.Date;
            }
            set
            {
                if (this.TodoNote.ScheduledNotification == null)
                {
                    this.TodoNote.ScheduledNotification = new ScheduledNotification();
                }
                this.TodoNote.ScheduledNotification.Date = value;
                OnPropertyChanged("NotificationDate");
            }
        }

        public TimeSpan NotificationTime
        {
            get
            {
                if (this.TodoNote.ScheduledNotification == null)
                {
                    this.TodoNote.ScheduledNotification = new ScheduledNotification();
                }
                return this.TodoNote.ScheduledNotification.Time;
            }
            set
            {

                if (this.TodoNote.ScheduledNotification == null)
                {
                    this.TodoNote.ScheduledNotification = new ScheduledNotification();
                }
                this.TodoNote.ScheduledNotification.Time = value;
                OnPropertyChanged("NotificationTime");
            }
        }

        public string NotificationID
        {
            get
            {
                return TodoNote.ScheduledNotification.SchedulingId;
            }
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
