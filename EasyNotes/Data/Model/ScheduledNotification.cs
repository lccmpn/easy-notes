using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Model
{
    public class ScheduledNotification
    {
        public long DataBaseId { get; set; }
        public string SchedulingId { get; set; }
        public DateTimeOffset Date { get; set; }
        public TimeSpan Time { get; set; }

        public ScheduledNotification()
        {
            this.Date = DateTimeOffset.Now.Date;
            this.Time = DateTimeOffset.Now.TimeOfDay;
        }

        public ScheduledNotification(long dataBaseId, string id, DateTimeOffset date)
        {
            this.SchedulingId = id;
            this.DataBaseId = dataBaseId;
            this.Date = date.Date;
            this.Time = date.TimeOfDay;
        }

        public override string ToString()
        {
            return "[SCHEDULED NOTIFICATION] - [Date] " + Date.ToString() + " [Time] " + Time.ToString();
        }

    }
}
