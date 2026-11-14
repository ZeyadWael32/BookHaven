using BookHaven.API.Models.Dto.CartItem;
using BookHaven.API.Models.Dto.ShoppingCart;
using BookHaven.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHaven.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;
    private string? UserId => User.FindFirst("sub")?.Value; // extract used id from JWT keycloak token

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShoppingCartDto>> GetCart(CancellationToken cancellationToken)
    {
        if (UserId is null)
            return Unauthorized();

        var cart = await _cartService.GetCartAsync(UserId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShoppingCartDto>> AddItem([FromBody] CartItemAddDto addDto, CancellationToken cancellationToken)
    {
        if (UserId is null)
            return Unauthorized();

        var result = await _cartService.AddItemToCartAsync(UserId, addDto, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("items/{bookId}")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShoppingCartDto>> UpdateItem(int bookId, [FromBody] CartItemUpdateDto updateDto, CancellationToken cancellationToken)
    {
        if (UserId is null)
            return Unauthorized();

        var result = await _cartService.UpdateCartItemAsync(UserId, bookId, updateDto, cancellationToken: cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpDelete("items/{bookId}")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShoppingCartDto>> RemoveItem(int bookId, CancellationToken cancellationToken)
    {
        if (UserId is null)
            return Unauthorized();

        var result = await _cartService.RemoveItemFromCartAsync(UserId, bookId, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }

    [HttpDelete("clear")]
    [ProducesResponseType(typeof(ShoppingCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShoppingCartDto>> ClearCart(CancellationToken cancellationToken)
    {
        if (UserId is null)
            return Unauthorized();

        var result = await _cartService.ClearCartAsync(UserId, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResponse(result);
        }
        return Ok(result.Data);
    }
}
