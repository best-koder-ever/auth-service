using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
}