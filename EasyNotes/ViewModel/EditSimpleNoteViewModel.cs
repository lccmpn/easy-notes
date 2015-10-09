using System;
using EasyNotes.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EasyNotes.ViewModel
{
    public class EditSimpleNoteDetailViewModel : BaseViewModel
    {
        private SimpleNote simpleNote;
        private bool notificationDateVisible;

        public EditSimpleNoteDetailViewModel()
        {
            this.simpleNote = new SimpleNote();
        }

        public EditSimpleNoteDetailViewModel(SimpleNote simpleNote)
        {
            this.simpleNote = simpleNote;
        }

        public long ID
        {
            get
            {
                return this.simpleNote.ID;
            }
        }

        public string Title
        {
            get
            {
                return simpleNote.Title;
            }
            set
            {
                this.simpleNote.Title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Content
        {
            get
            {
                return simpleNote.Content;
            }
            set
            {
                if (simpleNote.Content != value)
                {
                    simpleNote.Content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public bool IsNotificationDateVisible
        {
            get
            {
                return simpleNote.ScheduledNotification != null;
            }
            set
            {
                notificationDateVisible = value;
            }
        }

        //public DateTimeOffset NotificationDate
        //{
        //    get
        //    {
        //        if (this.simpleNote.ScheduledNotification == null)
        //        {
        //            return DateTimeOffset.Now;
        //        }
        //        Debug.WriteLine("Getting date..");
        //        return this.simpleNote.ScheduledNotification.Date;
        //    }
        //    set
        //    {
        //        this.simpleNote.ScheduledNotification.Date = value;
        //        Debug.WriteLine("Date setted to" + this.simpleNote.ScheduledNotification.Date);
        //        if (this.simpleNote.ScheduledNotification == null)
        //        {
        //            this.simpleNote.ScheduledNotification = new ScheduledNotification();
        //        }

        //        OnPropertyChanged("NotificationDate");
        //    }
        //}

        public DateTimeOffset NotificationDate
        {
            get
            {
                if(simpleNote.ScheduledNotification == null)
                {
                    this.simpleNote.ScheduledNotification = new ScheduledNotification();
                }
                return this.simpleNote.ScheduledNotification.Date;
            }
            set
            {
                Debug.WriteLine("Setting date");
                if (this.simpleNote.ScheduledNotification == null)
                {
                    this.simpleNote.ScheduledNotification = new ScheduledNotification();
                }
                this.simpleNote.ScheduledNotification.Date = value;
                OnPropertyChanged("NotificationDate");
            }
        }

        public TimeSpan NotificationTime
        {
            get
            {
                if (this.simpleNote.ScheduledNotification == null)
                {
                    this.simpleNote.ScheduledNotification = new ScheduledNotification();
                }
                return this.simpleNote.ScheduledNotification.Time;
            }
            set
            {
                
                if (this.simpleNote.ScheduledNotification == null)
                {
                    this.simpleNote.ScheduledNotification = new ScheduledNotification();
                }
                this.simpleNote.ScheduledNotification.Time = value;
                Debug.WriteLine("Setting time" + this.simpleNote.ScheduledNotification.Time);
                OnPropertyChanged("NotificationTime");
            }
        }

        public string NotificationID
        {
            get
            {
                return simpleNote.ScheduledNotification.SchedulingId;
            }
        }

        public string ToString()
        {
            return simpleNote.ToString();
        }

    }
}
