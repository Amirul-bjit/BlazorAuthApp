namespace BlazorAuthApp.DTOs
{
    public class BlogLikeDto
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string BlogTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime LikedAt { get; set; }
    }
}
