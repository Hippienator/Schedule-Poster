using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Schedule_Poster.SaveLoadHandling
{
    public static class AccountHandling
    {
        private static string filepath = AppDomain.CurrentDomain.BaseDirectory;
        private static Aes aes = Aes.Create();



        public static string GetUUID()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsUUID();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxUUID();
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system.");
            }
        }

        // Windows: Retrieve UUID using WMI
        private static string GetWindowsUUID()
        {
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/C wmic csproduct get uuid";

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string result;
            using (var reader = process.StandardOutput)
            {
                string[] results = reader.ReadToEnd().Trim().Split("\n");
                result = results[results.Length-1];
            }

            process.WaitForExit();
            return result;
        }

        // Linux: Retrieve UUID from /etc/machine-id or /var/lib/dbus/machine-id
        private static string GetLinuxUUID()
        {
            string uuid = string.Empty;
            if (System.IO.File.Exists("/etc/machine-id"))
            {
                uuid = System.IO.File.ReadAllText("/etc/machine-id").Trim();
            }
            else if (System.IO.File.Exists("/var/lib/dbus/machine-id"))
            {
                uuid = System.IO.File.ReadAllText("/var/lib/dbus/machine-id").Trim();
            }
            return uuid;
        }


    }
}
