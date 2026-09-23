using System.Text;

using HireFlow.Data;

using HireFlow.Repositories.Application;
using HireFlow.Repositories.Job;

using HireFlow.Services.Application;
using HireFlow.Services.Auth;
using HireFlow.Services.Job;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


// =====================================================
// CREATE BUILDER
// =====================================================

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// MVC / API CONTROLLERS
// =====================================================

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// =====================================================
// DATABASE CONFIGURATION
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            )
        );
    }
);


// =====================================================
// JWT CONFIGURATION VALUES
// =====================================================

var jwtKey =
    builder.Configuration["Jwt:Key"];

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];


// =====================================================
// VALIDATE JWT CONFIGURATION
// =====================================================

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured."
    );
}


if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is not configured."
    );
}


if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Audience is not configured."
    );
}


// =====================================================
// JWT AUTHENTICATION
// =====================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    // -----------------------------------------
                    // SIGNING KEY
                    // -----------------------------------------

                    ValidateIssuerSigningKey =
                        true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtKey
                            )
                        ),


                    // -----------------------------------------
                    // ISSUER
                    // -----------------------------------------

                    ValidateIssuer =
                        true,

                    ValidIssuer =
                        jwtIssuer,


                    // -----------------------------------------
                    // AUDIENCE
                    // -----------------------------------------

                    ValidateAudience =
                        true,

                    ValidAudience =
                        jwtAudience,


                    // -----------------------------------------
                    // TOKEN EXPIRATION
                    // -----------------------------------------

                    ValidateLifetime =
                        true,


                    // -----------------------------------------
                    // NO CLOCK SKEW
                    // -----------------------------------------

                    ClockSkew =
                        TimeSpan.Zero
                };
        }
    );


// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization();


// =====================================================
// DEPENDENCY INJECTION
// =====================================================


// -----------------------------------------------------
// AUTH SERVICE
// -----------------------------------------------------

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();


// -----------------------------------------------------
// JOB REPOSITORY
// -----------------------------------------------------

builder.Services.AddScoped<
    IJobRepository,
    JobRepository
>();


// -----------------------------------------------------
// JOB SERVICE
// -----------------------------------------------------

builder.Services.AddScoped<
    IJobService,
    JobService
>();


// -----------------------------------------------------
// APPLICATION REPOSITORY
// -----------------------------------------------------

builder.Services.AddScoped<
    IApplicationRepository,
    ApplicationRepository
>();


// -----------------------------------------------------
// APPLICATION SERVICE
// -----------------------------------------------------

builder.Services.AddScoped<
    IApplicationService,
    ApplicationService
>();


// =====================================================
// OPENAPI
// =====================================================

builder.Services.AddOpenApi();


// =====================================================
// BUILD APPLICATION
// =====================================================

var app =
    builder.Build();


// Create or migrate the local SQLite schema automatically on startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        dbContext.Database.Migrate();
    }
    catch
    {
        dbContext.Database.EnsureCreated();
    }

    try
    {
        await DbSeeder.SeedJobsAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        logger.LogWarning(ex, "DbSeeder encountered an issue while seeding initial IT jobs.");
    }
}


// =====================================================
// CORS
// =====================================================

app.UseCors("AllowAll");


// =====================================================
// DEVELOPMENT CONFIGURATION
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// =====================================================
// HTTPS REDIRECTION
// =====================================================

app.UseHttpsRedirection();


// =====================================================
// FRONTEND - WWWROOT
// =====================================================

// Automatically load index.html
app.UseDefaultFiles();


// Serve:
// HTML
// CSS
// JavaScript
// Images
// etc.

app.UseStaticFiles();


// =====================================================
// AUTHENTICATION
// =====================================================
//
// Authentication MUST come before Authorization.
//

app.UseAuthentication();


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// API CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// RUN APPLICATION
// =====================================================

app.Run();