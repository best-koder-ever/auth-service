using System;
namespace AuthService.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserProfileSummaryDto UserProfile { get; set; }
    }

    public class UserProfileSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastActiveAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
