using BlazorAuthApp.Data;
using BlazorAuthApp.DTOs;
using BlazorAuthApp.Interfaces;
using BlazorAuthApp.Model;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuthApp.Services
{
    public class BlogLikeService : IBlogLikeService
    {
        private readonly ApplicationDbContext _context;

        public BlogLikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleLikeAsync(int blogId, string userId)
        {
            // Check if blog exists and is published
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId && !b.IsDeleted && b.IsPublished);

            if (blog == null) return false;

            // Check if user already liked this blog
            var existingLike = await _context.BlogLikes
                .FirstOrDefaultAsync(bl => bl.BlogId == blogId && bl.UserId == userId);

            if (existingLike != null)
            {
                // Unlike - remove the like
                _context.BlogLikes.Remove(existingLike);
                blog.LikeCount = Math.Max(0, blog.LikeCount - 1);
            }
            else
            {
                // Like - add new like
                var newLike = new BlogLike
                {
                    BlogId = blogId,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                _context.BlogLikes.Add(newLike);
                blog.LikeCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsLikedByUserAsync(int blogId, string userId)
        {
            return await _context.BlogLikes
                .AnyAsync(bl => bl.BlogId == blogId && bl.UserId == userId);
        }

        public async Task<int> GetLikeCountAsync(int blogId)
        {
            return await _context.BlogLikes
                .CountAsync(bl => bl.BlogId == blogId);
        }

        public async Task<IEnumerable<BlogLikeDto>> GetBlogLikesAsync(int blogId, string requesterId)
        {
            // Check if requester is the blog author
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId && !b.IsDeleted);

            if (blog == null || blog.AuthorId != requesterId)
            {
                return Enumerable.Empty<BlogLikeDto>();
            }

            return await _context.BlogLikes
                .Include(bl => bl.User)
                .Include(bl => bl.Blog)
                .Where(bl => bl.BlogId == blogId)
                .Select(bl => new BlogLikeDto
                {
                    Id = bl.Id,
                    BlogId = bl.BlogId,
                    BlogTitle = bl.Blog.Title,
                    UserId = bl.UserId,
                    UserName = bl.User.UserName ?? bl.User.Email,
                    UserEmail = bl.User.Email,
                    LikedAt = bl.LikedAt
                })
                .OrderByDescending(bl => bl.LikedAt)
                .ToListAsync();
        }
    }
}
