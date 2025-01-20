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
        public string Title {  get; set; }

        public StreamInformation(DateTimeOffset startTime, string? gameName, string title)
        {
            StartTime = startTime;
            GameName = gameName;
            Title = title;
        }
    }
}
