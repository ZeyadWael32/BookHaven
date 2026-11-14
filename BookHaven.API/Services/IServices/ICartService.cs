using BookHaven.API.Models.Dto.CartItem;
using BookHaven.API.Models.Dto.ShoppingCart;

namespace BookHaven.API.Services.IServices;

public interface ICartService
{
    Task<ShoppingCartDto> GetCartAsync(string userId, CancellationToken cancellationToken);
    Task<ServiceResult<ShoppingCartDto>> AddItemToCartAsync(string userId, CartItemAddDto createDto, CancellationToken cancellationToken);
    Task<ServiceResult<ShoppingCartDto>> UpdateCartItemAsync(string userId, int bookId, CartItemUpdateDto updateDto, CancellationToken cancellationToken);
    Task<ServiceResult<ShoppingCartDto>> RemoveItemFromCartAsync(string userId, int bookId, CancellationToken cancellationToken);
    Task<ServiceResult<ShoppingCartDto>> ClearCartAsync(string userId, CancellationToken cancellationToken);
}
