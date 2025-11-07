using Task1.Extensions;
using Task1.Implementations;
using Task1.Interfaces;
using Task1Web.Domain.Models;
using Task1Web.Domain.Services;
using Task1Web.Infrastructure.Extensions;
using Task2.Implementations;
using MigrationRunner = Task1Web.Infrastructure.Migrations.MigrationRunner;

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

// MigrationRunner.DropAllMigrations(host.Services);
MigrationRunner.RunMigrations(host.Services);

ProductService productService = host.Services.GetRequiredService<ProductService>();
long productId1 = await productService.CreateProduct("tovar1", 100);
long productId2 = await productService.CreateProduct("tovar2", 200);
long productId3 = await productService.CreateProduct("tovar3", 300);

OrderService orderService = host.Services.GetRequiredService<OrderService>();
long orderId = await orderService.CreateOrder("ivan_zolo");
await orderService.AddProductToOrder(orderId, productId1, 10);
await orderService.AddProductToOrder(orderId, productId2, 20);
await orderService.DeleteProductInOrder(orderId, productId2);
await orderService.ChangeOrderStatusToProcessing(orderId);
await orderService.ChangeOrderStatusToCompleted(orderId);

QueryOrderHistoryResponse historyOfOrder = await orderService.GetOrderHistory(orderId, null, 10, null);
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