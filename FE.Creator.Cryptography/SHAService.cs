using FE.Creator.FEConsole.Shared.Services.Cryptography;
using System;
using System.Security.Cryptography;

namespace FE.Creator.Cryptography
{
    public class SHAService : ISHAService
    {
        public string CalculateSHA256(byte[] content)
        {
            using (var sha = SHA256.Create())
            {
                byte[] checksum = sha.ComputeHash(content);

                return Convert.ToBase64String(checksum);
            }
        }
    }
}
