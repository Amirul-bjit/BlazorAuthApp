using BlazorAuthApp.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuthApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogLike> BlogLikes { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50);
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50);

                // Create index for faster queries
                entity.HasIndex(e => e.Name)
                    .IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Blog entity
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(10000);

                entity.Property(e => e.Summary)
                    .HasMaxLength(300);

                entity.Property(e => e.FeaturedImageUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.MetaDescription)
                    .HasMaxLength(160);

                entity.Property(e => e.Slug)
                    .HasMaxLength(100);

                entity.Property(e => e.DeletedBy)
                    .HasMaxLength(50);

                // Configure relationships
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure many-to-many relationship with Categories
                entity.HasMany(e => e.Categories)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "BlogCategory",
                        j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                        j => j.HasOne<Blog>().WithMany().HasForeignKey("BlogId"),
                        j =>
                        {
                            j.HasKey("BlogId", "CategoryId");
                            j.ToTable("BlogCategories");
                        });

                // Create indexes for better performance
                entity.HasIndex(e => e.AuthorId);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => e.Slug)
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = false"); // Only unique among non-deleted blogs
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.PublishedAt);
                entity.HasIndex(e => e.ViewCount);
                entity.HasIndex(e => e.LikeCount);

                // Configure default values
                entity.Property(e => e.IsPublished)
                    .HasDefaultValue(false);
                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);
                entity.Property(e => e.ViewCount)
                    .HasDefaultValue(0);
                entity.Property(e => e.LikeCount)
                    .HasDefaultValue(0);
                entity.Property(e => e.EstimatedReadTime)
                    .HasDefaultValue(1);

                // Navigation properties for likes and comments
                entity.HasMany(b => b.Likes)
                    .WithOne(bl => bl.Blog)
                    .HasForeignKey(bl => bl.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(b => b.Comments)
                    .WithOne(bc => bc.Blog)
                    .HasForeignKey(bc => bc.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // BlogLike entity configuration
            modelBuilder.Entity<BlogLike>(entity =>
            {
                entity.HasKey(bl => bl.Id);

                // Foreign key to Blog
                entity.HasOne(bl => bl.Blog)
                    .WithMany(b => b.Likes)
                    .HasForeignKey(bl => bl.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to User
                entity.HasOne(bl => bl.User)
                    .WithMany()
                    .HasForeignKey(bl => bl.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Composite unique index to prevent duplicate likes
                entity.HasIndex(bl => new { bl.BlogId, bl.UserId })
                    .IsUnique()
                    .HasDatabaseName("IX_BlogLikes_BlogId_UserId");

                // Index for performance
                entity.HasIndex(bl => bl.BlogId)
                    .HasDatabaseName("IX_BlogLikes_BlogId");
            });

            // BlogComment entity configuration
            modelBuilder.Entity<BlogComment>(entity =>
            {
                entity.HasKey(bc => bc.Id);

                // Foreign key to Blog
                entity.HasOne(bc => bc.Blog)
                    .WithMany(b => b.Comments)
                    .HasForeignKey(bc => bc.BlogId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to User
                entity.HasOne(bc => bc.User)
                    .WithMany()
                    .HasForeignKey(bc => bc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                entity.HasIndex(bc => bc.BlogId)
                    .HasDatabaseName("IX_BlogComments_BlogId");

                entity.HasIndex(bc => bc.UserId)
                    .HasDatabaseName("IX_BlogComments_UserId");

                entity.HasIndex(bc => bc.IsDeleted)
                    .HasDatabaseName("IX_BlogComments_IsDeleted");

                entity.HasIndex(bc => new { bc.BlogId, bc.IsDeleted })
                    .HasDatabaseName("IX_BlogComments_BlogId_IsDeleted");
            });

        }
    }
}
