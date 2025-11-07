using Grpc.Net.Client;
using HttpGateway.Clients;
using HttpGateway.Interfaces;
using HttpGateway.Middleware;
using System.Text.Json;
using Task1Web.Presentation.Grpc.Models;

namespace HttpGateway.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.AddSingleton(sp =>
        {
            GrpcOptions? grpcOptions = configuration.GetSection("Grpc").Get<GrpcOptions>();
            string address = $"http://{grpcOptions?.Host ?? "localhost"}:{grpcOptions?.Port ?? 5000}";
            return GrpcChannel.ForAddress(address);
        });

        services.AddScoped<IGrpcOrderClient, GrpcOrderClient>();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
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