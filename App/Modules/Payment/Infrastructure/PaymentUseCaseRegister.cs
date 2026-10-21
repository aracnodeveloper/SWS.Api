using App.Shared;
using Microsoft.Extensions.Options;

namespace App.Modules.Payment;

public sealed class PaymentUseCaseRegister : IRegisterUseCase
{
    public static void Register(IServiceCollection services)
    {
        services.AddHttpClient(PayPhoneGateway.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PayPhoneOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Pasarelas disponibles: el resolver elige por nombre (Order.PaymentMethod).
        services.AddScoped<IPaymentGateway, PayPhoneGateway>();
        services.AddScoped<PaymentGatewayResolver>();
        services.AddScoped<StartPayment>();
        services.AddScoped<ConfirmPayment>();
    }
}
