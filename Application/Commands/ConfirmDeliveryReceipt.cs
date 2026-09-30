using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class ConfirmDeliveryReceipt
    {
        public record ConfirmDeliveryReceiptCommand(Guid CustomerUserId, Guid OrderId)
            : IRequest<BaseResponse<string>>;

        public class ConfirmDeliveryReceiptHandler(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
            : IRequestHandler<ConfirmDeliveryReceiptCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                ConfirmDeliveryReceiptCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<string>.Failure("Customer profile not found.");

                    var order = await orderRepository.GetOrderAsync(request.OrderId);
                    if (order is null || order.CustomerId != customer.Id)
                        return BaseResponse<string>.Failure("Order not found.");

                    if (order.FulfilmentType != FulfilmentType.Delivery)
                        return BaseResponse<string>.Failure("This order is not a delivery order.");

                    if (order.Delivery is null || order.Delivery.Status != DeliveryStatus.Delivered)
                        return BaseResponse<string>.Failure("This order hasn't been delivered yet.");

                    if (order.Status == OrderStatus.Completed)
                        return BaseResponse<string>.Failure("You've already confirmed this order.");

                    order.Status = OrderStatus.Completed;
                    order.DateModified = DateTime.UtcNow;
                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    var vendor = order.OrderListings.FirstOrDefault()?.Listing.Vendor;
                    if (vendor is not null)
                    {
                        await notificationService.SendNotificationAsync(
                            vendor.UserId,
                            "Delivery Confirmed",
                            $"The customer confirmed receipt of order ({order.OrderNo}).",
                            NotificationType.OrderStatusChanged);
                    }

                    return BaseResponse<string>.Success("Thanks for confirming - order completed.", "Completed");
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}