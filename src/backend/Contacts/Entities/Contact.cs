namespace Contacts.Infrastructure.Entites;

public class Contact
{
    public int Id { get; set;}
    public string FirstName { get; set;} = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }

    public string Email {get; set; } = null!;
    public string Password { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? SubCategoryId { get; set; }
    public Subcategory? Subcategory { get; set; } = null!;
    public string? CustomSubcategory { get; set; } // only when Category = "Inny"
}