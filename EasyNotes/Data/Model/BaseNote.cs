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

        protected BaseNote() : base() { }
        
        public BaseNote(long id, string title) : base(id)
        {
            this.ID = id;
            this.title = title;
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
            return base.ToString() + " [Title]: " + Title;
        }
    }
}
