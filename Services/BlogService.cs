using BlazorAuthApp.Data;
using BlazorAuthApp.DTOs;
using BlazorAuthApp.Interfaces;
using BlazorAuthApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BlazorAuthApp.Services
{
    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _context;

        public BlogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BlogListDto>> GetAllBlogsAsync(string? currentUserId = null, bool includeUnpublished = false)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => !b.IsDeleted);

            if (!includeUnpublished)
            {
                query = query.Where(b => b.IsPublished);
            }

            var blogs = await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Summary = b.Summary,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    AuthorName = b.Author.UserName ?? b.Author.Email,
                    Categories = b.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive
                    }).ToList(),
                    IsPublished = b.IsPublished,
                    CreatedAt = b.CreatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount,
                    LikeCount = b.LikeCount,
                    EstimatedReadTime = b.EstimatedReadTime,
                    IsOwner = currentUserId != null && b.AuthorId == currentUserId,
                    Slug = b.Slug
                })
                .ToListAsync();

            return blogs;
        }

        public async Task<IEnumerable<BlogListDto>> GetBlogsByAuthorAsync(string authorId, string? currentUserId = null)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => b.AuthorId == authorId && !b.IsDeleted);

            // If not the owner, only show published blogs
            if (currentUserId != authorId)
            {
                query = query.Where(b => b.IsPublished);
            }

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Summary = b.Summary,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    AuthorName = b.Author.UserName ?? b.Author.Email,
                    Categories = b.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive
                    }).ToList(),
                    IsPublished = b.IsPublished,
                    CreatedAt = b.CreatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount,
                    LikeCount = b.LikeCount,
                    EstimatedReadTime = b.EstimatedReadTime,
                    IsOwner = currentUserId == authorId,
                    Slug = b.Slug
                })
                .ToListAsync();
        }

        public async Task<BlogDto?> GetBlogByIdAsync(int id, string? currentUserId = null)
        {
            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (blog == null) return null;

            // Check if user can access this blog
            if (!blog.IsPublished && blog.AuthorId != currentUserId)
                return null;

            return new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Summary = blog.Summary,
                FeaturedImageUrl = blog.FeaturedImageUrl,
                AuthorId = blog.AuthorId,
                AuthorName = blog.Author.UserName ?? blog.Author.Email,
                AuthorEmail = blog.Author.Email ?? "",
                Categories = blog.Categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList(),
                CategoryIds = blog.Categories.Select(c => c.Id).ToList(),
                IsPublished = blog.IsPublished,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                PublishedAt = blog.PublishedAt,
                MetaDescription = blog.MetaDescription,
                Slug = blog.Slug,
                ViewCount = blog.ViewCount,
                LikeCount = blog.LikeCount,
                EstimatedReadTime = blog.EstimatedReadTime,
                IsOwner = currentUserId != null && blog.AuthorId == currentUserId
            };
        }

        public async Task<BlogDto?> GetBlogBySlugAsync(string slug, string? currentUserId = null)
        {
            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Slug == slug && !b.IsDeleted);

            if (blog == null) return null;

            // Check if user can access this blog
            if (!blog.IsPublished && blog.AuthorId != currentUserId)
                return null;

            return new BlogDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Summary = blog.Summary,
                FeaturedImageUrl = blog.FeaturedImageUrl,
                AuthorId = blog.AuthorId,
                AuthorName = blog.Author.UserName ?? blog.Author.Email,
                AuthorEmail = blog.Author.Email ?? "",
                Categories = blog.Categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList(),
                CategoryIds = blog.Categories.Select(c => c.Id).ToList(),
                IsPublished = blog.IsPublished,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt,
                PublishedAt = blog.PublishedAt,
                MetaDescription = blog.MetaDescription,
                Slug = blog.Slug,
                ViewCount = blog.ViewCount,
                LikeCount = blog.LikeCount,
                EstimatedReadTime = blog.EstimatedReadTime,
                IsOwner = currentUserId != null && blog.AuthorId == currentUserId
            };
        }

        public async Task<BlogDto> CreateBlogAsync(CreateBlogDto createBlogDto, string authorId)
        {
            var categories = await _context.Categories
                .Where(c => createBlogDto.CategoryIds.Contains(c.Id))
                .ToListAsync();

            var slug = await GenerateSlugAsync(createBlogDto.Title);
            var estimatedReadTime = CalculateReadTime(createBlogDto.Content);

            var blog = new Blog
            {
                Title = createBlogDto.Title.Trim(),
                Content = createBlogDto.Content.Trim(),
                Summary = createBlogDto.Summary?.Trim(),
                FeaturedImageUrl = createBlogDto.FeaturedImageUrl?.Trim(),
                AuthorId = authorId,
                Categories = categories,
                IsPublished = createBlogDto.IsPublished,
                MetaDescription = createBlogDto.MetaDescription?.Trim(),
                Slug = slug,
                EstimatedReadTime = estimatedReadTime,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = createBlogDto.IsPublished ? DateTime.UtcNow : null
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            // Reload to get author information
            return await GetBlogByIdAsync(blog.Id) ?? throw new InvalidOperationException("Failed to create blog");
        }

        public async Task<BlogDto?> UpdateBlogAsync(UpdateBlogDto updateBlogDto, string currentUserId)
        {
            var blog = await _context.Blogs
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == updateBlogDto.Id && !b.IsDeleted);

            if (blog == null || blog.AuthorId != currentUserId) return null;

            var categories = await _context.Categories
                .Where(c => updateBlogDto.CategoryIds.Contains(c.Id))
                .ToListAsync();

            var wasPublished = blog.IsPublished;
            var estimatedReadTime = CalculateReadTime(updateBlogDto.Content);

            // Update slug if title changed
            if (blog.Title != updateBlogDto.Title)
            {
                blog.Slug = await GenerateSlugAsync(updateBlogDto.Title, blog.Id);
            }

            blog.Title = updateBlogDto.Title.Trim();
            blog.Content = updateBlogDto.Content.Trim();
            blog.Summary = updateBlogDto.Summary?.Trim();
            blog.FeaturedImageUrl = updateBlogDto.FeaturedImageUrl?.Trim();
            blog.Categories.Clear();
            foreach (var category in categories)
            {
                blog.Categories.Add(category);
            }
            blog.IsPublished = updateBlogDto.IsPublished;
            blog.MetaDescription = updateBlogDto.MetaDescription?.Trim();
            blog.EstimatedReadTime = estimatedReadTime;
            blog.UpdatedAt = DateTime.UtcNow;

            // Set published date if publishing for the first time
            if (!wasPublished && updateBlogDto.IsPublished)
            {
                blog.PublishedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetBlogByIdAsync(blog.Id);
        }

        public async Task<bool> DeleteBlogAsync(int id, string currentUserId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (blog == null || blog.AuthorId != currentUserId) return false;

            blog.IsDeleted = true;
            blog.DeletedAt = DateTime.UtcNow;
            blog.DeletedBy = currentUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreBlogAsync(int id, string currentUserId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted);

            if (blog == null || blog.AuthorId != currentUserId) return false;

            blog.IsDeleted = false;
            blog.DeletedAt = null;
            blog.DeletedBy = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishBlogAsync(int id, string currentUserId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (blog == null || blog.AuthorId != currentUserId) return false;

            if (!blog.IsPublished)
            {
                blog.IsPublished = true;
                blog.PublishedAt = DateTime.UtcNow;
                blog.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UnpublishBlogAsync(int id, string currentUserId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (blog == null || blog.AuthorId != currentUserId) return false;

            blog.IsPublished = false;
            blog.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementViewCountAsync(int id)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted && b.IsPublished);

            if (blog == null) return false;

            blog.ViewCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleLikeAsync(int id, string userId)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted && b.IsPublished);

            if (blog == null) return false;

            // In a real application, you would track individual likes in a separate table
            // For now, we'll just increment the count
            blog.LikeCount = Math.Max(0, blog.LikeCount + 1);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BlogExistsAsync(int id)
        {
            return await _context.Blogs.AnyAsync(b => b.Id == id && !b.IsDeleted);
        }

        public async Task<bool> IsUserOwnerAsync(int id, string userId)
        {
            return await _context.Blogs.AnyAsync(b => b.Id == id && b.AuthorId == userId && !b.IsDeleted);
        }

        public async Task<string> GenerateSlugAsync(string title, int? excludeId = null)
        {
            var baseSlug = GenerateSlug(title);
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        public async Task<IEnumerable<BlogListDto>> SearchBlogsAsync(string searchTerm, string? currentUserId = null)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => !b.IsDeleted && b.IsPublished);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b =>
                    b.Title.Contains(searchTerm) ||
                    b.Content.Contains(searchTerm) ||
                    b.Summary!.Contains(searchTerm) ||
                    b.Categories.Any(c => c.Name.Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Summary = b.Summary,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    AuthorName = b.Author.UserName ?? b.Author.Email,
                    Categories = b.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive
                    }).ToList(),
                    IsPublished = b.IsPublished,
                    CreatedAt = b.CreatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount,
                    LikeCount = b.LikeCount,
                    EstimatedReadTime = b.EstimatedReadTime,
                    IsOwner = currentUserId != null && b.AuthorId == currentUserId,
                    Slug = b.Slug
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogListDto>> GetBlogsByCategoryAsync(int categoryId, string? currentUserId = null)
        {
            return await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => !b.IsDeleted && b.IsPublished && b.Categories.Any(c => c.Id == categoryId))
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Summary = b.Summary,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    AuthorName = b.Author.UserName ?? b.Author.Email,
                    Categories = b.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive
                    }).ToList(),
                    IsPublished = b.IsPublished,
                    CreatedAt = b.CreatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount,
                    LikeCount = b.LikeCount,
                    EstimatedReadTime = b.EstimatedReadTime,
                    IsOwner = currentUserId != null && b.AuthorId == currentUserId,
                    Slug = b.Slug
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogListDto>> GetRecentBlogsAsync(int count = 10, string? currentUserId = null)
        {
            return await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => !b.IsDeleted && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .Take(count)
                .Select(b => new BlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Summary = b.Summary,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    AuthorName = b.Author.UserName ?? b.Author.Email,
                    Categories = b.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive
                    }).ToList(),
                    IsPublished = b.IsPublished,
                    CreatedAt = b.CreatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount,
                    LikeCount = b.LikeCount,
                    EstimatedReadTime = b.EstimatedReadTime,
                    IsOwner = currentUserId != null && b.AuthorId == currentUserId,
                    Slug = b.Slug
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogListDto>> GetPopularBlogsAsync(int count = 10, string? currentUserId = null)
        {
            return await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => !b.IsDeleted && b.IsPublished)
                .OrderByDescending(b => b.ViewCount)
                .ThenByDescending(b => b.LikeCount)
                .Take(count)
                .Select(b => new BlogListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Summary = b.Summary,
                    FeaturedImageUrl = b.FeaturedImageUrl,
                    AuthorName = b.Author.UserName ?? b.Author.Email,
                    Categories = b.Categories.Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive
                    }).ToList(),
                    IsPublished = b.IsPublished,
                    CreatedAt = b.CreatedAt,
                    PublishedAt = b.PublishedAt,
                    ViewCount = b.ViewCount,
                    LikeCount = b.LikeCount,
                    EstimatedReadTime = b.EstimatedReadTime,
                    IsOwner = currentUserId != null && b.AuthorId == currentUserId,
                    Slug = b.Slug
                })
                .ToListAsync();
        }

        // Private helper methods
        private async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
        {
            var query = _context.Blogs.Where(b => b.Slug == slug && !b.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(b => b.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        private static string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "untitled";

            // Convert to lowercase
            var slug = title.ToLowerInvariant();

            // Remove special characters and replace with hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", " ").Trim();
            slug = Regex.Replace(slug, @"\s", "-");

            // Remove consecutive hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            // Limit length
            if (slug.Length > 50)
            {
                slug = slug.Substring(0, 50).TrimEnd('-');
            }

            return string.IsNullOrEmpty(slug) ? "untitled" : slug;
        }

        private static int CalculateReadTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return 1;

            // Average reading speed is about 200-250 words per minute
            // We'll use 200 for a conservative estimate
            var wordCount = content.Split(new char[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;

            var readTime = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
            return readTime;
        }
    }
}