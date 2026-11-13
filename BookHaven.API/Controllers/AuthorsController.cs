using BookHaven.API.Models.Dto;
using BookHaven.API.Models.Dto.Author;
using BookHaven.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorsController : ApiControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(CacheProfileName = "Default30")]
    [ProducesResponseType(typeof(PagedResponse<AuthorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<AuthorDto>>> GetAuthors(
        CancellationToken cancellationToken,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1)
    {
        var response = await _authorService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetAuthor")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthorDto>> GetAuthor(int id, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetByIdAsync(id, cancellationToken);
        if (author is null)
            return NotFound();

        return Ok(author);
    }

    [HttpPost]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(
        [FromBody] AuthorCreateDto createDto, CancellationToken cancellationToken)
    {
        var result = await _authorService.CreateAsync(createDto, cancellationToken);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return CreatedAtRoute("GetAuthor", new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}", Name = "UpdateAuthor")]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthorDto>> UpdateAuthor(
        int id, [FromBody] AuthorUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var result = await _authorService.UpdateAsync(id, updateDto, cancellationToken);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAuthor(int id, CancellationToken cancellationToken)
    {
        var result = await _authorService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }

    [HttpPost("{id:int}/picture")]
    [Authorize(Policy = "CatalogWriter")]
    [Consumes("multipart/form-data")] // Specify that the endpoint consumes multipart/form-data
    [RequestSizeLimit(6 * 1024 * 1024)] // Limit request size to 6 MB
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthorDto>> UploadAuthorPicture(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var result = await _authorService.UploadAuthorPictureAsync(id, file, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpDelete("{id:int}/picture")]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAuthorPicture(int id, CancellationToken cancellationToken)
    {
        var result = await _authorService.DeleteAuthorPictureAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return NoContent();
    }
}