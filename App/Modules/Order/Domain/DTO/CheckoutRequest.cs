using System.ComponentModel.DataAnnotations;

namespace App.Modules.Order;

public class CheckoutRequest
{
    [Required(ErrorMessage = "Ingresa tu nombre."), MaxLength(120)]
    public string CustomerName { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu correo."), EmailAddress(ErrorMessage = "Correo inválido."), MaxLength(160)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu teléfono."), RegularExpression(@"^\+?[0-9 ]{7,15}$", ErrorMessage = "Teléfono inválido.")]
    public string Phone { get; set; } = "";

    /// <summary>Cédula (10 dígitos) o RUC (13 dígitos).</summary>
    [Required(ErrorMessage = "Ingresa tu cédula o RUC."), RegularExpression(@"^(\d{10}|\d{13})$", ErrorMessage = "Cédula o RUC inválido.")]
    public string DocumentId { get; set; } = "";

    [Required(ErrorMessage = "Selecciona tu provincia."), MaxLength(60)]
    public string Province { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu ciudad."), MaxLength(80)]
    public string City { get; set; } = "";

    [Required(ErrorMessage = "Ingresa tu dirección."), MaxLength(250)]
    public string Address { get; set; } = "";

    [MaxLength(250)]
    public string? AddressReference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>payphone | whatsapp</summary>
    [Required]
    public string PaymentMethod { get; set; } = PaymentMethods.PayPhone;

    [Required, MinLength(1, ErrorMessage = "El carrito está vacío."), MaxLength(30)]
    public List<CheckoutItem> Items { get; set; } = new();
}

public class CheckoutItem
{
    public Guid ProductId { get; set; }

    [Range(1, 20, ErrorMessage = "Cantidad inválida.")]
    public int Quantity { get; set; }
}
