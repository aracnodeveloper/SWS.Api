namespace App.Modules.Payment;

/// <summary>
/// Credenciales de PayPhone (Developer → Configuración en appdeveloper.payphonetodoesposible.com).
/// Sección <c>PayPhone</c>. Nunca se envían al front.
/// </summary>
public class PayPhoneOptions
{
    public const string SectionAccesor = "PayPhone";

    public string BaseUrl { get; set; } = "https://pay.payphonetodoesposible.com";
    public string Token { get; set; } = "";
    public string StoreId { get; set; } = "";
}
