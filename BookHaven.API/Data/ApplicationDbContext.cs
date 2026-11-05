using BookHaven.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AuthorId).IsRequired();
            entity.Property(e => e.Isbn).IsRequired().HasMaxLength(13);
            entity.Property(e => e.PublishedYear).IsRequired();
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.ImageLocalPath).HasMaxLength(500);
            entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.Isbn).IsUnique(); // Ensure ISBN is unique across all books for performance optimization and data integrity

            entity.HasOne(b => b.Category)
                  .WithMany(c => c.Books)
                  .HasForeignKey(b => b.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of a category if it has associated books

            entity.HasOne(b => b.Author)
                  .WithMany(a => a.Books)
                  .HasForeignKey(b => b.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of an author if they have associated books
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50).UseCollation("SQL_Latin1_General_CP1_CI_AS"); 
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Biography).HasMaxLength(2000);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
}
