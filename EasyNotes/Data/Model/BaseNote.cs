using System;
using EasyNotes.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Model
{
    public class BaseNote : BaseModel
    {
        private string title;
        public ScheduledNotification ScheduledNotification {get; set;}

        protected BaseNote() : base() {
            this.ScheduledNotification = null;
        }

        public BaseNote(long id, string title, ScheduledNotification scheduledNotification)
            : base(id)
        {
            this.Id = id;
            this.title = title;
            this.ScheduledNotification = scheduledNotification;
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (value != this.title)
                {
                    this.title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public override string ToString()
        {
            if(ScheduledNotification == null)
            {
                return base.ToString() + " [Title]: " + Title;
            }
            return base.ToString() + " [Title]: " + Title + " [Date]: " + ScheduledNotification.Date.ToString() + " " + ScheduledNotification.Time.ToString();
        }
    }
}
