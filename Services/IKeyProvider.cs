using System.Security.Cryptography;

namespace AuthService.Services
{
    public interface IKeyProvider
    {
        RSA GetPrivateKey();
        RSA GetPublicKey();
    }
}