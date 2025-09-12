using BlazorAuthApp.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlazorAuthApp.Model
{
    public class Blog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 10000 characters")]
        public string Content { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Summary cannot exceed 300 characters")]
        public string? Summary { get; set; }

        [StringLength(500, ErrorMessage = "Featured image URL cannot exceed 500 characters")]
        public string? FeaturedImageUrl { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        // Navigation property for the author
        public ApplicationUser Author { get; set; } = null!;

        // Many-to-many relationship with Categories
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public string? DeletedBy { get; set; }

        // SEO and metadata
        [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
        public string? MetaDescription { get; set; }

        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters")]
        public string? Slug { get; set; }

        // Engagement metrics
        public int ViewCount { get; set; } = 0;

        public int LikeCount { get; set; } = 0;

        // Reading time in minutes (calculated)
        public int EstimatedReadTime { get; set; } = 1;
    }
}
