using Gateway.Application.Abstractions.Clients;
using Gateway.Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Presentation.Grpc;
using CreateOrderRequest = Gateway.Application.Models.Requests.CreateOrderRequest;
using CreateOrderResponse = Gateway.Application.Models.Responses.CreateOrderResponse;

namespace Gateway.Presentation.Controllers;

[ApiController]
[Route("/orders/")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IGrpcOrderClient _grpcOrderClient;

    public OrdersController(ILogger<OrdersController> logger, IGrpcOrderClient grpcOrderClient)
    {
        _grpcOrderClient = grpcOrderClient;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        long orderId = await _grpcOrderClient.CreateOrderAsync(request.CreatedBy, cancellationToken);
        return Ok(new CreateOrderResponse(orderId));
    }

    [HttpPost("add_product")]
    [ProducesResponseType(typeof(AddProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddProductResponse>> AddProduct(
        AddProductRequest request,
        CancellationToken cancellationToken)
    {
        await _grpcOrderClient.AddProductToOrderAsync(
            request.OrderId,
            request.ProductId,
            request.Quantity,
            cancellationToken);
        return Ok(new AddProductResponse());
    }

    [HttpPost("remove_product")]
    [ProducesResponseType(typeof(RemoveProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RemoveProductResponse>> RemoveProduct(
        RemoveProductRequest request,
        CancellationToken cancellationToken)
    {
        await _grpcOrderClient.RemoveProductFromOrderAsync(
            request.OrderId,
            request.ProductId,
            cancellationToken);
        return Ok(new RemoveProductResponse());
    }

    [HttpPatch("{orderId:long}/change_status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChangeStatusResponse>> ChangeStatus(
        [FromRoute] long orderId,
        [FromQuery] OrderState status,
        CancellationToken cancellationToken)
    {
        await _grpcOrderClient.ChangeOrderStatusAsync(orderId, status, cancellationToken);
        return Ok(new ChangeStatusResponse());
    }

    [HttpGet("{orderId:long}/order_history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetHistoryResponse>> GetHistory(
        [FromRoute] long orderId,
        [FromQuery] OrderHistoryItemKind? kind,
        [FromQuery] int pageSize,
        [FromQuery] string? pageToken,
        CancellationToken cancellationToken)
    {
        GetOrderHistoryResponse response =
            await _grpcOrderClient.GetOrderHistoryAsync(orderId, kind, pageSize, pageToken, cancellationToken);
        return Ok(response);
    }
}