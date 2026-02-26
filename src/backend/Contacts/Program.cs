using Contacts.Data;
using Microsoft.EntityFrameworkCore;
// Authorization packages
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
// Auth
using Contacts.Auth;
using Contacts.Data.Entities;
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

// Authorization and authentication
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
 // JWT generator service
builder.Services.AddSingleton<JwtTokenService>();

// Hash passwords
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<PasswordHasher<Contact>>();

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
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", f => f.WithOrigins("http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod());
});

// App loading up
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapContactsEndpoints();

app.Run();