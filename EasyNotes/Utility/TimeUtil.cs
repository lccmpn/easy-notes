using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Utility
{
    class TimeUtil
    {
        public static String GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss");
        }
    }
}
