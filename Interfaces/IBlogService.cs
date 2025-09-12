using BlazorAuthApp.DTOs;

namespace BlazorAuthApp.Interfaces
{
    public interface IBlogService
    {
        // Get methods
        Task<IEnumerable<BlogListDto>> GetAllBlogsAsync(string? currentUserId = null, bool includeUnpublished = false);
        Task<IEnumerable<BlogListDto>> GetBlogsByAuthorAsync(string authorId, string? currentUserId = null);
        Task<BlogDto?> GetBlogByIdAsync(int id, string? currentUserId = null);
        Task<BlogDto?> GetBlogBySlugAsync(string slug, string? currentUserId = null);

        // CRUD operations
        Task<BlogDto> CreateBlogAsync(CreateBlogDto createBlogDto, string authorId);
        Task<BlogDto?> UpdateBlogAsync(UpdateBlogDto updateBlogDto, string currentUserId);
        Task<bool> DeleteBlogAsync(int id, string currentUserId);
        Task<bool> RestoreBlogAsync(int id, string currentUserId);

        // Publishing
        Task<bool> PublishBlogAsync(int id, string currentUserId);
        Task<bool> UnpublishBlogAsync(int id, string currentUserId);

        // Engagement
        Task<bool> IncrementViewCountAsync(int id);
        Task<bool> ToggleLikeAsync(int id, string userId);

        // Validation and utility
        Task<bool> BlogExistsAsync(int id);
        Task<bool> IsUserOwnerAsync(int id, string userId);
        Task<string> GenerateSlugAsync(string title, int? excludeId = null);

        // Search and filtering
        Task<IEnumerable<BlogListDto>> SearchBlogsAsync(string searchTerm, string? currentUserId = null);
        Task<IEnumerable<BlogListDto>> GetBlogsByCategoryAsync(int categoryId, string? currentUserId = null);
        Task<IEnumerable<BlogListDto>> GetRecentBlogsAsync(int count = 10, string? currentUserId = null);
        Task<IEnumerable<BlogListDto>> GetPopularBlogsAsync(int count = 10, string? currentUserId = null);
    }
}
