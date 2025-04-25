using System.Security.Cryptography;
using AuthService.Services;

namespace AuthService.Tests
{
    public class InMemoryKeyProvider : IKeyProvider
    {
        private readonly RSA _rsa;

        public InMemoryKeyProvider(RSA rsa)
        {
            _rsa = rsa;
        }

        public RSA GetPrivateKey()
        {
            return _rsa;
        }

        public RSA GetPublicKey()
        {
            return _rsa;
        }
    }
}