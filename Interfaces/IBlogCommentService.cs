using BlazorAuthApp.DTOs;

namespace BlazorAuthApp.Interfaces
{
    public interface IBlogCommentService
    {
        Task<BlogCommentDto?> CreateCommentAsync(CreateCommentDto createCommentDto, string userId);
        Task<BlogCommentDto?> UpdateCommentAsync(UpdateCommentDto updateCommentDto, string userId);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        Task<IEnumerable<BlogCommentDto>> GetBlogCommentsAsync(int blogId, string? currentUserId = null);
        Task<IEnumerable<BlogCommentDto>> GetBlogCommentsForAuthorAsync(int blogId, string authorId, string requesterId);
        Task<int> GetCommentCountAsync(int blogId);
    }
}
