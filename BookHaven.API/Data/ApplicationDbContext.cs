using BookHaven.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
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

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.UserId).IsUnique(); // Ensure one shopping cart per user

            entity.HasMany(sc => sc.Items)
                  .WithOne(ci => ci.ShoppingCart)
                  .HasForeignKey(ci => ci.ShoppingCartId)
                  .OnDelete(DeleteBehavior.Cascade); // Cascade delete cart items when a shopping cart is deleted
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.Property(e => e.Quantity).IsRequired();
            entity.HasIndex(e => new { e.ShoppingCartId, e.BookId }).IsUnique(); // Ensure one cart item per book in a shopping cart (composite unique index for book and shopping cart together)
            entity.HasOne(ci => ci.Book)
                  .WithMany()
                  .HasForeignKey(ci => ci.BookId)
                  .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of a book if it is in any cart
        });
    }
}
