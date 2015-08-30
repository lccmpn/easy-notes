using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.DataModel
{
    public class BaseNote : INotifyPropertyChanged
    {
        private string title;
        public long ID { get; private set; }

        public BaseNote() { }

        public BaseNote(long id, string title)
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
            return "[ID]: " + ID + " [Title]: " + Title;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
