using Microsoft.AspNetCore.Mvc;
using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersServiceGateway.Applicarion.Abstractions.Clients;
using CreateOrderRequest = OrdersServiceGateway.Application.Models.Requests.CreateOrderRequest;
using CreateOrderResponse = OrdersServiceGateway.Application.Models.Responses.CreateOrderResponse;

namespace OrdersServiceGateway.Presentation.Controllers;

[ApiController]
[Route("/orders-creation/")]
public class OrderCreationController : ControllerBase
{
    private readonly IGrpcOrderClient _client;

    public OrderCreationController(IGrpcOrderClient client)
    {
        _client = client;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        long orderId = await _client.CreateOrderAsync(request.CreatedBy, cancellationToken);
        return Ok(new CreateOrderResponse(orderId));
    }

    [HttpPost("start-processing")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> StartProcessing(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _client.ChangeOrderStatusAsync(orderId, OrderState.Processing, cancellationToken);
        return Ok(new CreateOrderResponse(orderId));
    }

    [HttpPost("cancel-order")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> CancelOrder(long orderId, CancellationToken cancellationToken)
    {
        bool response = await _client.CancelOrderAsync(orderId, cancellationToken);
        return Ok(response);
    }
}