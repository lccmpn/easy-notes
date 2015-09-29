using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Model
{
    public class SimpleNote : BaseNote
    {
        private string content;

        public SimpleNote() : base() { }

        public SimpleNote(long id, string title, string content)
            : base(id, title)
        {
            this.content = content;
        }

        public string Content
        {
            get { return this.content; }
            set
            {
                if(this.content != value){
                    this.content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + " [Content]: " + content;
        }

    }
}
