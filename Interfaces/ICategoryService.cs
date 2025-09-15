using BlazorAuthApp.DTOs;
using BlazorAuthApp.Shared.Pagination;

namespace BlazorAuthApp.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedResultDto<CategoryDto>> GetPagedCategoriesAsync(PagedRequestDto input);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(); // Keep for dropdown lists
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, string? createdBy = null);
        Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, string? updatedBy = null);
        Task<bool> DeleteCategoryAsync(int id, string? deletedBy = null);
        Task<bool> RestoreCategoryAsync(int id); // Optional
        Task<bool> CategoryExistsAsync(int id);
    }
}
