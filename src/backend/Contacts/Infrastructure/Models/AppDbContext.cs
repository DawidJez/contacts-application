using Microsoft.EntityFrameworkCore;

namespace Contacts.Infrastructure.Models;

public class AppDbContext : DbContext
{
    public DbSet<Contact> Contacts { get; set;}
    public AppDbContext(DbContextOptions options) : base(options) { }
}
