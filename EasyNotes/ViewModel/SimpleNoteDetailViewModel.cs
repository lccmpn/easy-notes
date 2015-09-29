using System;
using EasyNotes.Data.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.ViewModel
{
    public class SimpleNoteDetailViewModel : BaseViewModel
    {
        public SimpleNote SimpleNote { get; set; }

        public SimpleNoteDetailViewModel()
        {
            this.SimpleNote = new SimpleNote();
        }
    }
}
