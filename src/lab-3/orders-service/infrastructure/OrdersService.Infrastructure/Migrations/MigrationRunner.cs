using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace OrdersService.Infrastructure.Migrations;

public static class MigrationRunner
{
    public static void RunMigrations(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public static void DropMigrations(IServiceProvider provider, long version)
    {
        using IServiceScope scope = provider.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(version);
    }

    public static void DropAllMigrations(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(0);
    }
}