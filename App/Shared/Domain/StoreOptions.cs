namespace App.Shared.Domain;

/// <summary>Reglas comerciales de la tienda. Sección <c>Store</c>.</summary>
public class StoreOptions
{
    public const string SectionAccesor = "Store";

    public string Name { get; set; } = "Sale World Sale";

    /// <summary>Número en formato internacional sin "+" (wa.me/593...).</summary>
    public string WhatsAppNumber { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string Currency { get; set; } = "USD";

    /// <summary>IVA Ecuador (0.15 desde abril 2024).</summary>
    public decimal VatRate { get; set; } = 0.15m;

    /// <summary>true: el precio publicado ya incluye IVA y se desglosa hacia atrás.</summary>
    public bool PricesIncludeVat { get; set; } = true;

    public decimal ShippingCost { get; set; }

    /// <summary>Monto desde el cual el envío es gratis; null = nunca.</summary>
    public decimal? FreeShippingFrom { get; set; }

    /// <summary>Minutos que se reserva el stock de un pedido con pago en línea sin confirmar.</summary>
    public int PendingPaymentMinutes { get; set; } = 30;

    /// <summary>Horas que se reserva el stock de un pedido por WhatsApp sin confirmar.</summary>
    public int WhatsAppHoldHours { get; set; } = 24;
}
