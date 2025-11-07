using Task1Web.Presentation.Grpc.Interceptors;
using Task1Web.Presentation.Grpc.Models;
using Task1Web.Presentation.Grpc.Services;

namespace Task1Web.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GrpcOptions>(configuration.GetSection("Grpc"));
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ServerInterceptor>();
        });
        services.AddSingleton<ServerInterceptor>();
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