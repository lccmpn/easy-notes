using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.DataModel
{
    class SimpleNoteDetail : BaseNote
    {
        private string content;

        public SimpleNoteDetail() : base() { }

        public SimpleNoteDetail(long id, string title, string content)
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
