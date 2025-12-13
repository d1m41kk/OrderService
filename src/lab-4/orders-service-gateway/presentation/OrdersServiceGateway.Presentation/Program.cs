using OrdersCreationService.Application.Orders;
using OrdersServiceGateway.Presentation.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
builder.Services.AddSingleton<OrdersCreatingService>();

builder.Services.AddGateway(builder.Configuration);
WebApplication app = builder.Build();
app.UseGateway();
app.Run();