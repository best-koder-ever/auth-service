// Add SignInManager configuration
services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddSignInManager()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Seed a test user
var userManager = scopedServices.GetRequiredService<UserManager<User>>();
var user = new User
{
    Email = "test@example.com",
    UserName = "testuser"
};
userManager.CreateAsync(user, "password123").Wait();