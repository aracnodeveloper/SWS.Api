using App.Shared;

namespace App.Modules.Order;

public sealed class OrderUseCaseRegister : IRegisterUseCase
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<CreateOrder>();
        services.AddScoped<CancelOrder>();
        services.AddScoped<UpdateOrderStatus>();
        services.AddScoped<OrderNotifier>();
        services.AddHostedService<ExpirePendingOrdersJob>();
    }
}
