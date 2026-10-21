using App.Shared;

namespace App.Modules.Catalog;

public sealed class CatalogUseCaseRegister : IRegisterUseCase
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<UploadProductImage>();
    }
}
