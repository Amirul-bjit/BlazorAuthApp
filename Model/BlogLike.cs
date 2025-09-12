using BlazorAuthApp.Data;
using System.ComponentModel.DataAnnotations;

namespace BlazorAuthApp.Model
{
    public class BlogLike
    {
        public int Id { get; set; }

        [Required]
        public int BlogId { get; set; }
        public Blog Blog { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
