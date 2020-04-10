using FE.Creator.FEConsole.Shared.Services.Cryptography;
using System;
using System.Security.Cryptography;

namespace FE.Creator.Cryptography
{
    internal class RSACryptographyService : IRSACryptographyService
    {
        /// <summary>
        /// Decrypt A encryption binary data 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="decryptKey">base64 formatted private/public key pairs</param>
        /// <param name="fOAEP"></param>
        /// <returns></returns>
        public byte[] DecryptData(byte[] data, string decryptKey, bool fOAEP)
        {
            byte[] keys = Convert.FromBase64String(decryptKey);

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportCspBlob(keys);

                return RSA.Decrypt(data, fOAEP);
            }
        }

        public byte[] EncryptData(byte[] data, string encryptKey, bool fOAEP)
        {
            byte[] keys = Convert.FromBase64String(encryptKey);
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportCspBlob(keys);

                return RSA.Encrypt(data, fOAEP);
            }
        }

        public byte[] ExtractPrivateKey(byte[] keys)
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportCspBlob(keys);

                if (RSA.PublicOnly)
                    throw new ArgumentException("Not a valid private key provided.");
                return RSA.ExportCspBlob(true);
            }
        }

        public byte[] ExtractPublicKey(byte[] keys)
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportCspBlob(keys);

                return RSA.ExportCspBlob(false);
            }
        }

        public byte[] getEncryptionKeys()
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
               return RSA.ExportCspBlob(true);
            }
        }

        /// <summary>
        /// Sign the bytes by SHA256 alg with RSA private key.
        /// </summary>
        /// <param name="dataToSign">data array to be signed.</param>
        /// <param name="privateKey">base64 string private key used to sign the data.</param>
        /// <returns></returns>
        public byte[] HashAndSignBytes(byte[] dataToSign, string privateKey)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.  
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportCspBlob(Convert.FromBase64String(privateKey));

                // Hash and sign the data. Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.SignData(dataToSign, new SHA256CryptoServiceProvider());
            }
            catch (CryptographicException e)
            {
                throw e;
            }
        }

        public bool VerifySignedHash(byte[] dataToVerify, byte[] signedData, string publicKey)
        {
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAalg.ImportCspBlob(Convert.FromBase64String(publicKey));

                // Verify the data using the signature.  Pass a new instance of SHA1CryptoServiceProvider
                // to specify the use of SHA1 for hashing.
                return RSAalg.VerifyData(dataToVerify, new SHA256CryptoServiceProvider(), signedData);

            }
            catch (CryptographicException e)
            {
                return false;
            }
        }
    }
}
