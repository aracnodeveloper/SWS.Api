namespace App.Modules.Order;

public class OrderStatusPatchDto
{
    public string Status { get; set; } = "";
    public string? Note { get; set; }
}
