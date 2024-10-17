using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    
    public class ScheduleInformation
    {
        public string TimeCode { get; set; }
        public string GameName { get; set; }

        public ScheduleInformation(string timeCode, string gameName)
        { 
            TimeCode = timeCode; 
            GameName = gameName; 
        }
    }
}
