using BookHaven.API.Models.Dto;

namespace BookHaven.API.Services.IServices;

public interface IAuthorService
{
    Task<PagedResponse<AuthorDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<AuthorDto>> CreateAsync(AuthorCreateDto createDto, CancellationToken cancellationToken);
    Task<ServiceResult<AuthorDto>> UpdateAsync(int id, AuthorUpdateDto updateDto, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<BookDto>> UploadAuthorPictureAsync(int id, IFormFile file, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAuthorPictureAsync(int id, CancellationToken cancellationToken);
}