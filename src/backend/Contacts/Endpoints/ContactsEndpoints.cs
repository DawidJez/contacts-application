using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

using Contacts.Dtos.Contacts;
using Contacts.Data;
using Contacts.Data.Entities;

namespace Contacts.Endpoints;

public static class ContactsEndpoints
{
    public static IEndpointRouteBuilder MapContactsEndpoints (this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contacts").WithTags("Contacts");

        group.MapGet("", GetContacts);
        group.MapGet("/{id:int}", GetContactDetails);
        group.MapPost("", CreateContact).RequireAuthorization();
        group.MapDelete("", DeleteContact).RequireAuthorization();
        group.MapPut("", EditContact).RequireAuthorization();

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

        if (email.Length > 320) return Results.BadRequest("Email max length is 320.");

        if (!Regex.IsMatch(email, @"^([^@\s]+@[^@\s]+\.[^@\s]+)$")) // Simple regex -> sth@sth.sth
            return Results.BadRequest("Invalid email format.");

        // Password
        if (req.Password.Length < 8) return Results.BadRequest("Password must be at least 8 characters.");
        if (req.Password.Length > 255) return Results.BadRequest("Password max length is 255 characters.");

        // String requires: lower char + upper char + digit + (not letter, not digit) special char
        if (!Regex.IsMatch(req.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$")) 
            return Results.BadRequest("Password must have upper and lower cahracter, digit and special character.");

        // First name and last name
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            return Results.BadRequest("First name and last name are required.");

        var firstname = req.FirstName.Trim();
        var lastname = req.LastName.Trim();

        if (firstname.Length > 100) return Results.BadRequest("First name max length is 100.");
        if (lastname.Length > 100) return Results.BadRequest("Last name max length is 100.");

        // Category
        var category = await db.Categories.FirstOrDefaultAsync(el => el.Id == req.CategoryId);
        if (category is null) return Results.BadRequest("Invalid category.");

        var categoryName = category.Name;

        // Custom category and subcategory *optional
        if (!string.IsNullOrWhiteSpace(req.CustomSubcategory)) req.CustomSubcategory = req.CustomSubcategory.Trim();

        if (categoryName == "Służbowy")
        {
            if (!string.IsNullOrWhiteSpace(req.CustomSubcategory))
                return Results.BadRequest("Custom subcategory must be empty for category 'Służbowy'.");

            // If subcategory exists it must belong to the selected category
            var subcExists = await db.Subcategories.AnyAsync(el => el.Id == req.SubcategoryId && el.CategoryId == req.CategoryId);

            if (!subcExists) return Results.BadRequest("Invalid subcategory id for selected category.");
        }
        else if (categoryName == "Inny")
        {
            if (string.IsNullOrWhiteSpace(req.CustomSubcategory))
                return Results.BadRequest("Custom subcategory is required for category 'Inny'.");

            if (req.CustomSubcategory.Length > 100)
                return Results.BadRequest("Custom subcategory max length is 100.");

            if (req.SubcategoryId is not null)
                return Results.BadRequest("Subcategory id must be empty for category 'Inny'.");
        }
        else // "Prywatny"
        {
            if (req.SubcategoryId is not null)
                return Results.BadRequest("Subcategory id must be empty for this category.");

            if (!string.IsNullOrWhiteSpace(req.CustomSubcategory))
                return Results.BadRequest("Custom subcategory must be empty for this category.");
        }

        // Phone number *optional
        if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
        {
            req.PhoneNumber = req.PhoneNumber.Trim();
            var phone = req.PhoneNumber;

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

    private static async Task<IResult> EditContact ( int id, UpdateContactRequest req, AppDbContext db, PasswordHasher<Contact> hasher )
    {
        var foundContact = await db.Contacts.FirstOrDefaultAsync(el => el.Id == id);
        if (foundContact is null) return Results.NotFound();

        // Checks if there is anything to update
        // Using is not null instead: IsNullOrWhiteSpace, because we allow clearing optional elements
        var hasAny =
            req.FirstName is not null ||
            req.LastName is not null ||
            req.PhoneNumber is not null ||
            req.Email is not null ||
            req.Password is not null ||
            req.CategoryId is not null ||
            req.SubcategoryId is not null ||
            req.CustomSubcategory is not null;

        if (!hasAny) return Results.BadRequest("Nothing to update.");

        // Validating the data to put
        // First and last name
        if (req.FirstName is not null)
        {
            var name = req.FirstName.Trim();
            if (name.Length == 0) return Results.BadRequest("FirstName cannot be empty.");
            if (name.Length > 100) return Results.BadRequest("FirstName max length is 100.");
            foundContact.FirstName = name;
        }

        if (req.LastName is not null)
        {
            var name = req.LastName.Trim();
            if (name.Length == 0) return Results.BadRequest("LastName cannot be empty.");
            if (name.Length > 100) return Results.BadRequest("LastName max length is 100.");
            foundContact.LastName = name;
        }

        // Phone number
        if (req.PhoneNumber is not null)
        {
            req.PhoneNumber = req.PhoneNumber.Trim();
            var phone = req.PhoneNumber;

            if (phone.Length == 0)
            {
                // clearing phone
                foundContact.PhoneNumber = null;
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(\+?[0-9]{7,15})$")) 
                    return Results.BadRequest("Invalid phone number format. (Make sure to include international prefix)");
                
                foundContact.PhoneNumber = phone;
            }
        }

        // Email
        if (req.Email is not null)
        {
            var email = req.Email.Trim();
            if (email.Length == 0) return Results.BadRequest("Email cannot be empty.");
            if (email.Length > 320) return Results.BadRequest("Email max length is 320.");

            var exists = await db.Contacts.AnyAsync(x => x.Email == email && x.Id != id);
            if (exists) return Results.Conflict("Contacts email already exists");

            if (!Regex.IsMatch(email, @"^([^@\s]+@[^@\s]+\.[^@\s]+)$")) // Simple regex -> sth@sth.sth
                return Results.BadRequest("Invalid email format.");

            foundContact.Email = email;
        }

        // Password -> New password
        if (!string.IsNullOrWhiteSpace(req.Password))
        {
            if (req.Password.Length < 8) return Results.BadRequest("Password must be at least 8 characters.");
            if (req.Password.Length > 255) return Results.BadRequest("Password max length is 255 characters.");

            // String requires: lower char + upper char + digit + (not letter, not digit) special char
            if (!Regex.IsMatch(req.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$")) 

            return Results.BadRequest("Password must have upper and lower cahracter, digit and special character.");
            foundContact.Password = hasher.HashPassword(foundContact, req.Password);
        }

        // Category, Subcategory and CustomSubcategory
        if (req.CategoryId is not null || req.SubcategoryId is not null || req.CustomSubcategory is not null)
        {
            // Use new category id if it came with request and it must exist
            var newCategoryId = req.CategoryId ?? foundContact.CategoryId;
            var category = await db.Categories.FirstOrDefaultAsync(el => el.Id == newCategoryId);
            if (category is null) return Results.BadRequest("Invalid CategoryId.");

            var categoryName = category.Name;

            // Custom category
            string? custom;
            if (req.CustomSubcategory is null)
            {
                custom = foundContact.CustomSubcategory;
            }
            else
            {
                // If not null then it may be "" 
                if (string.IsNullOrWhiteSpace(req.CustomSubcategory))
                    custom = null; // if so -> clear it
                else
                    custom = req.CustomSubcategory.Trim(); // New custom
            }

            if (custom is not null && custom.Length > 100)
                return Results.BadRequest("CustomSubcategory max length is 100.");

            // Subcategory
            int? subcId;
            if (req.SubcategoryId is null) subcId = foundContact.SubcategoryId; // Don't change
            else subcId = req.SubcategoryId; // Change

            if (categoryName == "Służbowy")
            {
                if (subcId is null) return Results.BadRequest("Subcategory is required for category 'Służbowy'.");

                // Subcategory must exist and belong to selected category
                var subcExists = await db.Subcategories.AnyAsync(el => el.Id == subcId && el.CategoryId == newCategoryId);
                if (!subcExists) return Results.BadRequest("Invalid subcategory for selected category.");

                foundContact.CategoryId = newCategoryId;
                foundContact.SubcategoryId = subcId;
                foundContact.CustomSubcategory = null;
            }
            else if (categoryName == "Inny")
            {
                if (custom is null)
                    return Results.BadRequest("Custom subcategory is required for category 'Inny'.");

                foundContact.CategoryId = newCategoryId;
                foundContact.SubcategoryId = null;
                foundContact.CustomSubcategory = custom;
            }
            else // "Prywatny"
            {
                foundContact.CategoryId = newCategoryId;
                foundContact.SubcategoryId = null;
                foundContact.CustomSubcategory = null;
            }
        }

        await db.SaveChangesAsync();
        return Results.Ok("Contact has been edited");
    }
}