namespace Contacts.Dtos.Contacts;

public sealed class CreateContactRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    public int CategoryId { get; set; }

    public int? SubcategoryId { get; set; }
    public string? CustomSubcategory { get; set; }
}