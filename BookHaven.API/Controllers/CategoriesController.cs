using BookHaven.API.Models.Dto;
using BookHaven.API.Models.Dto.Category;
using BookHaven.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(CacheProfileName = "Default30")]
    [ProducesResponseType(typeof(PagedResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<CategoryDto>>> GetCategories(
        CancellationToken cancellationToken,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1)
    {
        var response = await _categoryService.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetCategory")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CategoryCreateDto createDto, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(createDto, cancellationToken);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return CreatedAtRoute("GetCategory", new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}", Name = "UpdateCategory")]
    [Authorize(Policy = "CatalogWriter")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id, [FromBody] CategoryUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, updateDto, cancellationToken);

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
    public async Task<ActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return ToErrorResponse(result);

        return NoContent();
    }
}