using Microsoft.EntityFrameworkCore;

namespace Contacts.Infrastructure.Entites;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Subcategory> Subcategories { get; set; }

    // Configuration and seed
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Categories
        modelBuilder.Entity<Category>(el =>
        {
            el.ToTable("Categories");
            el.HasKey(x => x.Id);
            el.Property(x => x.Name).IsRequired().HasMaxLength(50);
            el.HasIndex(x => x.Name).IsUnique(); // Faster access
        });

        // Subcategories
        modelBuilder.Entity<Subcategory>(el =>
        {
            el.ToTable("Subcategories");
            el.HasKey(x => x.Id);
            el.Property(x => x.Name).IsRequired().HasMaxLength(50);

            el.HasOne(subc => subc.Category).WithMany(c => c.Subcategories).HasForeignKey(subc => subc.CategoryId); // Relation

            el.HasIndex(x => x.Name ).IsUnique();
        });

        // Contacts
        modelBuilder.Entity<Contact>(el =>
        {
            el.ToTable("Contacts");
            el.HasKey(x => x.Id);

            el.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            el.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            el.Property(x => x.PhoneNumber).HasMaxLength(16); // International length standard -> "+" + max 15 numbers

            el.Property(x => x.Email).IsRequired().HasMaxLength(320);
            el.HasIndex(x => x.Email).IsUnique();

            el.Property(x => x.Password).IsRequired().HasMaxLength(255);

            el.Property(x => x.CustomSubcategory).HasMaxLength(100);

            el.HasOne(x => x.Category)
             .WithMany()
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            el.HasOne(x => x.Subcategory)
             .WithMany()
             .HasForeignKey(x => x.SubcategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
