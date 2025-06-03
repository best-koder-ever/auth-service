using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using AuthService.Data; // Assuming ApplicationDbContext is in this namespace
using System.Linq; // Added for .Any()
using System; // Added for InvalidOperationException

namespace AuthService.Data // Or the appropriate namespace
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Get environment variable to determine the environment
            string environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // This will be the AuthService project directory when running EF tools
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("AuthServiceDb");

            // --- BEGIN DIAGNOSTIC ---
            System.Console.WriteLine($"DesignTimeDbContextFactory: ASPNETCORE_ENVIRONMENT = {environment}");
            System.Console.WriteLine($"DesignTimeDbContextFactory: BasePath = {Directory.GetCurrentDirectory()}");
            System.Console.WriteLine($"DesignTimeDbContextFactory: Attempting to read connection string 'AuthServiceDb'.");
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Console.WriteLine("DesignTimeDbContextFactory: Connection string 'AuthServiceDb' NOT FOUND or is empty.");
                // Log all connection strings for debugging
                var allConnectionStrings = configuration.GetSection("ConnectionStrings").GetChildren();
                if (allConnectionStrings.Any())
                {
                    foreach (var cs in allConnectionStrings)
                    {
                        System.Console.WriteLine($"DesignTimeDbContextFactory: Found CS: {cs.Key} = {cs.Value}");
                    }
                }
                else
                {
                    System.Console.WriteLine("DesignTimeDbContextFactory: No 'ConnectionStrings' section found or it is empty.");
                }
                throw new InvalidOperationException("Could not find a connection string named 'AuthServiceDb'. Ensure it is configured in appsettings.json or appsettings.Development.json.");
            }
            else
            {
                System.Console.WriteLine($"DesignTimeDbContextFactory: Found connection string 'AuthServiceDb' = '{connectionString}'");
            }
            // --- END DIAGNOSTIC ---

            // Using specific server version from docker-compose.yml (mysql:8.0.28)
            builder.UseMySql(connectionString, new MySqlServerVersion(new System.Version(8, 0, 28)),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: System.TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
