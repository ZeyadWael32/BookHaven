using AutoMapper;
using BookHaven.API.Infrastructure.Storage;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;

namespace BookHaven.API.Services;

public class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public AuthorService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResponse<AuthorDto>> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var authors = await _unitOfWork.Authors.GetAllAsync(
            tracked: false,
            cancellationToken: cancellationToken,
            pageSize: pageSize,
            pageNumber: pageNumber);

        var totalCount = await _unitOfWork.Authors.CountAsync(cancellationToken: cancellationToken);

        return new PagedResponse<AuthorDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = _mapper.Map<IEnumerable<AuthorDto>>(authors)
        };
    }

    public async Task<AuthorDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Authors.GetAsync(
            c => c.Id == id, tracked: false, cancellationToken: cancellationToken);

        return author is null ? null : _mapper.Map<AuthorDto>(author);
    }

    public async Task<ServiceResult<AuthorDto>> CreateAsync(
        AuthorCreateDto createDto, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Authors.NameExistsAsync(createDto.Name, cancellationToken: cancellationToken))
            return ServiceResult<AuthorDto>.Failure(
                ServiceErrorType.Conflict,
                "Author name already exists",
                "An author with the provided name already exists.");

        var author = _mapper.Map<Author>(createDto);
        await _unitOfWork.Authors.CreateAsync(author, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        var created = await _unitOfWork.Authors.GetAsync(
            c => c.Id == author.Id, tracked: false, cancellationToken: cancellationToken);

        return ServiceResult<AuthorDto>.Success(_mapper.Map<AuthorDto>(created));
    }

    public async Task<ServiceResult<AuthorDto>> UpdateAsync(
        int id, AuthorUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Authors.GetAsync(
            c => c.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (author is null)
            return ServiceResult<AuthorDto>.Failure(
                ServiceErrorType.NotFound,
                "Author not found",
                "The author with the provided ID does not exist.");

        _mapper.Map(updateDto, author);
        await _unitOfWork.Authors.UpdateAsync(author, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        var updated = await _unitOfWork.Authors.GetAsync(
            c => c.Id == author.Id, tracked: false, cancellationToken: cancellationToken);

        return ServiceResult<AuthorDto>.Success(_mapper.Map<AuthorDto>(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Authors.GetAsync(
            c => c.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (author is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound,
                "Author not found",
                "The author with the provided ID does not exist.");

        if (await _unitOfWork.Authors.HasBooksAsync(id, cancellationToken))
            return ServiceResult<bool>.Failure(
                ServiceErrorType.Conflict,
                "Author has associated books",
                "Cannot delete an author that has associated books.");

        var oldPath = author.ProfileImageUrl;
        await _unitOfWork.Authors.RemoveAsync(author, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        if (!string.IsNullOrEmpty(oldPath))
        {
            _fileStorageService.Delete(oldPath); // Only after database update is successful, delete the old file to avoid orphaned files
        }

        return ServiceResult<bool>.Success(true);
    }

    #region File Storage Operations

    public async Task<ServiceResult<AuthorDto>> UploadAuthorCoverAsync(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Authors.GetAsync(a => a.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (author is null)
            return ServiceResult<AuthorDto>.Failure(
                ServiceErrorType.NotFound, "Author not found", "The author with the provided ID does not exist.");

        var (extension, errorMessage) = await ImageValidator.ValidateImageAsync(file, cancellationToken); // deconstructuring the tuple returned by ValidateImageAsync into extension and errorMessage

        if (errorMessage is not null)
            return ServiceResult<AuthorDto>.Failure(
                ServiceErrorType.Validation, "Invalid file", errorMessage);

        var oldPath = author.ProfileImageUrl;

        string newPath;
        await using (var stream = file.OpenReadStream())
        {
            newPath = await _fileStorageService.SaveAsync(stream, extension, "author-pictures", cancellationToken);
        }

        try
        {
            author.ProfileImageUrl = newPath;
            await _unitOfWork.Authors.UpdateAsync(author, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
        catch
        {
            // If saving to the database fails, delete the newly uploaded file to avoid orphaned files
            _fileStorageService.Delete(newPath); // always local path
            throw;
        }

        // if old path is null skip deletion, otherwise delete the old file
        if (!string.IsNullOrEmpty(oldPath))
        {
            _fileStorageService.Delete(oldPath); // Only after database update is successful, delete the old file to avoid orphaned files
        }

        var updatedAuthor = await _unitOfWork.Authors.GetAsync(a => a.Id == id, tracked: false, includeProperties: "ProfileImage", cancellationToken: cancellationToken);
        return ServiceResult<AuthorDto>.Success(_mapper.Map<AuthorDto>(updatedAuthor));
    }

    public async Task<ServiceResult<bool>> DeleteAuthorPictureAsync(int id, CancellationToken cancellationToken)
    {
        var author = await _unitOfWork.Authors.GetAsync(a => a.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (author is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound, "Author not found", "The author with the provided ID does not exist.");

        var oldPath = author.ProfileImageUrl;
        if (string.IsNullOrEmpty(oldPath))
            return ServiceResult<bool>.Success(true); // No profile image to delete

        author.ProfileImageUrl = null; // Set the profile image URL to null before deleting the file

        await _unitOfWork.Authors.UpdateAsync(author, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        _fileStorageService.Delete(oldPath); // Only after database update is successful, delete the old file to avoid orphaned files

        return ServiceResult<bool>.Success(true);
    }

    #endregion
}