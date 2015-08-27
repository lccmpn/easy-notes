using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.DataModel
{
    class AbstractNote
    {
        public string Title { get; private set; }
        public long ID { get; private set; }

        public AbstractNote(long id, string title)
        {
            this.ID = id;
            this.Title = title;
        }

        public override string ToString()
        {
            return "[ID]: " + ID + " [Title]: " + Title;
        }
    }
}
