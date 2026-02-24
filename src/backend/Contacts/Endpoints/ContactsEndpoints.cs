using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

using Contacts.Dtos.Contacts;
using Contacts.Data;
using Contacts.Data.Entities;
using System.Timers;

namespace Contacts.Endpoints;

public static class ContactsEndpoints
{
    public static IEndpointRouteBuilder MapContactsEndpoints (this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contacts").WithTags("Contacts");

        group.MapGet("", GetContacts);
        group.MapGet("/{id:int}", GetContactDetails);
        group.MapPost("", CreateContact).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetContacts ( AppDbContext db )
    {
        // .AsNoTracking() gives us faster select response
        var contacts = await db.Contacts.AsNoTracking().Select(el => new ContactList
        {
            Id = el.Id,
            FirstName = el.FirstName,
            LastName = el.LastName,
            Email = el.Email
        }).OrderBy(x => x.LastName).ToListAsync();

        return Results.Ok(contacts);
    }

    private static async Task<IResult> GetContactDetails ( int id , AppDbContext db)
    {
        var contactDetails = await db.Contacts.AsNoTracking().Select(el => new ContactDetails
        {
            Id = el.Id,
            FirstName = el.FirstName,
            LastName = el.LastName,
            PhoneNumber = el.PhoneNumber,

            Email = el.Email,

            CategoryId = el.CategoryId,
            CategoryName = el.Category.Name,

            SubcategoryId = el.SubcategoryId,
            // Could have been a null reference
            SubcategoryName = el.Subcategory != null ? el.Subcategory.Name : null,

            CustomSubcategory = el.CustomSubcategory
        }).Where(el => el.Id == id ).FirstOrDefaultAsync();

        return contactDetails is not null ? Results.Ok(contactDetails) : Results.NotFound();
    }

    private static async Task<IResult> CreateContact ( CreateContactRequest req, AppDbContext db, PasswordHasher<Contact> hasher )
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Email and password are required.");

        // Email
        var email = req.Email.Trim();

        var exists = await db.Contacts.AnyAsync(x => x.Email == email);
        if (exists) return Results.Conflict("Contacts email already exists");

        if (!Regex.IsMatch(email, @"^([^@\s]+@[^@\s]+\.[^@\s]+)$")) // Simple regex -> sth@sth.sth
            return Results.BadRequest("Invalid email format.");

        // Password
        if (req.Password.Length < 8) return Results.BadRequest("Password must be at least 8 characters.");

        // String requires: lower char + upper char + digit + (not letter, not digit) special char
        if (!Regex.IsMatch(req.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$")) 
            return Results.BadRequest("Password must have upper and lower cahracter, digit and special character.");

        // First name and last name
        var firstname = req.FirstName.Trim();
        var lastname = req.LastName.Trim();

        if (firstname.Length == 0) return Results.BadRequest("First name is required.");
        if (lastname.Length == 0) return Results.BadRequest("Last name is required.");

        // Category
        var category = await db.Categories.FirstOrDefaultAsync(el => el.Id == req.CategoryId);
        if (category is null) return Results.BadRequest("Invalid category.");

        var categoryName = category.Name;

        // Custom category and subcategory *optional
        if (!string.IsNullOrWhiteSpace(req.CustomSubcategory)) req.CustomSubcategory = req.CustomSubcategory.Trim();

        if (categoryName == "Służbowy")
        {
            if (!string.IsNullOrWhiteSpace(req.CustomSubcategory))
                return Results.BadRequest("CustomSubcategory must be empty for category 'Służbowy'.");

            // If subcategory exists it must belong to the selected category
            var subcExists = await db.Subcategories.AnyAsync(el => el.Id == req.SubcategoryId && el.CategoryId == req.CategoryId);

            if (!subcExists) return Results.BadRequest("Invalid SubcategoryId for selected category.");
        }
        else if (categoryName == "Inny")
        {
            if (string.IsNullOrWhiteSpace(req.CustomSubcategory))
                return Results.BadRequest("CustomSubcategory is required for category 'Inny'.");

            if (req.CustomSubcategory.Length > 100)
                return Results.BadRequest("CustomSubcategory max length is 100.");

            if (req.SubcategoryId is not null)
                return Results.BadRequest("SubcategoryId must be empty for category 'Inny'.");
        }
        else // "Prywatny"
        {
            if (req.SubcategoryId is not null)
                return Results.BadRequest("SubcategoryId must be empty for this category.");

            if (!string.IsNullOrWhiteSpace(req.CustomSubcategory))
                return Results.BadRequest("CustomSubcategory must be empty for this category.");
        }

        // Phone number *optional
        if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
        {
            req.PhoneNumber = req.PhoneNumber.Trim();
            var phone = req.PhoneNumber;
            if (phone.Length > 16) return Results.BadRequest("PhoneNumber max length is 16.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+?[0-9]{7,15})$")) 
                return Results.BadRequest("Invalid phone number format. (Make sure to include international prefix)");
        }

        // Creates contact
        var contact = new Contact 
        {
            FirstName = firstname,
            LastName = lastname,
            Email = email,
            CategoryId = req.CategoryId,
            
            PhoneNumber = req.PhoneNumber,
            SubcategoryId = req.SubcategoryId,
            CustomSubcategory = req.CustomSubcategory
        };

        contact.Password = hasher.HashPassword(contact, req.Password);

        db.Contacts.Add(contact);
        await db.SaveChangesAsync();

        return Results.Created($"/api/contacts/{contact.Id}", new { contact.Id, contact.FirstName, contact.LastName } );
    }

    private static async Task<IResult> DeleteContact ( int id, AppDbContext db )
    {
        var foundContact = await db.Contacts.FirstOrDefaultAsync(el => el.Id == id);
        if (foundContact is null) return Results.NotFound();

        db.Contacts.Remove(foundContact);
        await db.SaveChangesAsync();

        return Results.Ok($"Contact {id} has been deleted.");
    }
}