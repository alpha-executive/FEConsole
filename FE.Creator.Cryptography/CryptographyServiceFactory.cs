using FE.Creator.FEConsole.Shared.Services.Cryptography;

namespace FE.Creator.Cryptography
{
    public class CryptographyServiceFactory
    {
        private static object SyncRoot = new object();
        private static IRSACryptographyService rsaCryptoService = null;
        private static ISHAService shaService = null;
        private static ISymmetricCryptographyService symmetricCryptoService = null;

        public static IRSACryptographyService RSACryptoService
        {
            get
            {
                lock (SyncRoot)
                {
                    if (rsaCryptoService == null)
                    {
                        rsaCryptoService = new RSACryptographyService();
                    }
                }

                return rsaCryptoService;
            }
        }

        public static ISHAService SHAService
        {
            get
            {
                lock (SyncRoot)
                {
                    if (shaService == null)
                    {
                        shaService = new SHAService();
                    }
                }

                return shaService;
            }
        }

        public static ISymmetricCryptographyService SymmetricCryptoService
        {
            get
            {
                lock (SyncRoot)
                {
                    if(symmetricCryptoService == null)
                    {
                        symmetricCryptoService = new SymmetricCryptographyService();
                    }
                }

                return symmetricCryptoService;
            }
        }
    }
}
