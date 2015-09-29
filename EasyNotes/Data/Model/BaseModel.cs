using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Data.Model
{
    public class BaseModel : INotifyPropertyChanged
    {
        public long ID { get; protected set; }

        protected BaseModel() { }

        protected BaseModel(long id)
        {
            this.ID = id;
        }

        public override string ToString()
        {
            return "[ID]: " + this.ID;
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
