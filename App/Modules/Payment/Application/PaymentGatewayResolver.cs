namespace App.Modules.Payment;

public class PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways)
{
    public IPaymentGateway Resolve(string name) =>
        gateways.FirstOrDefault(g => g.Name == name && g.IsConfigured)
        ?? throw new PaymentGatewayException($"El método de pago \"{name}\" no está disponible.");

    public IReadOnlyList<string> Available() =>
        gateways.Where(g => g.IsConfigured).Select(g => g.Name).ToList();
}
