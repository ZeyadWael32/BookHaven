namespace BookHaven.API.Models.Dto;

public class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public string? ProfileImageUrl { get; set; }
}
