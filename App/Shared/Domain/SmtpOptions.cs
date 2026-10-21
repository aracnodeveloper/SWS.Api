namespace App.Shared.Domain;

/// <summary>SMTP para correos transaccionales. Sección <c>Smtp</c> / env <c>SMTP_*</c>.</summary>
public class SmtpOptions
{
    public const string SectionAccesor = "Smtp";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string User { get; set; } = "";
    public string Pass { get; set; } = "";
    /// <summary>From; si vacío usa User.</summary>
    public string From { get; set; } = "";
    public string FromName { get; set; } = "Sale World Sale";
}
