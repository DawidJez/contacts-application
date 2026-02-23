namespace Contacts.Infrastructure.Entites;

public class Subcategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }  // fk
    public Category Category { get; set; } = null!; // navigation
}