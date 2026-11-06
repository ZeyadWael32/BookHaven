using AutoMapper;
using BookHaven.API.Infrastructure.Storage;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto;
using BookHaven.API.Models.Queries;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;

namespace BookHaven.API.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;

    public BookService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResponse<BookDto>> GetPagedAsync(BookQueryParameters query, CancellationToken cancellationToken)
    {
        var filter = query.ToFilter();

        var books = await _unitOfWork.Books.GetAllAsync(
            filter: filter,
            tracked: false,
            includeProperties: "Author,Category",
            cancellationToken: cancellationToken,
            pageSize: query.PageSize,
            pageNumber: query.PageNumber);

        var totalCount = await _unitOfWork.Books.CountAsync(filter, cancellationToken);

        return new PagedResponse<BookDto>
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
            Items = _mapper.Map<IEnumerable<BookDto>>(books)
        };
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetAsync(u => u.Id == id, tracked: false, includeProperties: "Author,Category", cancellationToken: cancellationToken);
        return book is null ? null : _mapper.Map<BookDto>(book);
    }

    public async Task<ServiceResult<BookDto>> CreateAsync(BookCreateDto createDto, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetAsync(
            c => c.Id == createDto.CategoryId, tracked: false, cancellationToken: cancellationToken);

        var author = await _unitOfWork.Authors.GetAsync(
            a => a.Id == createDto.AuthorId, tracked: false, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid category", "The provided category ID is invalid.");

        if (author is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid author", "The provided author ID is invalid.");

        if (await _unitOfWork.Books.IsbnExistsAsync(createDto.Isbn, cancellationToken: cancellationToken))
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Conflict, "ISBN already exists", "A book with the provided ISBN already exists.");

        var book = _mapper.Map<Book>(createDto);
        await _unitOfWork.Books.CreateAsync(book, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        var created = await _unitOfWork.Books.GetAsync(
            b => b.Id == book.Id, tracked: false, includeProperties: "Author,Category", cancellationToken: cancellationToken);

        return ServiceResult<BookDto>.Success(_mapper.Map<BookDto>(created));
    }

    public async Task<ServiceResult<BookDto>> UpdateAsync(int id, BookUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var bookToUpdate = await _unitOfWork.Books.GetAsync(u => u.Id == id, tracked: true, includeProperties: "Author,Category", cancellationToken: cancellationToken);

        if (bookToUpdate is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.NotFound, "Book not found", "The book with the provided ID does not exist.");

        var category = await _unitOfWork.Categories.GetAsync(c => c.Id == updateDto.CategoryId, tracked: false, cancellationToken: cancellationToken);

        var author = await _unitOfWork.Authors.GetAsync(a => a.Id == updateDto.AuthorId, tracked: false, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid category", "The provided category ID is invalid.");

        if (author is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid author", "The provided author ID is invalid.");

        if (await _unitOfWork.Books.IsbnExistsAsync(updateDto.Isbn, excludeId: id, cancellationToken: cancellationToken))
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Conflict, "ISBN already exists", "A book with the provided ISBN already exists.");

        _mapper.Map(updateDto, bookToUpdate); // Map the updateDto to the existing book entity
        await _unitOfWork.Books.UpdateAsync(bookToUpdate, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        var updated = await _unitOfWork.Books.GetAsync(
            b => b.Id == bookToUpdate.Id, tracked: false, includeProperties: "Author,Category", cancellationToken: cancellationToken);
        return ServiceResult<BookDto>.Success(_mapper.Map<BookDto>(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var bookToDelete = await _unitOfWork.Books.GetAsync(u => u.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (bookToDelete is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound, "Book not found", "The book with the provided ID does not exist.");

        var coverImagePath = bookToDelete.CoverImageUrl; // Store the cover image path before deletion to delete the file after the database operation is successful
        await _unitOfWork.Books.RemoveAsync(bookToDelete, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);

        if (!string.IsNullOrEmpty(coverImagePath))
        {
            _fileStorageService.Delete(coverImagePath); // Only after database deletion is successful, delete the cover image to avoid orphaned files
        }

        return ServiceResult<bool>.Success(true);
    }

    #region File Storage Operations

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, byte[]> ImageSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } }
    };

    public async Task<ServiceResult<BookDto>> UploadCoverAsync(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetAsync(b => b.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (book is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.NotFound, "Book not found", "The book with the provided ID does not exist.");

        if (file.Length == 0 || file.Length > MaxFileSize)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid file size", $"The file size must be between 1 byte and {MaxFileSize / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageSignatures.TryGetValue(extension, out var signature)) // compare file signature with the expected signature and out true if it matches
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid file type", "Only JPG, JPEG and PNG files are allowed.");

        // check the real content of the file by reading the first few bytes and comparing it with the expected signature
        await using (var stream = file.OpenReadStream()) // Open the file stream for reading
        {
            var header = new byte[signature.Length]; // Read the file header to validate the signature
            var readBytes = await stream.ReadAsync(header, 0, header.Length, cancellationToken); // Read the header bytes
            if (readBytes != signature.Length || !signature.SequenceEqual(header)) // Compare the read bytes with the expected signature
                return ServiceResult<BookDto>.Failure(
                    ServiceErrorType.Validation, "Invalid file content", "The file content does not match the expected format.");
        }

        var oldPath = book.CoverImageUrl;

        string newPath;
        await using (var stream = file.OpenReadStream())
        {
            newPath = await _fileStorageService.SaveAsync(stream, extension, "book-covers", cancellationToken);
        }

        try
        {
            book.CoverImageUrl = newPath;
            await _unitOfWork.Books.UpdateAsync(book, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
        catch
        {
            // If saving to the database fails, delete the newly uploaded file to avoid orphaned files
            _fileStorageService.Delete(newPath);
            throw;
        }

        // if old path is null skip deletion, otherwise delete the old file
        if (!string.IsNullOrEmpty(oldPath)) 
        {
            _fileStorageService.Delete(oldPath); // Only after database update is successful, delete the old file to avoid orphaned files
        }

        var updatedBook = await _unitOfWork.Books.GetAsync(b => b.Id == id, tracked: false, includeProperties: "Author,Category", cancellationToken: cancellationToken);
        return ServiceResult<BookDto>.Success(_mapper.Map<BookDto>(updatedBook));
    }

    public async Task<ServiceResult<bool>> DeleteCoverAsync(int id, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetAsync(b => b.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (book is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound, "Book not found", "The book with the provided ID does not exist.");

        var oldPath = book.CoverImageUrl;
        if (string.IsNullOrEmpty(oldPath))
            return ServiceResult<bool>.Success(true); // No cover image to delete

        book.CoverImageUrl = null; // Set the cover image URL to null before deleting the file

        await _unitOfWork.Books.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        _fileStorageService.Delete(oldPath); // Only after database update is successful, delete the old file to avoid orphaned files

        return ServiceResult<bool>.Success(true);
    }
    #endregion

}
