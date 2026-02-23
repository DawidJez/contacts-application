using Contacts.Data;
using Microsoft.EntityFrameworkCore;
// Authorization packages
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
// Auth + Dtos
using Contacts.Dtos.Auth;
using Contacts.Auth;
using Contacts.Data.Entities;
using System.Text.RegularExpressions;


var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// Authorization
builder.Services.AddAuthorization();
 // JWT generator service
builder.Services.AddSingleton<JwtTokenService>();

// Hash password
builder.Services.AddScoped<PasswordHasher<User>>();

// JWT config
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new Exception("Missing Jwt:Key in configuration");
var jwtIssuer = jwtSection["Issuer"] ?? throw new Exception("Missing Jwt:Issuer in configuration");
var jwtAudience = jwtSection["Audience"] ?? throw new Exception("Missing Jwt:Audience in configuration");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// App starting up
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapPost("/api/auth/register", async ( RegisterRequest req, AppDbContext db, PasswordHasher<User> hasher ) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Email and password are required.");

    // Email
    var email = req.Email.Trim(); // Trimming whitespaces

    var exists = await db.Users.AnyAsync(x => x.Email == email);
    if (exists) return Results.Conflict("Email already exists");

    if (!Regex.IsMatch(email, @"^([^@\s]+@[^@\s]+\.[^@\s]+)$")) // Simple regex -> sth@sth.sth
        return Results.BadRequest("Invalid email format.");

    // Password
    if (req.Password.Length < 8) return Results.BadRequest("Password must be at least 8 characters.");

    if (!Regex.IsMatch(req.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$")) // String requires: lower char + upper char + digit + (not letter, not digit) special char
        return Results.BadRequest("Password must have upper and lower cahracter, digit and special character.");

    // Creates user
    var user = new User {
        Email = email,
        Password = null
    };

    user.Password = hasher.HashPassword(user, req.Password);

    // Adds user to db
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email });
});

// App start up
app.Run();