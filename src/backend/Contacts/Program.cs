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
// Endpoints import
using Contacts.Endpoints;


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
app.MapAuthEndpoints();

app.Run();