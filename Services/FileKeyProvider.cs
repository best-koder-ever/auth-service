using System;
using System.Security.Cryptography;

namespace AuthService.Services
{
    public class FileKeyProvider : IKeyProvider
    {
        private readonly string _privateKeyPath;

        public FileKeyProvider(string privateKeyPath)
        {
            _privateKeyPath = privateKeyPath;
        }

        public RSA GetPrivateKey()
        {
            var privateKey = System.IO.File.ReadAllText(_privateKeyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);
            return rsa;
        }

        public RSA GetPublicKey()
        {
            // Implement logic to retrieve the public key if needed
            throw new NotImplementedException();
        }
    }
}