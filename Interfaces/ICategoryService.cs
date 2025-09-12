using BlazorAuthApp.DTOs;

namespace BlazorAuthApp.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string? createdBy = null);
        Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, string? updatedBy = null);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);
    }
}
