using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

using Contacts.Auth;
using Contacts.Dtos.Auth;
using Contacts.Data;
using Contacts.Data.Entities;

namespace Contacts.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);

        return app;
    }

    // Register
    private static async Task<IResult> Register ( RegisterRequest req, AppDbContext db, PasswordHasher<User> hasher )
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
            return Results.BadRequest("Password must have: upper and lower character, digit and special character.");

        // Creates user
        var user = new User {
            Email = email
        };

        user.Password = hasher.HashPassword(user, req.Password);

        // Adds user to db
        db.Users.Add(user);
        await db.SaveChangesAsync();

        //return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email });
        return Results.Ok("User created"); // To show message in frontend
    }

    // Login
    private static async Task<IResult> Login (LoginRequest req, AppDbContext db, PasswordHasher<User> hasher, JwtTokenService tokens)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Email and password are required.");

        // Email
        var email = req.Email.Trim();

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null) 
            return Results.Problem(title: "Unauthorized", detail: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);

        // Password
        var verify = hasher.VerifyHashedPassword(user, user.Password, req.Password);
        if (verify == PasswordVerificationResult.Failed) 
            return Results.Problem(title: "Unauthorized", detail: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);

        var token = tokens.CreateToken(user.Id, user.Email);
        return Results.Ok(new { token });
    }
}