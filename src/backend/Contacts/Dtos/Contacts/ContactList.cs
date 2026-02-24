namespace Contacts.Dtos.Contacts;

public class ContactList
{
    public int Id { get; set;}
    public string FirstName { get; set;} = null!;
    public string LastName { get; set; } = null!;
    public string Email {get; set; } = null!;
}