using Gateway.Kafka.Contracts;
using Grpc.Net.Client;
using OrdersCreationService.Application.Grpc.Models;
using OrdersServiceGateway.Applicarion.Abstractions.Clients;
using OrdersServiceGateway.Application.Models.Options;
using OrdersServiceGateway.Presentation.Clients;
using OrdersServiceGateway.Presentation.Middleware;
using System.Text.Json;

namespace OrdersServiceGateway.Presentation.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
    {
        GrpcProcessingOptions? processingOptions = configuration.GetSection("Grpc:Processing").Get<GrpcProcessingOptions>();
        GrpcOptions? creationOptions = configuration.GetSection("Grpc:Creation").Get<GrpcOptions>();

        services.AddScoped(_ =>
        {
            string address = $"http://{creationOptions?.Host ?? "localhost"}:{creationOptions?.Port ?? 8081}";
            var channel = GrpcChannel.ForAddress(address);
            return new OrderCreationService.OrderCreationServiceClient(channel);
        });

        services.AddScoped(_ =>
        {
            string address = $"http://{processingOptions?.Host ?? "localhost"}:{processingOptions?.Port ?? 8080}";
            var channel = GrpcChannel.ForAddress(address);
            return new OrderService.OrderServiceClient(channel);
        });

        services.AddScoped<IGrpcOrderClient, GrpcOrderCreatingClient>();
        services.AddScoped<GrpcOrderProcessingClient>();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.WriteIndented = true;
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static WebApplication UseGateway(this WebApplication app)
    {
        app.UseMiddleware<GrpcExceptionHandler>();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        return app;
    }
}