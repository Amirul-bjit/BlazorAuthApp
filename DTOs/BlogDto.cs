using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.ComponentModel.DataAnnotations;

namespace BlazorAuthApp.DTOs
{
    public class BlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
        public List<int> CategoryIds { get; set; } = new();
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? MetaDescription { get; set; }
        public string? Slug { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int EstimatedReadTime { get; set; }
        public bool IsOwner { get; set; } = false;
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; } = false;
        public DateTime CreatedAtLoal => CreatedAt.ToLocalTime();
        public DateTime? UpdatedAtLocal => UpdatedAt?.ToLocalTime();
        public DateTime? PublishedAtLocal => PublishedAt?.ToLocalTime();
        public List<BlogCommentDto> Comments { get; set; } = new(); // Only visible to author
        public List<BlogLikeDto> Likes { get; set; } = new(); // Only visible to author
    }

    public class CreateBlogDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 10000 characters")]
        public string Content { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Summary cannot exceed 300 characters")]
        public string? Summary { get; set; }

        [StringLength(500, ErrorMessage = "Featured image URL cannot exceed 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? FeaturedImageUrl { get; set; }

        [Required(ErrorMessage = "Please select at least one category")]
        [MinLength(1, ErrorMessage = "Please select at least one category")]
        public List<int> CategoryIds { get; set; } = new();

        public bool IsPublished { get; set; } = false;

        [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
        public string? MetaDescription { get; set; }
    }

    public class UpdateBlogDto
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
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? FeaturedImageUrl { get; set; }

        [Required(ErrorMessage = "Please select at least one category")]
        [MinLength(1, ErrorMessage = "Please select at least one category")]
        public List<int> CategoryIds { get; set; } = new();

        public bool IsPublished { get; set; }

        [StringLength(160, ErrorMessage = "Meta description cannot exceed 160 characters")]
        public string? MetaDescription { get; set; }
    }

    public class BlogListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public List<CategoryDto> Categories { get; set; } = new();
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int EstimatedReadTime { get; set; }
        public bool IsOwner { get; set; } = false;
        public string? Slug { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; } = false;
        public DateTime CreatedAtLoal => CreatedAt.ToLocalTime();
        public DateTime? PublishedAtLocal => PublishedAt?.ToLocalTime();
    }
}
