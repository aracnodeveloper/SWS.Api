namespace App.Shared.Domain;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
