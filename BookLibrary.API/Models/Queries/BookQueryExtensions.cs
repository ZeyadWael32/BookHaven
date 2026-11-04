using System.Linq.Expressions;

namespace BookHaven.API.Models.Queries;

public static class BookQueryExtensions
{
    public static Expression<Func<Book, bool>> ToFilter(this BookQueryParameters parameters)
    {
        var term = string.IsNullOrWhiteSpace(parameters.SearchTerm) ? null : parameters.SearchTerm.Trim();

        return book =>
            (term == null || book.Title.Contains(term) || book.Author.Contains(term)) &&
            (parameters.CategoryId == null || book.CategoryId == parameters.CategoryId) &&
            (parameters.PublishedYearFrom == null || book.PublishedYear >= parameters.PublishedYearFrom) &&
            (parameters.PublishedYearTo == null || book.PublishedYear <= parameters.PublishedYearTo);
    }
}
