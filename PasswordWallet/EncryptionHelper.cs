using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PasswordWallet
{
    public class EncryptionHelper
    {
        public static string EncryptPasswordAES(string password, string encryptionKey)
        {
            EncryptionHelper encriptionHelper = new EncryptionHelper();

            byte[] md5Key = encriptionHelper.CalculateMD5(encryptionKey);

            return encriptionHelper.EncryptAES(password, md5Key);
        }

        public byte[] CalculateMD5(string _secretText)
        {
            string secretText = _secretText ?? "";
            var encoding = new System.Text.ASCIIEncoding();

            byte[] secretTextBytes = encoding.GetBytes(secretText);
            byte[] hashmessage;

            using (var md5 = MD5.Create())
            {
                hashmessage = md5.ComputeHash(secretTextBytes);
            }

            return hashmessage;
        }

        public string EncryptAES(string _password, byte[] _key)
        {
            // Check arguments.
            if (_password == null || _password.Length <= 0)
            {
                throw new ArgumentNullException("_password");
            }

            if (_key == null || _key.Length <= 0)
            {
                throw new ArgumentNullException("_key");
            }

            byte[] encrypted;
            byte[] iv = new byte[16];

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.Zeros;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(_password);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return Convert.ToBase64String(encrypted);
        }



        public static string DecryptPasswordAES(string password, string encryptionKey)
        {
            EncryptionHelper encriptionHelper = new EncryptionHelper();
            byte[] md5Key = encriptionHelper.CalculateMD5(encryptionKey);

            return encriptionHelper.DecryptAES(password, md5Key);
        }

        public string DecryptAES(string _encryptedPassword, byte[] _key)
        {
            // Check arguments.
            if (_encryptedPassword == null || _encryptedPassword.Length <= 0)
            {
                throw new ArgumentNullException("_encryptedPassword");
            }

            if (_key == null || _key.Length <= 0)
            {
                throw new ArgumentNullException("_key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;
            byte[] iv = new byte[16];
            byte[] encryptedBytes = Convert.FromBase64String(_encryptedPassword);

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.Zeros;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext.TrimEnd('\0');
        }
}
}
