namespace Contacts.Dtos.Contacts;

public sealed class ContactDetails
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = null!;

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;

    public int? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }

    public string? CustomSubcategory { get; set; } // only when Category = "Inny"
}