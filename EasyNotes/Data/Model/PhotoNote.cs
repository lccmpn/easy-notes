using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Model
{
    class PhotoNote : BaseNote
    {
        public string Content { get; set; }
        public string PhotoPath { get; set; }

        public PhotoNote() : base() { }

        public PhotoNote(long id, string title, ScheduledNotification schedule, string content, string photoPath)
            : base(id, title, schedule)
        {
            this.Content = content;
            this.PhotoPath = photoPath;
        }

        public override string ToString()
        {
            return " [Content] " + Content + "[PhotoPath] " + PhotoPath;
        }
    }
}

