using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Threading.Tasks;

using AuthService.Services;
using AuthService.Data;
using AuthService.Models;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Register IKeyProvider with FileKeyProvider
        services.AddSingleton<IKeyProvider>(provider =>
            new FileKeyProvider("private.key") // Provide the path to the private key
        );

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console() // Log to console
            .WriteTo.Seq("http://seq:5341") // Log to Seq
            .WriteTo.GrafanaLoki("http://loki:3100", labels: new[]
            {
                new LokiLabel { Key = "app", Value = "auth-service" },
                new LokiLabel { Key = "environment", Value = "development" }
            })
            .CreateLogger();

        try
        {
            Log.Information("Hello, World! Serilog is working!");
            Log.Information("This is a test log for Seq and Loki.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while logging.");
        }

        // Add DbContext with conditional logic for in-memory database
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        }
        else
        {
            // Add services to the container.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), 
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));
        }

        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Issuer"],
                IssuerSigningKey = new RsaSecurityKey(GetPrivateKey()) // Use private key for signing
            };
        })
        .AddFacebook(options =>
        {
            options.AppId = Configuration["Authentication:Facebook:AppId"];
            options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
        })
        .AddGoogle(options =>
        {
            options.ClientId = Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
        {
            options.InputFormatters.Add(new PlainTextInputFormatter());
        });

        services.AddControllers(options =>
        {
            options.InputFormatters.Add(new PlainTextInputFormatter());
        });
        services.AddScoped<IAuthService, AuthService.Services.AuthService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Auth Service API",
                Version = "v1",
                Description = "API documentation for the Auth Service."
            });
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check if the database provider is relational before calling Migrate
            if (dbContext.Database.IsRelational())
            {
                dbContext.Database.Migrate();
            }
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Service API v1");
                c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
            });
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private static RSA GetPrivateKey()
    {
        var privateKeyPath = "private.key"; // Path to the private key file

        if (!File.Exists(privateKeyPath))
        {
            // Generate a temporary RSA key for testing purposes
            var rsa = RSA.Create(2048);
            return rsa;
        }

        var privateKey = File.ReadAllText(privateKeyPath);
        var rsaKey = RSA.Create();
        rsaKey.ImportFromPem(privateKey);
        return rsaKey;
    }
}

public class PlainTextInputFormatter : Microsoft.AspNetCore.Mvc.Formatters.TextInputFormatter
{
    public PlainTextInputFormatter()
    {
        SupportedMediaTypes.Add("text/plain");
        SupportedEncodings.Add(System.Text.Encoding.UTF8);
        SupportedEncodings.Add(System.Text.Encoding.Unicode);
    }

    protected override bool CanReadType(Type type)
    {
        return type == typeof(string);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        using var reader = new StreamReader(context.HttpContext.Request.Body, encoding);
        var content = await reader.ReadToEndAsync();
        return await InputFormatterResult.SuccessAsync(content);
    }
}