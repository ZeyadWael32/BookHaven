namespace BookHaven.API.Infrastructure.Storage
{
    public static class ImageUrlHelper
    {
        public static bool IsExternalUrl(string? url)
        {
            // Check if the URL is absolute and uses HTTP or HTTPS (http://example.com/image.jpg or https://example.com/image.jpg)
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps); 
        }
    }
}
