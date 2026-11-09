using BookHaven.API.Models.Dto;
using BookHaven.API.Models.Queries;
using BookHaven.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHaven.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(CacheProfileName = "Default30")]
    [ProducesResponseType(typeof(BookDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<BookDto>>> GetBooks([FromQuery] BookQueryParameters query, CancellationToken cancellationToken)
    {
        var response = await _bookService.GetPagedAsync(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetBook")]
    [AllowAnonymous]
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

    [HttpPost]
    [Authorize(Policy = "CatalogWriter")]
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

    [HttpPut("{id:int}", Name = "UpdateBook")]
    [Authorize(Policy = "CatalogWriter")]
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

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CatalogWriter")]
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

    [HttpPost("{id:int}/cover")]
    //[Authorize(Policy = "CatalogWriter")]
    [Consumes("multipart/form-data")] // Specify that the endpoint consumes multipart/form-data
    [RequestSizeLimit(5 * 1024 * 1024)] // Limit file size to 5 MB
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BookDto>> UploadBookCover(int id, IFormFile cover, CancellationToken cancellationToken)
    {
        var result = await _bookService.UploadBookCoverAsync(id, cover, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}/cover")]
    //[Authorize(Policy = "CatalogWriter")]
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
