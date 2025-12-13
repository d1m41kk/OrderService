using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrdersCreationService.Application.Grpc.Interceptors;
using OrdersCreationService.Application.Grpc.Models;
using OrdersCreationService.Application.Grpc.Services;

namespace OrdersCreationService.Application.Grpc.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.AddSingleton<RequestInterceptor>();
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<RequestInterceptor>();
        });
        services.AddScoped<GrpcOrderService>();
        return services;
    }

    public static WebApplication GrpcWebApplication(this WebApplication app)
    {
        GrpcOptions? options = app.Configuration.GetSection("Grpc").Get<GrpcOptions>();
        app.MapGrpcService<GrpcOrderService>();
        return app;
    }
}