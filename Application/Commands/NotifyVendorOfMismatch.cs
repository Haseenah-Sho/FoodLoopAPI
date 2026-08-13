using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class NotifyVendorOfMismatch
    {
        public record NotifyVendorOfMismatchCommand(Guid OrderId) : IRequest<BaseResponse<string>>;

        public class NotifyVendorOfMismatchHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEmailService emailService)
            : IRequestHandler<NotifyVendorOfMismatchCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                NotifyVendorOfMismatchCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var order = await orderRepository.GetOrderAsync(request.OrderId);
                    if (order is null)
                        return BaseResponse<string>.Failure("Order not found.");

                    if (!order.DescriptionMismatchFlagged)
                        return BaseResponse<string>.Failure("This order has no mismatch report to notify about.");

                    if (order.DescriptionMismatchVendorNotifiedAt is not null)
                        return BaseResponse<string>.Failure("This vendor has already been notified about this report.");

                    var vendor = order.OrderListings.FirstOrDefault()?.Listing.Vendor;
                    if (vendor is null)
                        return BaseResponse<string>.Failure("Vendor for this order could not be found.");

                    var message = $"A customer reported that the item on order {order.OrderNo} didn't match its description: \"{order.DescriptionMismatchNote}\". Please review your food item details and make sure descriptions accurately reflect your food.";

                    await notificationService.SendNotificationAsync(
                        vendor.UserId, "Description Mismatch Reported", message, NotificationType.StrikeIssued);

                    try
                    {
                        await emailService.SendOrderStatusEmailAsync(
                            vendor.User.Email, order.OrderNo, "Description Mismatch Reported", message);
                    }
                    catch
                    {
                        // Email failed but notification succeeded
                    }

                    order.DescriptionMismatchVendorNotifiedAt = DateTime.UtcNow;
                    order.DateModified = DateTime.UtcNow;
                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success("Vendor has been notified.", "Notified");
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}