using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.DataModel
{
    class SimpleNote : AbstractNote
    {
        public string Content { get; private set; }

        public SimpleNote(long id, string title, string content)
            : base(id, title)
        {
            this.Content = content;
        }

        public override string ToString()
        {
            return base.ToString() + " [Content]: " + Content;
        }

    }
}
