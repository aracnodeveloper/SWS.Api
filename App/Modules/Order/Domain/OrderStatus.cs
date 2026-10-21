namespace App.Modules.Order;

/// <summary>Estados del pedido. Strings (no enum) para que el front y la BD los lean igual.</summary>
public static class OrderStatus
{
    /// <summary>Pago en línea iniciado; stock reservado hasta ExpiresAt.</summary>
    public const string PendingPayment = "pending_payment";

    /// <summary>Pedido por WhatsApp; stock reservado hasta que el admin lo confirme.</summary>
    public const string PendingConfirmation = "pending_confirmation";

    public const string Paid = "paid";
    public const string Preparing = "preparing";
    public const string Shipped = "shipped";
    public const string Delivered = "delivered";
    public const string Cancelled = "cancelled";

    /// <summary>Venció la reserva sin pago; el stock se devolvió.</summary>
    public const string Expired = "expired";

    public static readonly string[] Pending = [PendingPayment, PendingConfirmation];

    /// <summary>Transiciones que el admin puede aplicar manualmente.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> AllowedTransitions =
        new Dictionary<string, string[]>
        {
            [PendingPayment] = [Paid, Cancelled],
            [PendingConfirmation] = [Paid, Cancelled],
            [Paid] = [Preparing, Shipped, Cancelled],
            [Preparing] = [Shipped, Cancelled],
            [Shipped] = [Delivered],
        };

    public static bool ReleasesStock(string status) => status is Cancelled or Expired;
}

public static class PaymentMethods
{
    public const string PayPhone = "payphone";
    public const string WhatsApp = "whatsapp";

    public static readonly string[] All = [PayPhone, WhatsApp];
}
