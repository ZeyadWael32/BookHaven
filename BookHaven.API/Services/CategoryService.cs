using AutoMapper;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;

namespace BookHaven.API.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<CategoryDto>> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(
            tracked: false,
            cancellationToken: cancellationToken,
            pageSize: pageSize,
            pageNumber: pageNumber);

        var totalCount = await _unitOfWork.Categories.CountAsync(cancellationToken: cancellationToken);

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
        var category = await _unitOfWork.Categories.GetAsync(
            c => c.Id == id, tracked: false, cancellationToken: cancellationToken);

        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(
        CategoryCreateDto createDto, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Categories.NameExistsAsync(createDto.Name, cancellationToken: cancellationToken))
            return ServiceResult<CategoryDto>.Failure(
                ServiceErrorType.Conflict,
                "Category name already exists",
                "A category with the provided name already exists.");

        var category = _mapper.Map<Category>(createDto);
        await _unitOfWork.Categories.CreateAsync(category, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        var created = await _unitOfWork.Categories.GetAsync(
            c => c.Id == category.Id, tracked: false, cancellationToken: cancellationToken);

        return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(created));
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(
        int id, CategoryUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetAsync(
            c => c.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<CategoryDto>.Failure(
                ServiceErrorType.NotFound,
                "Category not found",
                "The category with the provided ID does not exist.");

        if (await _unitOfWork.Categories.NameExistsAsync(updateDto.Name, id, cancellationToken: cancellationToken))
            return ServiceResult<CategoryDto>.Failure(
                ServiceErrorType.Conflict,
                "Category name already exists",
                "A category with the provided name already exists.");

        _mapper.Map(updateDto, category);
        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        var updated = await _unitOfWork.Categories.GetAsync(
            c => c.Id == category.Id, tracked: false, cancellationToken: cancellationToken);

        return ServiceResult<CategoryDto>.Success(_mapper.Map<CategoryDto>(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetAsync(
            c => c.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound,
                "Category not found",
                "The category with the provided ID does not exist.");

        if (await _unitOfWork.Categories.HasBooksAsync(id, cancellationToken))
            return ServiceResult<bool>.Failure(
                ServiceErrorType.Conflict,
                "Category has associated books",
                "Cannot delete a category that has associated books.");

        await _unitOfWork.Categories.RemoveAsync(category, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}