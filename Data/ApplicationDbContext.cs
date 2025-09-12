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
            });
        }
    }
}
