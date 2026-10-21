namespace App.Modules.Order;

/// <summary>Vista del pedido para el cliente (sin datos internos ni dirección completa).</summary>
public class PublicOrderDTO
{
    public string Code { get; set; } = "";

    /// <summary>Para enlazar a /pedido/{token} (quien lo recibe ya conoce el pedido).</summary>
    public Guid PublicToken { get; set; }

    public string Status { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Email { get; set; } = "";
    public string City { get; set; } = "";
    public decimal ItemsTotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? WhatsAppUrl { get; set; }
    public List<PublicOrderItemDTO> Items { get; set; } = new();
}

public class PublicOrderItemDTO
{
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
