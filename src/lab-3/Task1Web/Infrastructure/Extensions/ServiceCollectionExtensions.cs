using FluentMigrator.Runner;
using Microsoft.Extensions.Options;
using Npgsql;
using Task1Web.Domain.Enums;
using Task1Web.Domain.Interfaces;
using Task1Web.Domain.Models;
using Task1Web.Domain.Services;
using Task1Web.Infrastructure.Migrations;
using Task1Web.Infrastructure.Repositories;

namespace Task1Web.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DbOptions>(configuration.GetSection("Database"));
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                using ServiceProvider provider = services.BuildServiceProvider();
                DbOptions options = provider.GetRequiredService<IOptionsMonitor<DbOptions>>().CurrentValue;
                rb.AddPostgres()
                    .WithGlobalConnectionString(options.ConnectionString)
                    .ScanIn(typeof(ProductsTableInit).Assembly)
                    .For.Migrations();
            });
        services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<OrderService>();
        services.AddScoped<ProductService>();
        services.AddSingleton(sp =>
        {
            DbOptions options = sp.GetRequiredService<IOptionsMonitor<DbOptions>>().CurrentValue;
            var builder = new NpgsqlDataSourceBuilder(options.ConnectionString);
            builder.MapEnum<OrderHistoryItemKind>("order_history_item_kind");
            builder.MapEnum<OrderState>("order_state");
            return builder.Build();
        });

        return services;
    }
}