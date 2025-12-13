using Microsoft.AspNetCore.Mvc;
using Orders.ProcessingService.Contracts;
using OrdersServiceGateway.Presentation.Clients;

namespace OrdersServiceGateway.Presentation.Controllers;

[ApiController]
[Route("/order-processing/")]
public class OrderProcessingController : ControllerBase
{
    private readonly GrpcOrderProcessingClient _orderProcessingClient;

    public OrderProcessingController(GrpcOrderProcessingClient orderProcessingClient)
    {
        _orderProcessingClient = orderProcessingClient;
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApproveOrderAsync(
        [FromBody] ApproveOrderRequest request,
        CancellationToken cancellationToken)
    {
        await _orderProcessingClient.ApproveOrderAsync(
            request.OrderId,
            request.IsApproved,
            request.ApprovedBy,
            request.FailureReason,
            cancellationToken);

        return Ok(new ApproveOrderResponse());
    }

    [HttpPost("start-packing")]
    public async Task<IActionResult> StartPackingAsync(
        [FromBody] StartOrderPackingRequest request,
        CancellationToken cancellationToken)
    {
        await _orderProcessingClient.StartOrderPackingAsync(
            request.OrderId,
            request.PackingBy,
            cancellationToken);

        return Ok(new StartOrderPackingResponse());
    }

    [HttpPost("finish-packing")]
    public async Task<IActionResult> FinishPackingAsync(
        [FromBody] FinishOrderPackingRequest request,
        CancellationToken cancellationToken)
    {
        await _orderProcessingClient.FinishOrderPackingAsync(
            request.OrderId,
            request.IsSuccessful,
            request.FailureReason,
            cancellationToken);

        return Ok(new FinishOrderPackingResponse());
    }

    [HttpPost("start-delivery")]
    public async Task<IActionResult> StartDeliveryAsync(
        [FromBody] StartOrderDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        await _orderProcessingClient.StartOrderDeliveryAsync(
            request.OrderId,
            request.DeliveredBy,
            cancellationToken);

        return Ok(new StartOrderDeliveryResponse());
    }

    [HttpPost("finish-delivery")]
    public async Task<IActionResult> FinishDeliveryAsync(
        [FromBody] FinishOrderDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        await _orderProcessingClient.FinishOrderDeliveryAsync(
            request.OrderId,
            request.IsSuccessful,
            request.FailureReason,
            cancellationToken);

        return Ok(new FinishOrderDeliveryResponse());
    }
}