using BlazorAuthApp.DTOs;

namespace BlazorAuthApp.Interfaces
{
    public interface IBlogLikeService
    {
        Task<bool> ToggleLikeAsync(int blogId, string userId);
        Task<bool> IsLikedByUserAsync(int blogId, string userId);
        Task<int> GetLikeCountAsync(int blogId);
        Task<IEnumerable<BlogLikeDto>> GetBlogLikesAsync(int blogId, string requesterId);
    }
}
