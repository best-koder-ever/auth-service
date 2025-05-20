using Microsoft.AspNetCore.Identity;
using System; // Required for DateTime

namespace AuthService.Models
{
        public class User : IdentityUser
        {
                // Additional properties can be added here
                public string Bio { get; set; }
                public string ProfilePicture { get; set; }
                public DateTime? DateOfBirth { get; set; } // To calculate the user's age
                public string Gender { get; set; } // To store the user's gender
                public string Location { get; set; } // To store the user's city or country
                public string Interests { get; set; } // To store the user's interests or hobbies
                public DateTime LastActive { get; set; } // To track when the user was last active
        }
}