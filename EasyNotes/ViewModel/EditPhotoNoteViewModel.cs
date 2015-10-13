using EasyNotes.Data.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace EasyNotes.ViewModel
{
    [DataContract]

    class EditPhotoNoteViewModel : BaseViewModel
    {
        private PhotoNote photoNote;
        private bool notificationDateVisible;

        public EditPhotoNoteViewModel()
        {
            this.photoNote = new PhotoNote();
        }

        public EditPhotoNoteViewModel(PhotoNote simpleNote)
        {
            this.photoNote = simpleNote;
        }

        [DataMember]
        public long ID
        {
            get
            {
                return this.photoNote.ID;
            }
        }

        [DataMember]
        public string Title
        {
            get
            {
                return photoNote.Title;
            }
            set
            {
                this.photoNote.Title = value;
                OnPropertyChanged("Title");
            }
        }

        [DataMember]
        public string Content
        {
            get
            {
                return photoNote.Content;
            }
            set
            {
                if (photoNote.Content != value)
                {
                    photoNote.Content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public BitmapImage Photo
        {
            get
            {
                if (String.IsNullOrEmpty(photoNote.PhotoPath))
                {
                    return null;
                }
                else
                {
                    Debug.WriteLine("Path: " + photoNote.PhotoPath);
                    return new BitmapImage(new Uri(photoNote.PhotoPath));
                }
            }
        }

        [DataMember]
        public string PhotoPath
        {
            get
            {
                return photoNote.PhotoPath;
            }
            set
            {
                photoNote.PhotoPath = value;
            }
        }

        [DataMember]
        public bool IsNotificationDateVisible
        {
            get
            {
                return photoNote.ScheduledNotification != null;
            }
            set
            {
                notificationDateVisible = value;
            }
        }

        [DataMember]
        public DateTimeOffset NotificationDate
        {
            get
            {
                if(photoNote.ScheduledNotification == null)
                {
                    this.photoNote.ScheduledNotification = new ScheduledNotification();
                }
                return this.photoNote.ScheduledNotification.Date;
            }
            set
            {
                if (this.photoNote.ScheduledNotification == null)
                {
                    this.photoNote.ScheduledNotification = new ScheduledNotification();
                }
                this.photoNote.ScheduledNotification.Date = value;
                OnPropertyChanged("NotificationDate");
            }
        }

        [DataMember]
        public TimeSpan NotificationTime
        {
            get
            {
                if (this.photoNote.ScheduledNotification == null)
                {
                    this.photoNote.ScheduledNotification = new ScheduledNotification();
                }
                return this.photoNote.ScheduledNotification.Time;
            }
            set
            {
                
                if (this.photoNote.ScheduledNotification == null)
                {
                    this.photoNote.ScheduledNotification = new ScheduledNotification();
                }
                this.photoNote.ScheduledNotification.Time = value;
                OnPropertyChanged("NotificationTime");
            }
        }

        public string NotificationID
        {
            get
            {
                return photoNote.ScheduledNotification.SchedulingId;
            }
        }

        public override string ToString()
        {
            return photoNote.ToString();
        }
    }
}
