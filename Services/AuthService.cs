using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AuthService.Models;
using AuthService.DTOs;
using System;
using System.Linq;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IKeyProvider _keyProvider;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, IKeyProvider keyProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _keyProvider = keyProvider;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new ApplicationException("Email already exists");
            }

            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Registration failed: {errors}");
            }
            return GenerateJwtToken(user);
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
            return GenerateJwtToken(user);
        }

        public async Task<string> LoginWithFacebookAsync(string accessToken)
        {
            // Example implementation for Facebook login
            // Validate the Facebook access token and retrieve user information
            var facebookUserId = await ValidateFacebookAccessTokenAsync(accessToken);
            if (facebookUserId == null)
            {
                throw new UnauthorizedAccessException("Invalid Facebook access token.");
            }

            var user = await _userManager.FindByLoginAsync("Facebook", facebookUserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            return GenerateJwtToken(user);
        }

        public async Task<string> LoginWithGoogleAsync(string idToken)
        {
            // Example implementation for Google login
            // Validate the Google ID token and retrieve user information
            var googleUserId = await ValidateGoogleIdTokenAsync(idToken);
            if (googleUserId == null)
            {
                throw new UnauthorizedAccessException("Invalid Google ID token.");
            }

            var user = await _userManager.FindByLoginAsync("Google", googleUserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            return GenerateJwtToken(user);
        }

        public async Task<string> LoginWithPhoneNumberAsync(string phoneNumber, string verificationCode)
        {
            // Example implementation for phone number login
            // Validate the phone number and verification code
            var isValid = await ValidatePhoneNumberAsync(phoneNumber, verificationCode);
            if (!isValid)
            {
                throw new UnauthorizedAccessException("Invalid phone number or verification code.");
            }

            var user = await _userManager.FindByNameAsync(phoneNumber);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            return GenerateJwtToken(user);
        }

        private async Task<string> ValidateFacebookAccessTokenAsync(string accessToken)
        {
            // Add logic to validate the Facebook access token
            // Return the Facebook user ID if valid, otherwise return null
            return "facebook-user-id"; // Replace with actual implementation
        }

        private async Task<string> ValidateGoogleIdTokenAsync(string idToken)
        {
            // Add logic to validate the Google ID token
            // Return the Google user ID if valid, otherwise return null
            return "google-user-id"; // Replace with actual implementation
        }

        private async Task<bool> ValidatePhoneNumberAsync(string phoneNumber, string verificationCode)
        {
            // Add logic to validate the phone number and verification code
            return true; // Replace with actual implementation
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var rsa = _keyProvider.GetPrivateKey();

            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}