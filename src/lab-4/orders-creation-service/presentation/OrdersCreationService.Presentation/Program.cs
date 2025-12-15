using Orders.Kafka.Contracts;
using OrdersCreationService.Application.Abstractions.Persistence.Producers;
using OrdersCreationService.Application.Abstractions.Persistence.Repositories;
using OrdersCreationService.Application.Grpc.Extensions;
using OrdersCreationService.Application.KafkaHandlers;
using OrdersCreationService.Application.KafkaServices;
using OrdersCreationService.Application.Models.KafkaOptions;
using OrdersCreationService.Application.Orders;
using OrdersCreationService.Infrastructure;
using OrdersCreationService.Infrastructure.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Services.Configure<ProducerOptions>(builder.Configuration.GetSection("Kafka:Producer"));
builder.Services.Configure<ConsumerOptions>(builder.Configuration.GetSection("Kafka:Consumer"));
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();

builder.Services.AddSingleton<IKafkaMessageProducer<OrderCreationKey, OrderCreationValue>, KafkaOrderCreationProducer>();
builder.Services.AddScoped<OrderProcessingEventsHandler>();
builder.Services.AddHostedService<KafkaOrderCreationConsumer>();
builder.Services.AddScoped<OrdersCreatingService>();

builder.Services.AddGrpcServices(builder.Configuration);

WebApplication app = builder.Build();

app.GrpcWebApplication();

await app.RunAsync();