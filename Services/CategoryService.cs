using BlazorAuthApp.Data;
using BlazorAuthApp.DTOs;
using BlazorAuthApp.Interfaces;
using BlazorAuthApp.Model;
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

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

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

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
