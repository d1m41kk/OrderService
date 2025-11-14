using Microsoft.Extensions.Options;
using OrdersService.Application.Models.Orders;
using OrdersService.Infrastructure;
using OrdersService.Presentation.Grpc.Extensions;
using Task1.Extensions;
using Task1.Implementations;
using Task1.Interfaces;
using Task2.Implementations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var configurationBuilder = new ConfigurationBuilder();
var customProvider = new CustomConfigurationProvider();
configurationBuilder.Add(new CustomConfigurationProviderSource(customProvider));
configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
IConfiguration configuration = configurationBuilder.Build();

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton(customProvider);
builder.Services.Configure<CustomConfigurationServiceOptions>(configuration.GetSection("CustomProviderOptions"));
builder.Services.AddConfigClientRefit();
builder.Services.AddTransient<IConfigurationServiceClient, RefitClientConfigurationService>();
builder.Services.AddHostedService<CustomConfigurationService>();
builder.Services.AddPersistence(configuration);

builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

builder.Services.AddGrpcServices(builder.Configuration);

WebApplication app = builder.Build();
Console.WriteLine(app.Services.GetRequiredService<IOptionsMonitor<DbOptions>>().CurrentValue.ConnectionString);
app.GrpcWebApplication();
app.Run();