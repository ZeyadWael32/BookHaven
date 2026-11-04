using AutoMapper;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;

namespace BookHaven.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CategoryDto>> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(
            tracked: false,
            cancellationToken: cancellationToken,
            pageSize: pageSize,
            pageNumber: pageNumber);

        var totalCount = await _categoryRepository.CountAsync(cancellationToken: cancellationToken);

        return new PagedResponse<CategoryDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = _mapper.Map<IEnumerable<CategoryDto>>(categories)
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(
            c => c.Id == id, tracked: false, cancellationToken: cancellationToken);

        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(
        CategoryCreateDto createDto, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.NameExistsAsync(createDto.Name, cancellationToken: cancellationToken))
            return ServiceResult<CategoryDto>.Failure(
                ServiceErrorType.Conflict,
                "Category name already exists",
                "A category with the provided name already exists.");

        var category = _mapper.Map<Category>(createDto);
        await _categoryRepository.CreateAsync(category, cancellationToken);

        var created = await _categoryRepository.GetAsync(
            c => c.Id == category.Id, tracked: false, cancellationToken: cancellationToken);

        return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(created));
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(
        int id, CategoryUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(
            c => c.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<CategoryDto>.Failure(
                ServiceErrorType.NotFound,
                "Category not found",
                "The category with the provided ID does not exist.");

        if (await _categoryRepository.NameExistsAsync(updateDto.Name, id, cancellationToken: cancellationToken))
            return ServiceResult<CategoryDto>.Failure(
                ServiceErrorType.Conflict,
                "Category name already exists",
                "A category with the provided name already exists.");

        _mapper.Map(updateDto, category);
        await _categoryRepository.UpdateAsync(category, cancellationToken);

        var updated = await _categoryRepository.GetAsync(
            c => c.Id == category.Id, tracked: false, cancellationToken: cancellationToken);

        return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetAsync(
            c => c.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound,
                "Category not found",
                "The category with the provided ID does not exist.");

        if (await _categoryRepository.HasBooksAsync(id, cancellationToken))
            return ServiceResult<bool>.Failure(
                ServiceErrorType.Conflict,
                "Category has associated books",
                "Cannot delete a category that has associated books.");

        await _categoryRepository.RemoveAsync(category, cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}