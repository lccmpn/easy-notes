using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Utility
{
    class TimeUtil
    {
        private const string ISO8601_STANDARD_FORMAT = "s";

        public static String GetStringTimestamp()
        {
            return DateTimeOffset.Now.ToString(ISO8601_STANDARD_FORMAT);
        }

        public static String ConvertDateTimeOffsetToString(DateTimeOffset dateTime)
        {
            return dateTime.ToString(ISO8601_STANDARD_FORMAT);
        }

    }
}
