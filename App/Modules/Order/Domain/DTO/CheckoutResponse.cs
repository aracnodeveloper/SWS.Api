namespace App.Modules.Order;

public class CheckoutResponse
{
    public string OrderCode { get; set; } = "";
    public Guid PublicToken { get; set; }
    public string PaymentMethod { get; set; } = "";
    public decimal Total { get; set; }

    /// <summary>URL de la pasarela a la que el front debe redirigir (pago en línea).</summary>
    public string? RedirectUrl { get; set; }

    /// <summary>Link wa.me con el pedido prellenado (pedido por WhatsApp).</summary>
    public string? WhatsAppUrl { get; set; }
}
