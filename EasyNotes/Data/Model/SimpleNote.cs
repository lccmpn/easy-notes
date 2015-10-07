using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Model
{
    public class SimpleNote : BaseNote
    {
        public string Content { get; set; }

        public SimpleNote() : base() { }

        public SimpleNote(long id, string title, ScheduledNotification schedule, string content)
            : base(id, title, schedule)
        {
            this.Content = content;
        }

        public override string ToString()
        {
            return base.ToString() + " [Content]: " + this.Content;
        }

    }
}
