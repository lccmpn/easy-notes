using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace EasyNotes.ViewModel
{
    class PhotoDetailViewModel
    {
        private string photoPath;

        public PhotoDetailViewModel(string photoPath)
        {
            this.photoPath = photoPath;
        }

        public BitmapImage Photo
        {
            get
            {
                if (String.IsNullOrEmpty(photoPath))
                {
                    return null;
                }
                else
                {
                    Debug.WriteLine("Path: " + photoPath);
                    return new BitmapImage(new Uri(photoPath));
                }
            }
        } 

    }
}
