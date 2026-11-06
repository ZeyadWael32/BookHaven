namespace BookHaven.API.Infrastructure.Storage
{
    public interface IFileStorageService
    {
        Task<string?> SaveAsync(Stream content, string extension, string folder, CancellationToken cancellationToken = default);
        void Delete(string relativePath);
    }
}
