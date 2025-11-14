using Gateway.Application.Abstractions.Clients;
using Gateway.Presentation.Clients;
using Gateway.Presentation.Middleware;
using Gateway.Presentation.Protos;
using Grpc.Net.Client;
using OrdersService.Presentation.Grpc.Models;
using System.Text.Json;

namespace Gateway.Presentation.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.AddSingleton(_ =>
        {
            GrpcOptions? grpcOptions = configuration.GetSection("Grpc").Get<GrpcOptions>();
            string address = $"http://{grpcOptions?.Host ?? "localhost"}:{grpcOptions?.Port ?? 5000}";
            return GrpcChannel.ForAddress(address);
        });

        services.AddScoped(provider =>
        {
            GrpcChannel channel = provider.GetRequiredService<GrpcChannel>();
            return new OrderService.OrderServiceClient(channel);
        });

        services.AddScoped<IGrpcOrderClient, GrpcOrderClient>();
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