using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Schedule_Poster.SaveLoadHandling
{
    public static class AccountHandling
    {
        private static string filepath = AppDomain.CurrentDomain.BaseDirectory;
        private static Aes aes = Aes.Create();
        private const string constantsFile = "constants.enc";
        private const string tokensFile = "tokens.enc";

        public static void StartUp()
        {
            GetEncryption();
            GetConstants();
            GetTokens();
        }

        private static void GetTokens()
        {
            if (File.Exists(filepath + constantsFile))
            {
                string decryptedStrings = DecryptFile(tokensFile);
                string[] strings = decryptedStrings.Split(";");

                TwitchAPI.AccessToken = strings[0];
                TwitchAPI.RefreshToken = strings[1];
            }
            else
            {
                Console.WriteLine("Input the access token for the Twitch API:");
                TwitchAPI.AccessToken = NewPassword();
                Console.Clear();
                Console.WriteLine("Input the refresh token for the Twitch API:");
                TwitchAPI.RefreshToken = NewPassword();
                Console.Clear();

                string toEncrypt = Program.Token + ";" + TwitchAPI.ClientID + ";" + TwitchAPI.ClientSecret;
                EncryptFile(constantsFile, toEncrypt);
            }

            HttpResponseMessage response = TwitchAPI.ValidateToken().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {

            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                 
            }

        }

        private static void GetConstants()
        {
            if (File.Exists(filepath + constantsFile))
            {
                string decryptedStrings = DecryptFile(constantsFile);
                string[] strings = decryptedStrings.Split(";");

                Program.Token = strings[0];
                TwitchAPI.ClientID = strings[1];
                TwitchAPI.ClientSecret = strings[2];
            }
            else
            {
                Console.WriteLine("Input the Discord bot token:");
                Program.Token = NewPassword();
                Console.Clear();
                Console.WriteLine("Input the client id for the Twitch API:");
                TwitchAPI.ClientID = Console.ReadLine();
                Console.Clear();
                Console.WriteLine("Input the client secret for the Twitch API:");
                TwitchAPI.ClientSecret = NewPassword();
                Console.Clear();

                string toEncrypt = Program.Token + ";" + TwitchAPI.ClientID + ";" + TwitchAPI.ClientSecret;
                EncryptFile(constantsFile, toEncrypt);
            }
        }

        private static void GetEncryption()
        {
            byte[] salt = GetSalt();
            string uuid = GetUUID();
            byte[] pass = Encoding.UTF8.GetBytes(uuid);
            Rfc2898DeriveBytes keyGen = new Rfc2898DeriveBytes(pass, salt, 5000, HashAlgorithmName.SHA256);
            aes.KeySize = 256;
            aes.Key = keyGen.GetBytes(32);
        }

        private static byte[] GetSalt()
        {
            if (!File.Exists(filepath + "vrede.enc")) 
            {
                if (File.Exists(filepath + constantsFile))
                    File.Delete(filepath + constantsFile);
                if (File.Exists(filepath + tokensFile))
                    File.Delete(filepath + tokensFile);

                byte[] salt = new byte[8];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                string saltString = salt[0].ToString();
                for (int i = 1; i < salt.Length; i++)
                {
                    saltString += ";" + salt[i].ToString();
                }

                File.WriteAllText(filepath + "vrede.enc", saltString);

                return salt;
            }
            else
            {
                string loadedSalt = File.ReadAllText(filepath + "vrede");
                string[] splitString = loadedSalt.Split(";");
                byte[] salt = new byte[8];
                for (int i = 0; i < splitString.Length; i++)
                    salt[i] = byte.Parse(splitString[i]);

                return salt;
            }
        }

        private static void EncryptFile(string file, string toEncrypt)
        {
            aes.GenerateIV();
            using (FileStream fileStream = new FileStream(filepath + file, FileMode.Create))
            {
                fileStream.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cryptoStream = new(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new(cryptoStream, Encoding.UTF8))
                    {
                        writer.Write(toEncrypt);
                    }
                }
            }
        }

        private static string DecryptFile(string file)
        {
            string decryptedStrings = "";

            using (FileStream fileStream = new FileStream(filepath + file, FileMode.Open))
            {
                byte[] iv = new byte[aes.IV.Length];
                int bytesToRead = aes.IV.Length;
                int bytesRead = 0;

                while (bytesToRead > 0)
                {
                    int n = fileStream.Read(iv, bytesRead, bytesToRead);
                    if (n == 0)
                        break;
                    bytesRead += n;
                    bytesToRead -= n;
                }

                aes.IV = iv;

                using (CryptoStream cryptoStream = new(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader decryptReader = new(cryptoStream))
                    {
                        decryptedStrings = decryptReader.ReadToEnd();
                    }
                }
            }

            return decryptedStrings;
        }

        private static string InputPassword()
        {
            string pass = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (pass.Length > 1)
                        pass = pass.Substring(0, pass.Length - 2);
                    else
                        pass = "";
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                    pass += keyInfo.KeyChar;
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            return pass;
        }

        private static bool YNPrompt()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.KeyChar == 'y' || keyInfo.KeyChar == 'Y')
                    return true;
                else if (keyInfo.KeyChar == 'n' || keyInfo.KeyChar == 'N')
                    return false;
            }
        }

        private static string NewPassword()
        {
            bool matchingPass = false;
            string pass = "";

            while (!matchingPass)
            {
                pass = InputPassword();

                Console.WriteLine("Please re-enter the password:");
                if (pass == InputPassword())
                    matchingPass = true;
                else
                    Console.WriteLine("Passwords didn't match. Please input a new password:");
            }

            return pass;
        }

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
