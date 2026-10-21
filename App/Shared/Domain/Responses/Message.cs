namespace App.Shared.Domain;

/// <summary>Respuesta simple con un mensaje para el cliente.</summary>
public class HttpMessage
{
    public string message { get; set; }

    public HttpMessage(string message)
    {
        this.message = message;
    }
}
