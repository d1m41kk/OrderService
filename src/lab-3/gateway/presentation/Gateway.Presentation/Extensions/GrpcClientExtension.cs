using Gateway.Presentation.Protos;
using OrdersService.Application.Models.Orders.Enums;

namespace Gateway.Presentation.Extensions;

public static class GrpcClientExtension
{
    public static OrderStatus MapOrderStateToGrpcOrderStatus(OrderState status)
    {
        OrderStatus grpcStatus = status switch
        {
            OrderState.Processing => OrderStatus.Processing,
            OrderState.Completed => OrderStatus.Completed,
            OrderState.Created => OrderStatus.Created,
            OrderState.Cancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentException("Unknown order status"),
        };
        return grpcStatus;
    }

    public static HistoryItemKind MapOrderHistoryItemKindToGrpcHistoryItemKind(OrderHistoryItemKind? kind)
    {
        HistoryItemKind itemKind = kind switch
        {
            OrderHistoryItemKind.Created => HistoryItemKind.CreatedItem,
            OrderHistoryItemKind.ItemAdded => HistoryItemKind.ItemAdded,
            OrderHistoryItemKind.ItemRemoved => HistoryItemKind.ItemRemoved,
            OrderHistoryItemKind.StateChanged => HistoryItemKind.StateChanged,
            _ => throw new ArgumentException("Unknown kind"),
        };
        return itemKind;
    }
}