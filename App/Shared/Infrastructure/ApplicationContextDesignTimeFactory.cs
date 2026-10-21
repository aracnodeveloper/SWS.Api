using App.Shared.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace App.Shared.Infrastructure;

/// <summary>
/// Permite <c>dotnet ef migrations add</c> sin levantar la aplicación ni Docker.
/// Cadena: variable <c>SWS_DESIGN_TIME_CONNECTION</c> o Postgres local.
/// </summary>
public sealed class ApplicationContextDesignTimeFactory
    : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SWS_DESIGN_TIME_CONNECTION")
            ?? "Host=localhost;Port=5445;Database=swsdb;Username=swsuser;Password=swspass";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.CommandTimeout(600));

        return new ApplicationContext(optionsBuilder.Options, new SessionAccesor());
    }
}
