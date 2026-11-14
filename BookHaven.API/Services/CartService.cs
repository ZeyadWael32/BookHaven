using AutoMapper;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto.CartItem;
using BookHaven.API.Models.Dto.ShoppingCart;
using BookHaven.API.Repository.IRepository;
using BookHaven.API.Services.IServices;

namespace BookHaven.API.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ShoppingCartDto> GetCartAsync(string userId, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId, cancellationToken: cancellationToken);
        if (cart is null)
        {
            return new ShoppingCartDto(); // Return an empty cart DTO if the cart is not found (not created in the database yet)
        }
        return _mapper.Map<ShoppingCartDto>(cart);
    }

    public async Task<ServiceResult<ShoppingCartDto>> AddItemToCartAsync(string userId, CartItemAddDto addDto, CancellationToken cancellationToken)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(addDto.BookId, cancellationToken: cancellationToken);
        if (book is null)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.Validation, "Invalid book", "The provided book ID is invalid.");

        var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId, cancellationToken: cancellationToken);
        if (cart is null)
        {
            cart = new ShoppingCart
            {
                UserId = userId,
                Items = new List<CartItem>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Carts.CreateAsync(cart, cancellationToken);
        }
        // check if the item already exists in the cart
        var item = cart.Items.FirstOrDefault(i => i.BookId == addDto.BookId);

        // calculate the new quantity after adding the item
        var newQuantity = 0;

        if (item is not null)
            newQuantity = item.Quantity;

        newQuantity += addDto.Quantity;

        if (newQuantity > 99)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.Validation, "Quantity limit exceeded", "You cannot add more than 99 items of the same book to your cart.");

        // if the item doesn't exist add it to the cart, otherwise update the quantity of the existing item
        if (item is null)
        {
            cart.Items.Add(new CartItem
            {
                BookId = addDto.BookId,
                Quantity = addDto.Quantity
            });
        }
        else
        {
            item.Quantity = newQuantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);

        return ServiceResult<ShoppingCartDto>.Success(await GetCartAsync(userId, cancellationToken));
    }

    public async Task<ServiceResult<ShoppingCartDto>> UpdateCartItemAsync(string userId, int bookId, CartItemUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId, cancellationToken: cancellationToken);

        if (cart is null)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.NotFound, "Cart not found", "The cart for the specified user does not exist.");

        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (item is null)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.NotFound, "Item not found", "The item you are trying to update does not exist in your cart.");

        item.Quantity = updateDto.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);

        return ServiceResult<ShoppingCartDto>.Success(await GetCartAsync(userId, cancellationToken));
    }

    public async Task<ServiceResult<ShoppingCartDto>> RemoveItemFromCartAsync(string userId, int bookId, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId, cancellationToken: cancellationToken);

        if (cart is null)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.NotFound, "Cart not found", "The cart for the specified user does not exist.");

        var item = cart.Items.FirstOrDefault(i => i.BookId == bookId);

        if (item is null)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.NotFound, "Item not found", "The item you are trying to remove does not exist in your cart.");

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync(cancellationToken);

        return ServiceResult<ShoppingCartDto>.Success(await GetCartAsync(userId, cancellationToken));
    }

    public async Task<ServiceResult<ShoppingCartDto>> ClearCartAsync(string userId, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId, cancellationToken: cancellationToken);

        if (cart is null)
            return ServiceResult<ShoppingCartDto>.Failure(
                ServiceErrorType.NotFound, "Cart not found", "The cart for the specified user does not exist.");

        if (cart.Items.Count > 0)
        {
            cart.Items.Clear();
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
        }

        return ServiceResult<ShoppingCartDto>.Success(new ShoppingCartDto());
    }
}
