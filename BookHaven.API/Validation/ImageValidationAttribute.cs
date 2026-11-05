using System.ComponentModel.DataAnnotations;

namespace BookHaven.API.Validation;

public class ImageValidationAttribute : ValidationAttribute
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true; // Allow null values, use [Required] for mandatory fields

        if (value is not IFormFile file)
            return false;

        if (file.Length > 5 * 1024 * 1024) // 5 MB limit (1 KB = 1024 bytes, 1 MB = 1024 * 1024 bytes)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant(); // Get the file extension in lowercase

        return _allowedExtensions.Contains(extension);
    }
}
