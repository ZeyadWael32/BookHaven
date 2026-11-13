using BookHaven.API.Models.Dto;
using BookHaven.API.Models.Dto.Category;

namespace BookHaven.API.Services.IServices;

public interface ICategoryService
{
    Task<PagedResponse<CategoryDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<CategoryDto>> CreateAsync(CategoryCreateDto createDto, CancellationToken cancellationToken);
    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryUpdateDto updateDto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}