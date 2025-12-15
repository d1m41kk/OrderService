using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using OrdersCreationService.Application.Abstractions.Persistence.Repositories;
using OrdersCreationService.Application.Models.Orders;
using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersCreationService.Application.Orders;
using OrdersCreationService.Infrastructure.Migrations;
using OrdersCreationService.Infrastructure.Repositories;

namespace OrdersCreationService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DbOptions>(configuration.GetSection("Database"));
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                using ServiceProvider provider = services.BuildServiceProvider();
                DbOptions options = provider.GetRequiredService<IOptions<DbOptions>>().Value;
                rb.AddPostgres()
                    .WithGlobalConnectionString(options.ConnectionString)
                    .ScanIn(typeof(ProductsTableInit).Assembly)
                    .For.Migrations();
            });
        services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<OrdersCreatingService>();
        services.AddSingleton(sp =>
        {
            DbOptions options = sp.GetRequiredService<IOptions<DbOptions>>().Value;
            var builder = new NpgsqlDataSourceBuilder(options.ConnectionString);
            builder.MapEnum<OrderHistoryItemKind>("order_history_item_kind");
            builder.MapEnum<OrderState>("order_state");
            return builder.Build();
        });

        return services;
    }
}