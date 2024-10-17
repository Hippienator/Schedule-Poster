using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster
{
    internal class StreamInformation
    {
        public DateTimeOffset StartTime {  get; set; }
        public string? GameName { get; set; }

        public StreamInformation(DateTimeOffset startTime, string? gameName)
        {
            StartTime = startTime;
            GameName = gameName;
        }
    }
}
