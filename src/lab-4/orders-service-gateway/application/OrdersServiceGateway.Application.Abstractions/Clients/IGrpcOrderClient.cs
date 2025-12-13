using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersServiceGateway.Applicarion.Abstractions.Clients;

public interface IGrpcOrderClient
{
    Task<long> CreateOrderAsync(string createdBy, CancellationToken cancellationToken);

    Task ChangeOrderStatusAsync(long orderId, OrderState status, CancellationToken cancellationToken);

    Task<bool> CancelOrderAsync(long orderId, CancellationToken cancellationToken);
}