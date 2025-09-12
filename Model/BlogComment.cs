using BlazorAuthApp.Data;
using System.ComponentModel.DataAnnotations;

namespace BlazorAuthApp.Model
{
    public class BlogComment
    {
        public int Id { get; set; }

        [Required]
        public int BlogId { get; set; }
        public Blog Blog { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required(ErrorMessage = "Comment content is required")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
