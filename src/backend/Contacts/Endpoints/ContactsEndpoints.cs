using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Contacts.Auth;
using Contacts.Dtos.Auth;
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
}