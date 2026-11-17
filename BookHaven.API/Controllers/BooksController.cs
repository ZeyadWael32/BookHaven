using BookHaven.API.Models.Dto;
using BookHaven.API.Models.Dto.Book;
using BookHaven.API.Models.Queries;
using BookHaven.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHaven.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "Customer")]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [ResponseCache(CacheProfileName = "Default30")]
    [ProducesResponseType(typeof(PagedResponse<BookDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooks([FromQuery] BookQueryParameters query, CancellationToken cancellationToken)
    {
        var response = await _bookService.GetPagedAsync(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    [ProducesResponseType(typeof(BookDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> GetBook(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    [HttpPost("admin/create")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] BookCreateDto createDto, CancellationToken cancellationToken)
    {
        var result = await _bookService.CreateAsync(createDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return CreatedAtRoute("GetBook", new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("admin/{id:int}", Name = "UpdateBook")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromBody] BookUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var result = await _bookService.UpdateAsync(id, updateDto, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpDelete("admin/{id:int}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBook(int id, CancellationToken cancellationToken)
    {
        var result = await _bookService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return NoContent();
    }

    [HttpPost("admin/{id:int}/cover")]
    [Authorize(Policy = "Admin")]
    [Consumes("multipart/form-data")] // Specify that the endpoint consumes multipart/form-data
    [RequestSizeLimit(6 * 1024 * 1024)] // Limit request size to 6 MB
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> UploadBookCover(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var result = await _bookService.UploadBookCoverAsync(id, file, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }


    [HttpDelete("admin/{id:int}/cover")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBookCover(int id, CancellationToken cancellationToken)
    {
        var result = await _bookService.DeleteBookCoverAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return NoContent();
    }
}
