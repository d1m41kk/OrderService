using HttpGateway.Interfaces;
using HttpGateway.Models.Responses;
using HttpGateway.Protos;
using Microsoft.AspNetCore.Mvc;
using CreateOrderRequest = HttpGateway.Models.Requests.CreateOrderRequest;
using CreateOrderResponse = HttpGateway.Models.Responses.CreateOrderResponse;

namespace HttpGateway.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IGrpcOrderClient _grpcOrderClient;

    public OrdersController(ILogger<OrdersController> logger, IGrpcOrderClient grpcOrderClient)
    {
        _logger = logger;
        _grpcOrderClient = grpcOrderClient;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResponseModel<CreateOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(CreateOrderRequest request)
    {
        _logger.LogInformation("CreateOrder request by: {name}", request.CreatedBy);
        long orderId = await _grpcOrderClient.CreateOrderAsync(request.CreatedBy);
        return Ok(new ResponseModel<CreateOrderResponse>(new CreateOrderResponse(orderId), "Order created"));
    }

    [HttpPost("add_product")]
    [ProducesResponseType(typeof(ResponseModel<AddProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseModel<AddProductResponse>>> AddProduct(AddProductRequest request)
    {
        _logger.LogInformation("Adding product {ProductId} to order {OrderID}", request.ProductId, request.OrderId);
        await _grpcOrderClient.AddProductToOrderAsync(request.OrderId, request.ProductId, request.Quantity);
        return Ok(new ResponseModel<AddProductRequest>(new AddProductRequest(), "Product added"));
    }

    [HttpPost("remove_product")]
    [ProducesResponseType(typeof(ResponseModel<RemoveProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseModel<RemoveProductResponse>>> RemoveProduct(RemoveProductRequest request)
    {
        _logger.LogInformation("Removing product {ProductId}", request.ProductId);
        await _grpcOrderClient.RemoveProductFromOrderAsync(request.OrderId, request.ProductId);
        return Ok(new ResponseModel<RemoveProductRequest>(new RemoveProductRequest(), "Product removed"));
    }

    [HttpPatch("{orderId:long}/change_status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseModel<ChangeStatusResponse>>> ChangeStatus(
        [FromRoute] long orderId,
        [FromQuery] string status)
    {
        _logger.LogInformation("Changing status of order {OrderId}", orderId);
        await _grpcOrderClient.ChangeOrderStatusAsync(orderId, status);
        return Ok(new ResponseModel<ChangeStatusResponse>(new ChangeStatusResponse(), "Order status changed"));
    }

    [HttpGet("{orderId:long}/order_history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseModel<GetHistoryResponse>>> GetHistory(
        [FromRoute] long orderId,
        [FromQuery] string? kind,
        [FromQuery] int pageSize,
        [FromQuery] string? pageToken)
    {
        _logger.LogInformation("Getting history of order {OrderId}", orderId);
        GetHistoryResponse response = await _grpcOrderClient.GetOrderHistoryAsync(orderId, kind, pageSize, pageToken);
        return Ok(response);
    }
}