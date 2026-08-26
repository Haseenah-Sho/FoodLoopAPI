using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class VerifyPickup
    {
        public record VerifyPickupCommand(Guid VendorUserId, string OrderNo) : IRequest<BaseResponse<VerifyPickupResponse>>;

        public class VerifyPickupValidator : AbstractValidator<VerifyPickupCommand>
        {
            public VerifyPickupValidator()
            {
                RuleFor(x => x.OrderNo)
                    .NotEmpty().WithMessage("Order number is required.");
            }
        }

        public class VerifyPickupHandler(
            IVendorRepository vendorRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEmailService emailService)
            : IRequestHandler<VerifyPickupCommand, BaseResponse<VerifyPickupResponse>>
        {
            public async Task<BaseResponse<VerifyPickupResponse>> Handle(
                VerifyPickupCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<VerifyPickupResponse>.Failure("Vendor profile not found.");

                    var order = await orderRepository.GetOrderByOrderNoAsync(request.OrderNo);
                    if (order is null)
                        return BaseResponse<VerifyPickupResponse>.Failure("Order not found.");

                    var belongsToVendor = order.OrderListings
                        .Any(ol => ol.Listing.VendorId == vendor.Id);
                    if (!belongsToVendor)
                        return BaseResponse<VerifyPickupResponse>.Failure(
                            "This order does not belong to any of your listed food items.");

                    if (order.FulfilmentType != FulfilmentType.PickUp)
                        return BaseResponse<VerifyPickupResponse>.Failure("This order is not a pickup order.");

                    if (order.Status == OrderStatus.Completed)
                        return BaseResponse<VerifyPickupResponse>.Failure("This order has already been picked up.");

                    if (order.Status == OrderStatus.Cancelled)
                        return BaseResponse<VerifyPickupResponse>.Failure("This order has been cancelled.");

                    order.Status = OrderStatus.Completed;
                    order.DateModified = DateTime.UtcNow;

                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    await notificationService.SendNotificationAsync(
                        order.Customer.UserId,
                        "Order Picked Up",
                        $"Your order ({order.OrderNo}) has been picked up. Enjoy your meal!",
                        NotificationType.OrderStatusChanged);

                    try
                    {
                        await emailService.SendOrderStatusEmailAsync(
                            order.Customer.User.Email,
                            order.OrderNo,
                            "Order Picked Up",
                            "Your order has been picked up. Enjoy your meal!");
                    }
                    catch
                    {
                        // Email failed but pickup verification succeeded
                    }

                    return BaseResponse<VerifyPickupResponse>.Success(
                        "Pickup verified successfully. Order completed.",
                        new VerifyPickupResponse(order.Id, order.OrderNo, order.Status.ToString()));
                }
                catch (Exception ex)
                {
                    return BaseResponse<VerifyPickupResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record VerifyPickupResponse(Guid OrderId, string OrderNo, string Status);
    }
}