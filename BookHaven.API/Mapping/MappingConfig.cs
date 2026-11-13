using AutoMapper;
using BookHaven.API.Models;
using BookHaven.API.Models.Dto.Author;
using BookHaven.API.Models.Dto.Book;
using BookHaven.API.Models.Dto.CartItem;
using BookHaven.API.Models.Dto.Category;
using BookHaven.API.Models.Dto.ShoppingCart;

namespace BookHaven.API.Mapping;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Name));

        CreateMap<BookCreateDto, Book>()
            .ForMember(dest => dest.CoverImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore());

        CreateMap<BookUpdateDto, Book>()
            .ForMember(dest => dest.CoverImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore());

        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();

        CreateMap<Author, AuthorDto>();
        CreateMap<AuthorCreateDto, Author>();
        CreateMap<AuthorUpdateDto, Author>();

        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Book.Author.Name))
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.Book.CoverImageUrl))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Book.Price));

        CreateMap<ShoppingCart, ShoppingCartDto>();
    }
}
