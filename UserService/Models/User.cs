namespace UserService.Models;

public class User
{
    public int Id { get; set; }       // Primary key
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
