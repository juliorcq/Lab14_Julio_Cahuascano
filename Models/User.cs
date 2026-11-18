namespace Lab06_Julio_Cahuascano.Models;

public class User
{
    public int IdUser { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
}