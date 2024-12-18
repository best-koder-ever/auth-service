public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<string> LoginWithFacebookAsync(string accessToken);
    Task<string> LoginWithGoogleAsync(string idToken);
    Task<string> LoginWithPhoneNumberAsync(string phoneNumber, string code);
}