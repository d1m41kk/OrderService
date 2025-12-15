using OrdersService.Presentation.Grpc.Interceptors;
using OrdersService.Presentation.Grpc.Models;
using OrdersService.Presentation.Grpc.Services;

namespace OrdersService.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<RequestInterceptor>();
        });
        services.AddSingleton<RequestInterceptor>();
        services.AddScoped<GrpcOrderService>();
        services.AddScoped<GrpcProductService>();
        return services;
    }

    public static WebApplication GrpcWebApplication(this WebApplication app)
    {
        GrpcOptions? options = app.Configuration.GetSection("Grpc").Get<GrpcOptions>();
        app.MapGrpcService<GrpcOrderService>();
        app.MapGrpcService<GrpcProductService>();
        return app;
    }
}