using BlazorAuthApp.Data;
using BlazorAuthApp.DTOs;
using BlazorAuthApp.Interfaces;
using BlazorAuthApp.Model;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuthApp.Services
{
    public class BlogCommentService : IBlogCommentService
    {
        private readonly ApplicationDbContext _context;

        public BlogCommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BlogCommentDto?> CreateCommentAsync(CreateCommentDto createCommentDto, string userId)
        {
            // Check if blog exists and is published
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == createCommentDto.BlogId && !b.IsDeleted && b.IsPublished);

            if (blog == null) return null;

            var comment = new BlogComment
            {
                BlogId = createCommentDto.BlogId,
                UserId = userId,
                Content = createCommentDto.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogComments.Add(comment);
            await _context.SaveChangesAsync();

            // Return the created comment
            return await _context.BlogComments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Where(c => c.Id == comment.Id)
                .Select(c => new BlogCommentDto
                {
                    Id = c.Id,
                    BlogId = c.BlogId,
                    BlogTitle = c.Blog.Title,
                    UserId = c.UserId,
                    UserName = c.User.UserName ?? c.User.Email,
                    UserEmail = c.User.Email,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsOwner = c.UserId == userId,
                    CanEdit = c.UserId == userId
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BlogCommentDto?> UpdateCommentAsync(UpdateCommentDto updateCommentDto, string userId)
        {
            var comment = await _context.BlogComments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .FirstOrDefaultAsync(c => c.Id == updateCommentDto.Id && !c.IsDeleted);

            if (comment == null || comment.UserId != userId) return null;

            comment.Content = updateCommentDto.Content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new BlogCommentDto
            {
                Id = comment.Id,
                BlogId = comment.BlogId,
                BlogTitle = comment.Blog.Title,
                UserId = comment.UserId,
                UserName = comment.User.UserName ?? comment.User.Email,
                UserEmail = comment.User.Email,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                IsOwner = true,
                CanEdit = true
            };
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null) return false;

            // Allow deletion by comment owner or blog author
            if (comment.UserId != userId && comment.Blog.AuthorId != userId) return false;

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            comment.DeletedBy = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BlogCommentDto>> GetBlogCommentsAsync(int blogId, string? currentUserId = null)
        {
            return await _context.BlogComments
                .Include(c => c.User)
                .Include(c => c.Blog)
                .Where(c => c.BlogId == blogId && !c.IsDeleted)
                .Select(c => new BlogCommentDto
                {
                    Id = c.Id,
                    BlogId = c.BlogId,
                    BlogTitle = c.Blog.Title,
                    UserId = c.UserId,
                    UserName = c.User.UserName ?? c.User.Email,
                    UserEmail = c.User.Email,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsOwner = currentUserId != null && c.UserId == currentUserId,
                    CanEdit = currentUserId != null && (c.UserId == currentUserId || c.Blog.AuthorId == currentUserId)
                })
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogCommentDto>> GetBlogCommentsForAuthorAsync(int blogId, string authorId, string requesterId)
        {
            // Check if requester is the blog author
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == blogId && !b.IsDeleted);

            if (blog == null || blog.AuthorId != requesterId)
            {
                return Enumerable.Empty<BlogCommentDto>();
            }

            return await GetBlogCommentsAsync(blogId, requesterId);
        }

        public async Task<int> GetCommentCountAsync(int blogId)
        {
            return await _context.BlogComments
                .CountAsync(c => c.BlogId == blogId && !c.IsDeleted);
        }
    }
}
