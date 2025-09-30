using System.Threading.Tasks;   
using AuthService.Models;
using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<string> LoginWithFacebookAsync(string accessToken);
        Task<string> LoginWithGoogleAsync(string idToken);
        Task<string> LoginWithPhoneNumberAsync(string phoneNumber, string code);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(User user, string password);
        string GenerateJwtToken(User user);
    }

}