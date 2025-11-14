using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using OrdersService.Application.Abstractions.Persistence.Repositories;
using OrdersService.Application.Models.Orders;
using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Application.Orders;
using OrdersService.Infrastructure.Migrations;
using OrdersService.Infrastructure.Repositories;

namespace OrdersService.Infrastructure;

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
        services.AddScoped<OrderService>();
        services.AddScoped<ProductService>();
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