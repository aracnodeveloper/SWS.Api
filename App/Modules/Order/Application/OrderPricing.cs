using App.Shared.Domain;

namespace App.Modules.Order;

public record OrderTotals(decimal ItemsTotal, decimal Shipping, decimal Subtotal, decimal Vat, decimal Total);

/// <summary>Cálculo de envío e IVA. Única fuente de verdad: el front solo muestra estimados.</summary>
public static class OrderPricing
{
    public static OrderTotals Calculate(decimal itemsTotal, StoreOptions store)
    {
        var shipping = store.FreeShippingFrom is decimal free && itemsTotal >= free ? 0m : store.ShippingCost;
        var rate = store.VatRate;

        if (store.PricesIncludeVat)
        {
            var total = itemsTotal + shipping;
            var subtotal = Math.Round(total / (1 + rate), 2, MidpointRounding.AwayFromZero);
            return new OrderTotals(itemsTotal, shipping, subtotal, total - subtotal, total);
        }

        var taxable = itemsTotal + shipping;
        var vat = Math.Round(taxable * rate, 2, MidpointRounding.AwayFromZero);
        return new OrderTotals(itemsTotal, shipping, taxable, vat, taxable + vat);
    }
}
