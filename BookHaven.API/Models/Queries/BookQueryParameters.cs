using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Models.Queries;

public class BookQueryParameters : IValidatableObject
{
    public string? SearchTerm { get; set; }
    public int? AuthorId { get; set; }
    public int? CategoryId { get; set; }

    [Range(1, 9999)] public int? PublishedYearFrom { get; set; }
    [Range(1, 9999)] public int? PublishedYearTo { get; set; }

    [Range(1, 100)] public int PageSize { get; set; } = 10;
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PublishedYearFrom.HasValue && PublishedYearTo.HasValue && PublishedYearFrom > PublishedYearTo)
        {
            // yield is used to return each iteration of ValidationResult one at a time, allowing for deferred execution and efficient memory usage.
            yield return new ValidationResult(
                "PublishedYearFrom cannot be greater than PublishedYearTo.",
                new[] { nameof(PublishedYearFrom), nameof(PublishedYearTo) });
        }
    }
}
