using App.Shared;

namespace App.Modules.Auth;

public sealed class AuthUseCaseRegister : IRegisterUseCase
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<GenerateTokenSession>();
        services.AddScoped<EnsureAdminUser>();
    }
}
