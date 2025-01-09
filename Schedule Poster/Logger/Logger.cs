using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule_Poster.Logging
{
    public static class Logger
    {
        private static string logfile = "";
        private static List<Log> logs = new List<Log>();
        private static bool running = false;
        private static readonly object runningLock = new object();
        private static readonly object logsLock = new object();

        public static void Initialize()
        {
            DateTime now = DateTime.Now;
            if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}logs"))
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}logs");
            logfile = $"{AppDomain.CurrentDomain.BaseDirectory}logs/{now.ToString("yyyy-MM-dd-HH-mm-ss")}.log";
            Log("Bot opened");
        }

        public static void Log(string message)
        {
            lock (logsLock)
            {
                logs.Add(new Log(message));
            }
            lock(runningLock)
            {
                if (running || logs.Count == 0)
                    return;
                running = true;
            }
            IEnumerable<Log> sortedLogs;
            lock (logsLock)
            {
               if (logs.Count == 1)
                {
                    File.AppendAllText(logfile, logs[0].message);
                    logs = new List<Log>();
                    running = false;
                    return;
                }
                sortedLogs = logs.OrderBy(x => x.time);
                logs = new List<Log>();
            }
            foreach (Log log in sortedLogs)
            {
                File.AppendAllText(logfile, log.message);
            }
            running = false;
        }
    }

    public class Log
    {
        public DateTime time;
        public string message;

        public Log(string message)
        {
            time = DateTime.Now;
            this.message = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] - {message} \n";
        }
    }
}
