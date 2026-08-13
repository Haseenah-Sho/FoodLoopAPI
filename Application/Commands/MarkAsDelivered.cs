using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;
using static Application.Commands.MarkAsDelivered;

namespace Application.Commands
{
    public class MarkAsDelivered
    {
        public record MarkAsDeliveredCommand(Guid VendorUserId, string OrderNo) : IRequest<BaseResponse<MarkAsDeliveredResponse>>;

        public class MarkAsDeliveredValidator : AbstractValidator<MarkAsDeliveredCommand>
        {
            public MarkAsDeliveredValidator()
            {
                RuleFor(x => x.OrderNo)
                    .NotEmpty().WithMessage("Order number is required.");
            }
        }

        public class MarkAsDeliveredHandler(
            IVendorRepository vendorRepository,
            IOrderRepository orderRepository,
            IDeliveryRepository deliveryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEmailService emailService)
            : IRequestHandler<MarkAsDeliveredCommand, BaseResponse<MarkAsDeliveredResponse>>
        {
            public async Task<BaseResponse<MarkAsDeliveredResponse>> Handle(
                MarkAsDeliveredCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<MarkAsDeliveredResponse>.Failure("Vendor profile not found.");

                    var order = await orderRepository.GetOrderByOrderNoAsync(request.OrderNo);
                    if (order is null)
                        return BaseResponse<MarkAsDeliveredResponse>.Failure("Order not found.");

                    var belongsToVendor = order.OrderListings
                        .Any(ol => ol.Listing.VendorId == vendor.Id);
                    if (!belongsToVendor)
                        return BaseResponse<MarkAsDeliveredResponse>.Failure(
                            "This order does not belong to any of your food items.");

                    var delivery = await deliveryRepository.GetByOrderAsync(order.Id);
                    if (delivery is null)
                        return BaseResponse<MarkAsDeliveredResponse>.Failure("No delivery record found for this order.");

                    if (delivery.Status != DeliveryStatus.Dispatched)
                        return BaseResponse<MarkAsDeliveredResponse>.Failure(
                            "Order must be dispatched before it can be marked as delivered.");

                    delivery.Status = DeliveryStatus.Delivered;
                    delivery.DeliveredAt = DateTime.UtcNow;
                    delivery.DateModified = DateTime.UtcNow;
                    deliveryRepository.Update(delivery);

                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(
                    order.Customer.UserId,
                    "Order Delivered",
                    $"Your order ({order.OrderNo}) has been delivered. Please confirm you received it in the app.",
                    NotificationType.OrderStatusChanged);

                    try
                    {
                        await emailService.SendOrderStatusEmailAsync(
                            order.Customer.User.Email,
                            order.OrderNo,
                            "Order Delivered",
                            "Your order has delivered. Please open the app and confirm you received it.");
                    }
                    catch
                    {
                        // Email failed but delivery update succeeded
                    }

                    return BaseResponse<MarkAsDeliveredResponse>.Success(
                        "Marked as delivered. Waiting on the customer to confirm receipt.",
                        new MarkAsDeliveredResponse(order.Id, order.OrderNo, order.Status.ToString(), delivery.DeliveredAt));
                }
                catch (Exception ex)
                {
                    return BaseResponse<MarkAsDeliveredResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record MarkAsDeliveredResponse(Guid OrderId, string OrderNo, string OrderStatus, DateTime? DeliveredAt);
    }
}