using AutoMapper;
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

    public BookService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<BookDto>> GetPagedAsync(BookQueryParameters query, CancellationToken cancellationToken)
    {
        var filter = query.ToFilter();

        var books = await _unitOfWork.Books.GetAllAsync(
            filter: filter,
            tracked: false,
            includeProperties: "Category",
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
        var book = await _unitOfWork.Books.GetAsync(u => u.Id == id, tracked: false, includeProperties: "Category", cancellationToken: cancellationToken);
        
        return book is null ? null : _mapper.Map<BookDto>(book);
    }

    public async Task<ServiceResult<BookDto>> CreateAsync(BookCreateDto createDto, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetAsync(
            c => c.Id == createDto.CategoryId, tracked: false, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid category", "The provided category ID is invalid.");

        if (await _unitOfWork.Books.IsbnExistsAsync(createDto.Isbn, cancellationToken: cancellationToken))
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Conflict, "ISBN already exists", "A book with the provided ISBN already exists.");

        var book = _mapper.Map<Book>(createDto);
        await _unitOfWork.Books.CreateAsync(book, cancellationToken);

        var created = await _unitOfWork.Books.GetAsync(
            b => b.Id == book.Id, tracked: false, includeProperties: "Category", cancellationToken: cancellationToken);

        return ServiceResult<BookDto>.Success(_mapper.Map<BookDto>(created));
    }

    public async Task<ServiceResult<BookDto>> UpdateAsync(int id, BookUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var bookToUpdate = await _unitOfWork.Books.GetAsync(u => u.Id == id, tracked: true, includeProperties: "Category", cancellationToken: cancellationToken);

        if (bookToUpdate is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.NotFound, "Book not found", "The book with the provided ID does not exist.");

        var category = await _unitOfWork.Categories.GetAsync(c => c.Id == updateDto.CategoryId, tracked: false, cancellationToken: cancellationToken);

        if (category is null)
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Validation, "Invalid category", "The provided category ID is invalid.");

        if (await _unitOfWork.Books.IsbnExistsAsync(updateDto.Isbn, excludeId: id, cancellationToken: cancellationToken))
            return ServiceResult<BookDto>.Failure(
                ServiceErrorType.Conflict, "ISBN already exists", "A book with the provided ISBN already exists.");

        _mapper.Map(updateDto, bookToUpdate); // Map the updateDto to the existing book entity
        await _unitOfWork.Books.UpdateAsync(bookToUpdate, cancellationToken);
        var updated = await _unitOfWork.Books.GetAsync(
            b => b.Id == bookToUpdate.Id, tracked: false, includeProperties: "Category", cancellationToken: cancellationToken);
        return ServiceResult<BookDto>.Success(_mapper.Map<BookDto>(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var bookToDelete = await _unitOfWork.Books.GetAsync(u => u.Id == id, tracked: true, cancellationToken: cancellationToken);

        if (bookToDelete is null)
            return ServiceResult<bool>.Failure(
                ServiceErrorType.NotFound, "Book not found", "The book with the provided ID does not exist.");

        await _unitOfWork.Books.RemoveAsync(bookToDelete, cancellationToken);
        return ServiceResult<bool>.Success(true);
    }
}
