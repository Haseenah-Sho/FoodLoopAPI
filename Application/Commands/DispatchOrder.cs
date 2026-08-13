using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands
{
    public class DispatchOrder
    {
        public record DispatchOrderCommand(Guid VendorUserId, string OrderNo) : IRequest<BaseResponse<DispatchOrderResponse>>;

        public class DispatchOrderValidator : AbstractValidator<DispatchOrderCommand>
        {
            public DispatchOrderValidator()
            {
                RuleFor(x => x.OrderNo)
                    .NotEmpty().WithMessage("Order number is required.");
            }
        }

        public class DispatchOrderHandler(
            IVendorRepository vendorRepository,
            IOrderRepository orderRepository,
            IDeliveryRepository deliveryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEmailService emailService)
            : IRequestHandler<DispatchOrderCommand, BaseResponse<DispatchOrderResponse>>
        {
            public async Task<BaseResponse<DispatchOrderResponse>> Handle(
                DispatchOrderCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<DispatchOrderResponse>.Failure("Vendor profile not found.");

                    var order = await orderRepository.GetOrderByOrderNoAsync(request.OrderNo);
                    if (order is null)
                        return BaseResponse<DispatchOrderResponse>.Failure("Order not found.");

                    var belongsToVendor = order.OrderListings
                        .Any(ol => ol.Listing.VendorId == vendor.Id);
                    if (!belongsToVendor)
                        return BaseResponse<DispatchOrderResponse>.Failure(
                            "This order does not belong to any of your food items.");

                    if (order.FulfilmentType != FulfilmentType.Delivery)
                        return BaseResponse<DispatchOrderResponse>.Failure("This order is not a delivery order.");

                    if (order.Status == OrderStatus.Cancelled)
                        return BaseResponse<DispatchOrderResponse>.Failure("This order has been cancelled.");

                    var existingDelivery = await deliveryRepository.GetByOrderAsync(order.Id);
                    Delivery delivery;

                    if (existingDelivery is not null)
                    {
                        if (existingDelivery.Status != DeliveryStatus.Pending)
                            return BaseResponse<DispatchOrderResponse>.Failure(
                                $"This order has already been {existingDelivery.Status}.");

                        existingDelivery.Status = DeliveryStatus.Dispatched;
                        existingDelivery.DispatchedAt = DateTime.UtcNow;
                        existingDelivery.DateModified = DateTime.UtcNow;

                        deliveryRepository.Update(existingDelivery);
                        delivery = existingDelivery;
                    }
                    else
                    {
                        delivery = new Delivery
                        {
                            OrderId = order.Id,
                            DeliveryAddress = order.DeliveryAddress ?? string.Empty,
                            Status = DeliveryStatus.Dispatched,
                            DispatchedAt = DateTime.UtcNow,
                            Fee = order.TotalAmount - order.OrderListings
                                .Sum(ol => ol.Listing.IsFree ? 0 : (ol.Listing.Price ?? 0) * ol.Quantity),
                            CreatedBy = vendor.User.Email,
                        };

                        await deliveryRepository.AddAsync(delivery);
                    }

                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(
                        order.Customer.UserId,
                        "Order Dispatched",
                        $"Your order ({order.OrderNo}) is on its way!",
                        NotificationType.OrderStatusChanged);

                    try
                    {
                        await emailService.SendOrderStatusEmailAsync(
                            order.Customer.User.Email,
                            order.OrderNo,
                            "Order Dispatched",
                            "Your order is on its way!");
                    }
                    catch
                    {
                        // Email failed but dispatch succeeded
                    }

                    return BaseResponse<DispatchOrderResponse>.Success(
                        "Your order has been dispatched.",
                        new DispatchOrderResponse(order.Id, order.OrderNo, delivery.Status.ToString(), delivery.DispatchedAt));
                }
                catch (Exception ex)
                {
                    return BaseResponse<DispatchOrderResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record DispatchOrderResponse(Guid OrderId, string OrderNo, string DeliveryStatus, DateTime? DispatchedAt);
    }
}