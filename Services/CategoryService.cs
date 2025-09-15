using BlazorAuthApp.Data;
using BlazorAuthApp.DTOs;
using BlazorAuthApp.Interfaces;
using BlazorAuthApp.Model;
using BlazorAuthApp.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace BlazorAuthApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted) // Add this filter
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy,
                    UpdatedBy = c.UpdatedBy
                })
                .ToListAsync();
        }

        public async Task<PagedResultDto<CategoryDto>> GetPagedCategoriesAsync(PagedRequestDto input)
        {
            var query = _context.Categories.Where(c => !c.IsDeleted).AsQueryable();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                input.Filter = input.Filter.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(input.Filter) ||
                    (c.Description != null && c.Description.ToLower().Contains(input.Filter)));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                // Simple sorting implementation - can be expanded
                if (input.Sorting.Contains("name", StringComparison.OrdinalIgnoreCase))
                {
                    query = input.Sorting.StartsWith("-")
                        ? query.OrderByDescending(c => c.Name)
                        : query.OrderBy(c => c.Name);
                }
                else if (input.Sorting.Contains("createdAt", StringComparison.OrdinalIgnoreCase))
                {
                    query = input.Sorting.StartsWith("-")
                        ? query.OrderByDescending(c => c.CreatedAt)
                        : query.OrderBy(c => c.CreatedAt);
                }
            }
            else
            {
                // Default sorting by name
                query = query.OrderBy(c => c.Name);
            }

            // Apply pagination
            var items = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedBy = c.CreatedBy,
                    UpdatedBy = c.UpdatedBy
                })
                .ToListAsync();

            return new PagedResultDto<CategoryDto>
            {
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted); // Add IsDeleted filter

            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.CreatedBy,
                UpdatedBy = category.UpdatedBy
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string? createdBy = null)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name.Trim(),
                Description = createCategoryDto.Description?.Trim(),
                IsActive = createCategoryDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.CreatedBy,
                UpdatedBy = category.UpdatedBy
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, string? updatedBy = null)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == updateCategoryDto.Id);

            if (category == null) return null;

            category.Name = updateCategoryDto.Name.Trim();
            category.Description = updateCategoryDto.Description?.Trim();
            category.IsActive = updateCategoryDto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.CreatedBy,
                UpdatedBy = category.UpdatedBy
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id, string? deletedBy = null)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null) return false;

            // Soft delete instead of hard delete
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            category.DeletedBy = deletedBy;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreCategoryAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);

            if (category == null) return false;

            category.IsDeleted = false;
            category.DeletedAt = null;
            category.DeletedBy = null;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id && !c.IsDeleted); // Add IsDeleted filter
        }

        public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories
                .Where(c => !c.IsDeleted) // Add IsDeleted filter
                .Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
