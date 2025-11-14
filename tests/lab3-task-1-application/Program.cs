using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Models.Orders;
using OrdersService.Application.Orders;
using OrdersService.Infrastructure;
using OrdersService.Infrastructure.Migrations;
using Task1.Extensions;
using Task1.Implementations;
using Task1.Interfaces;
using Task2.Implementations;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        var configurationBuilder = new ConfigurationBuilder();
        var customProvider = new CustomConfigurationProvider();
        configurationBuilder.Add(new CustomConfigurationProviderSource(customProvider));
        configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfiguration configuration = configurationBuilder.Build();

        services.AddSingleton(configuration);
        services.AddSingleton(customProvider);
        services.Configure<CustomConfigurationServiceOptions>(configuration.GetSection("CustomProviderOptions"));
        services.AddConfigClientRefit();
        services.AddTransient<IConfigurationServiceClient, RefitClientConfigurationService>();
        services.AddHostedService<CustomConfigurationService>();
        services.AddPersistence(configuration);
    })
    .ConfigureLogging(logging => logging.ClearProviders())
    .UseConsoleLifetime()
    .Build();

MigrationRunner.DropAllMigrations(host.Services);
MigrationRunner.RunMigrations(host.Services);

ProductService productService = host.Services.GetRequiredService<ProductService>();
long productId1 = productService.CreateProduct("tovar1", 100, CancellationToken.None).Id;
long productId2 = productService.CreateProduct("tovar2", 200, CancellationToken.None).Id;
long productId3 = productService.CreateProduct("tovar3", 300, CancellationToken.None).Id;

OrderService orderService = host.Services.GetRequiredService<OrderService>();
long orderId = await orderService.CreateOrder("ivan_zolo", CancellationToken.None);
await orderService.AddProductToOrder(orderId, productId1, 10, CancellationToken.None);
await orderService.AddProductToOrder(orderId, productId2, 20, CancellationToken.None);
await orderService.DeleteProductInOrder(orderId, productId2, CancellationToken.None);
await orderService.ChangeOrderStatusToProcessing(orderId, CancellationToken.None);
await orderService.ChangeOrderStatusToCompleted(orderId, CancellationToken.None);

QueryOrderHistoryResponse historyOfOrder =
    await orderService.GetOrderHistory(orderId, null, 10, null, CancellationToken.None);
if (historyOfOrder.OrderHistory != null)
{
    foreach (OrderHistory item in historyOfOrder.OrderHistory)
    {
        Console.WriteLine(item.OrderId);
        Console.WriteLine(item.OrderHistoryItemCreatedAt);
        Console.WriteLine(item.OrderHistoryItemKind);
        Console.WriteLine(item.OrderHistoryItemPayload);
    }
}

Console.WriteLine("Application is running. Press Ctrl+C to exit.");

await host.RunAsync();