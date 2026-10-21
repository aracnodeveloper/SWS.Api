using System.ComponentModel.DataAnnotations;

namespace App.Modules.Payment;

/// <summary>Parámetros que PayPhone agrega a la URL de retorno (?id=...&amp;clientTransactionId=...).</summary>
public class PayPhoneConfirmRequest
{
    [Range(1, long.MaxValue)]
    public long Id { get; set; }

    [Required, MaxLength(50)]
    public string ClientTransactionId { get; set; } = "";
}

/// <summary>El cliente volvió desde "Cancelar" en la pasarela.</summary>
public class PaymentCancelRequest
{
    [Required, MaxLength(50)]
    public string ClientTransactionId { get; set; } = "";

    public Guid Token { get; set; }
}
