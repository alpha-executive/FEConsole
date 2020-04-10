using FE.Creator.FEConsole.Shared.Services.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FE.Creator.Cryptography
{
    public class SymmetricCryptographyService : ISymmetricCryptographyService
    {
        private byte[] getIV(string key)
        {
            byte[] originalKey = Convert.FromBase64String(key);

            int ivLength = BitConverter.ToInt32(originalKey, originalKey.Length - 4);
            byte[] iv = new byte[ivLength];
            Array.Copy(originalKey, iv, ivLength);

            return iv;
        }
        private byte[] getKey(string key)
        {
            byte[] originalKey = Convert.FromBase64String(key);
            int ivLength = BitConverter.ToInt32(originalKey, originalKey.Length - 4);
            byte[] keys = new byte[originalKey.Length - 4 - ivLength];

            Array.Copy(originalKey, ivLength, keys, 0,  originalKey.Length - 4 - ivLength);

            return keys;
        }

        public byte[] DecryptData(byte[] data, string decryptKey)
        {
            TripleDESCryptoServiceProvider TDES = new TripleDESCryptoServiceProvider();
            TDES.Key = getKey(decryptKey);
            TDES.IV = getIV(decryptKey);

            using (MemoryStream msstream = new MemoryStream(data))
            {
                using (CryptoStream CryptStream = new CryptoStream(msstream,
                             TDES.CreateDecryptor(TDES.Key, TDES.IV),
                             CryptoStreamMode.Read))
                {
                    using (StreamReader SReader = new StreamReader(CryptStream))
                    {
                        string content = SReader.ReadToEnd();

                        return UTF8Encoding.UTF8.GetBytes(content);
                    }
                }
            }
        }

        public byte[] EncryptData(byte[] data, string encryptKey)
        {
            using (MemoryStream msstream = new MemoryStream())
            {
                TripleDESCryptoServiceProvider TDES = new TripleDESCryptoServiceProvider();
                TDES.Key = getKey(encryptKey);
                TDES.IV = getIV(encryptKey);
                using (CryptoStream CryptStream = new CryptoStream(msstream,
                                 TDES.CreateEncryptor(TDES.Key, TDES.IV),
                                 CryptoStreamMode.Write))
                {
                    using(StreamWriter writer = new StreamWriter(CryptStream))
                    {
                        writer.Write(UTF8Encoding.UTF8.GetString(data));
                    }

                    return msstream.ToArray();
                }
            }
        }


        /// <summary>
        /// Get a Symmetric cryptography key.
        /// IV+KEY+4 byte length of IV.
        /// </summary>
        /// <returns></returns>
        public byte[] getEncryptionKeys()
        {
            TripleDESCryptoServiceProvider TDES = new TripleDESCryptoServiceProvider();
            TDES.GenerateIV();
            TDES.GenerateKey();

            //length of 4 byte array.
            byte[] lengthBytes = BitConverter.GetBytes(TDES.IV.Length);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(lengthBytes);

            byte[] keys = new byte[TDES.IV.Length + TDES.Key.Length + lengthBytes.Length];
            Array.Copy(TDES.IV, keys, TDES.IV.Length);
            Array.Copy(TDES.Key, 0,  keys, TDES.IV.Length, TDES.Key.Length);
            Array.Copy(lengthBytes, 0, keys, TDES.IV.Length + TDES.Key.Length, lengthBytes.Length);

            return keys;
        }
    }
}
