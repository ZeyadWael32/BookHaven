namespace BookHaven.API.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _wwwRootPath;
        public LocalFileStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _wwwRootPath = webHostEnvironment.WebRootPath;
        }

        public async Task<string?> SaveAsync(Stream content, string extension, string folder, CancellationToken cancellationToken = default)
        {
            if (content != null && content.Length > 0)
            {
                string fileName = $"{Guid.NewGuid()}{extension}";
                string directory = Path.Combine(_wwwRootPath, "uploads", folder); // wwwroot/uploads/folder
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var fullPath = Path.Combine(directory, fileName); // wwwroot/uploads/folder/filename.ext
                await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew))
                {
                    await content.CopyToAsync(fileStream, cancellationToken); // Save the file to the specified path
                }

                return $"/uploads/{folder}/{fileName}";
            }

            return null;
        }

        public void Delete(string relativePath) // relativePath is expected to be in the format "/uploads/folder/filename.ext"
        {
            var fullPath = Path.GetFullPath(Path.Combine(_wwwRootPath, relativePath.TrimStart('/'))); // wwwroot + /uploads/folder/filename.ext
            var uploadsRoot = Path.GetFullPath(Path.Combine(_wwwRootPath, "uploads")) + Path.DirectorySeparatorChar; // wwwroot/uploads/

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase)) // Ensure the file is within the uploads directory
            {
                throw new InvalidOperationException("Attempt to delete a file outside the uploads directory.");
            }

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
