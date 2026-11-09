namespace BookHaven.API.Infrastructure.Storage;

public static class ImageValidator
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, byte[]> ImageSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } }
    };

    public static async Task<(string? Extension, string? ErrorMessage)> ValidateImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            return (null, "The file must be between 1 byte and 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ImageSignatures.TryGetValue(extension, out var signature)) // compare file signature with the expected signature and out true if it matches
            return (null, "Only .jpeg, .jpg and .png images are allowed.");

        // check the real content of the file by reading the first few bytes and comparing it with the expected signature
        await using (var stream = file.OpenReadStream()) // Open the file stream for reading
        {
            var header = new byte[signature.Length]; // Read the file header to validate the signature
            var readBytes = await stream.ReadAsync(header, 0, header.Length, cancellationToken); // Read the header bytes
            if (readBytes != signature.Length || !signature.SequenceEqual(header)) // Compare the read bytes with the expected signature
                return (null, "The file content does not match the expected image format.");
        }

        return (extension, null); // Return the file extension if validation is successful
    }
}
