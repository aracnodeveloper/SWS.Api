using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using App.Modules.Logs;
using Microsoft.Extensions.Options;

namespace App.Modules.Payment;

/// <summary>
/// PayPhone "Botón de Pagos" por redirección:
/// 1) POST /api/button/Prepare → URL de pago; 2) el cliente paga y vuelve a responseUrl
/// con ?id&amp;clientTransactionId; 3) POST /api/button/V2/Confirm. Si no se confirma en
/// ~5 minutos PayPhone reversa el cobro automáticamente.
/// </summary>
public class PayPhoneGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PayPhoneOptions> options,
    IAppLogger logger) : IPaymentGateway
{
    public const string GatewayName = "payphone";
    public const string HttpClientName = "PayPhone";

    /// <summary>statusCode de PayPhone: 3 = aprobada, 2 = cancelada.</summary>
    private const int ApprovedStatusCode = 3;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public string Name => GatewayName;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.Token) && !string.IsNullOrWhiteSpace(options.Value.StoreId);

    public async Task<PaymentPreparation> PrepareAsync(PaymentRequest request, CancellationToken ct)
    {
        var body = new PrepareBody
        {
            Amount = request.AmountCents,
            AmountWithTax = request.AmountWithTaxCents,
            AmountWithoutTax = request.AmountWithoutTaxCents,
            Tax = request.TaxCents,
            ClientTransactionId = request.ClientTransactionId,
            StoreId = options.Value.StoreId,
            Reference = request.Reference,
            Email = request.Email,
            ResponseUrl = request.ResponseUrl,
            CancellationUrl = request.CancellationUrl,
        };

        var raw = await PostAsync("api/button/Prepare", body, ct);
        var prepared = JsonSerializer.Deserialize<PrepareResponse>(raw, Json)
            ?? throw new PaymentGatewayException("Respuesta vacía de PayPhone.");

        var url = prepared.PayWithCard ?? prepared.PayWithPayPhone
            ?? throw new PaymentGatewayException("PayPhone no devolvió la URL de pago.");
        return new PaymentPreparation(url, prepared.PaymentId, raw);
    }

    public async Task<PaymentConfirmation> ConfirmAsync(string gatewayId, string clientTransactionId, CancellationToken ct)
    {
        if (!long.TryParse(gatewayId, out var id))
            throw new PaymentGatewayException("Id de transacción PayPhone inválido.");

        var raw = await PostAsync("api/button/V2/Confirm", new { id, clientTxId = clientTransactionId }, ct);
        var r = JsonSerializer.Deserialize<ConfirmResponse>(raw, Json)
            ?? throw new PaymentGatewayException("Respuesta vacía de PayPhone.");

        var approved = r.StatusCode == ApprovedStatusCode
            && string.Equals(r.TransactionStatus, "Approved", StringComparison.OrdinalIgnoreCase);

        return new PaymentConfirmation(
            Approved: approved,
            Status: approved ? TransactionStatus.Approved : TransactionStatus.Canceled,
            ClientTransactionId: r.ClientTransactionId ?? "",
            AmountCents: r.Amount,
            TransactionId: r.TransactionId?.ToString(),
            AuthorizationCode: r.AuthorizationCode,
            CardBrand: r.CardBrand,
            LastDigits: r.LastDigits,
            Message: r.Message ?? r.TransactionStatus,
            RawResponse: raw);
    }

    private async Task<string> PostAsync(string path, object body, CancellationToken ct)
    {
        if (!IsConfigured)
            throw new PaymentGatewayException("PayPhone no está configurado (PayPhone:Token / PayPhone:StoreId).");

        var client = httpClientFactory.CreateClient(HttpClientName);
        // StringContent (con Content-Length) en vez de JsonContent (chunked): máxima compatibilidad.
        using var req = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, body.GetType(), Json), Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.Token);

        string raw;
        HttpResponseMessage res;
        try
        {
            res = await client.SendAsync(req, ct);
            raw = await res.Content.ReadAsStringAsync(ct);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            // Caída de red o timeout: se trata como fallo de pasarela para liberar la reserva.
            logger.Log($"[PayPhone] {path} sin respuesta: {e.Message}", LogType.ERROR);
            throw new PaymentGatewayException("PayPhone no respondió.");
        }

        using var _ = res;
        if (!res.IsSuccessStatusCode)
        {
            logger.Log($"[PayPhone] {path} → {(int)res.StatusCode}: {raw}", LogType.ERROR);
            throw new PaymentGatewayException(ExtractMessage(raw) ?? "PayPhone rechazó la solicitud.");
        }
        return raw;
    }

    private static string? ExtractMessage(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class PrepareBody
    {
        public long Amount { get; init; }
        public long AmountWithoutTax { get; init; }
        public long AmountWithTax { get; init; }
        public long Tax { get; init; }
        public long Service { get; init; }
        public long Tip { get; init; }
        public string Currency { get; init; } = "USD";
        public string ClientTransactionId { get; init; } = "";
        public string StoreId { get; init; } = "";
        public string Reference { get; init; } = "";
        public string? Email { get; init; }
        public string ResponseUrl { get; init; } = "";
        public string CancellationUrl { get; init; } = "";
        public string Lang { get; init; } = "es";
        public int TimeZone { get; init; } = -5;
    }

    private sealed class PrepareResponse
    {
        public string? PaymentId { get; init; }
        public string? PayWithPayPhone { get; init; }
        public string? PayWithCard { get; init; }
    }

    private sealed class ConfirmResponse
    {
        public int StatusCode { get; init; }
        public string? TransactionStatus { get; init; }
        public long? TransactionId { get; init; }
        public string? ClientTransactionId { get; init; }
        public long Amount { get; init; }
        public string? AuthorizationCode { get; init; }
        public string? CardBrand { get; init; }
        public string? LastDigits { get; init; }
        public string? Message { get; init; }
    }
}
