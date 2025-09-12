namespace BlazorAuthApp.DTOs
{
    public class BlogCommentDto
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string BlogTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsOwner { get; set; } = false; // True if current user is the commenter
        public bool CanEdit { get; set; } = false; // True if current user can edit this comment
        public DateTime CreatedAtLocal => CreatedAt.ToLocalTime();
        public DateTime? UpdatedAtLocal => UpdatedAt?.ToLocalTime();
    }

    public class CreateCommentDto
    {
        public int BlogId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
