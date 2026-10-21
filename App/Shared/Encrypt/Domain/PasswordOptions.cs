namespace App.Shared.Encrypt;

public class PasswordOptions
{
    public const string SectionAccesor = "Password";
    public string Salt { get; set; } = string.Empty;
    public string Pepper { get; set; } = string.Empty;
}
