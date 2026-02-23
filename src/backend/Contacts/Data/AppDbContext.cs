using Microsoft.EntityFrameworkCore;
using Contacts.Data.Entities;

namespace Contacts.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<User> Users => Set<User>();

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

            el.Property(x => x.Email).IsRequired().HasMaxLength(320); // RFC length standard
            el.HasIndex(x => x.Email).IsUnique(); // Faster contant search
            el.Property(x => x.Password).IsRequired().HasMaxLength(255);

            el.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);
            el.HasOne(x => x.Subcategory).WithMany().HasForeignKey(x => x.SubcategoryId);
            el.Property(x => x.CustomSubcategory).HasMaxLength(100);
        });

        // Users
        modelBuilder.Entity<User>(el =>
        {
            el.ToTable("Users");
            el.HasKey(x => x.Id);

            el.Property(x => x.Email).IsRequired().HasMaxLength(320);
            el.HasIndex(x => x.Email).IsUnique();
            el.Property(x => x.Password).IsRequired().HasMaxLength(255);
        });

        // Seed
        // Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Służbowy" },
            new Category { Id = 2, Name = "Prywatny" },
            new Category { Id = 3, Name = "Inny" }
        );

        // Subcategories
        modelBuilder.Entity<Subcategory>().HasData(
            
            new Subcategory { Id = 1, CategoryId = 1, Name = "Szef"},
            new Subcategory { Id = 2, CategoryId = 1, Name = "Klient" },
            new Subcategory { Id = 3, CategoryId = 1, Name = "Współpracownik" }
        );
    }
}