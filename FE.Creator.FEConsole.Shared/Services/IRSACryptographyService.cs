using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FE.Creator.FEConsole.Shared.Services.Cryptography
{
   public interface IRSACryptographyService
    {
        byte[] getEncryptionKeys();

        byte[] EncryptData(byte[] data, string encryptKey, bool fOAEP);

        byte[] DecryptData(byte[] data, string decryptKey, bool fOAEP);

        byte[] ExtractPrivateKey(byte[] keys);

        byte[] ExtractPublicKey(byte[] keys);

        byte[] HashAndSignBytes(byte[] DataToSign, string privateKey);

        bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, string publicKey);
    }
}
