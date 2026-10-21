namespace App.Modules.Payment;

/// <summary>
/// Contrato común de pasarelas. Para sumar Kushki/Datafast/PayPal: implementar esta
/// interfaz, registrarla en PaymentUseCaseRegister y añadir el método en el front.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Nombre corto guardado en Transaction.Gateway y Order.PaymentMethod.</summary>
    string Name { get; }

    /// <summary>false si faltan credenciales: el checkout oculta este método.</summary>
    bool IsConfigured { get; }

    Task<PaymentPreparation> PrepareAsync(PaymentRequest request, CancellationToken ct);

    Task<PaymentConfirmation> ConfirmAsync(string gatewayId, string clientTransactionId, CancellationToken ct);
}

/// <summary>Montos en centavos (como los pide PayPhone).</summary>
public record PaymentRequest(
    string ClientTransactionId,
    long AmountCents,
    long AmountWithTaxCents,
    long AmountWithoutTaxCents,
    long TaxCents,
    string Reference,
    string Email,
    string ResponseUrl,
    string CancellationUrl);

public record PaymentPreparation(string RedirectUrl, string? GatewayPaymentId, string RawResponse);

public record PaymentConfirmation(
    bool Approved,
    string Status,
    string ClientTransactionId,
    long AmountCents,
    string? TransactionId,
    string? AuthorizationCode,
    string? CardBrand,
    string? LastDigits,
    string? Message,
    string RawResponse);

public class PaymentGatewayException(string message) : Exception(message);
