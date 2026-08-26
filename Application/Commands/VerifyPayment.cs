using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class VerifyPayment
    {
        public record VerifyPaymentCommand(string Reference) : IRequest<BaseResponse<VerifyPaymentResponse>>;

        public class VerifyPaymentValidator : AbstractValidator<VerifyPaymentCommand>
        {
            public VerifyPaymentValidator()
            {
                RuleFor(x => x.Reference)
                    .NotEmpty().WithMessage("Payment reference is required.");
            }
        }

        public class VerifyPaymentHandler(
            IPaymentRepository paymentRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IPaystackService paystackService,
            INotificationService notificationService,
            IEmailService emailService)
            : IRequestHandler<VerifyPaymentCommand, BaseResponse<VerifyPaymentResponse>>
        {
            public async Task<BaseResponse<VerifyPaymentResponse>> Handle(
                VerifyPaymentCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var payment = await paymentRepository.GetByReferenceAsync(request.Reference);
                    if (payment is null)
                        return BaseResponse<VerifyPaymentResponse>.Failure("Payment record not found.");

                    if (payment.Status == PaystackStatus.Successful)
                        return BaseResponse<VerifyPaymentResponse>.Success(
                            "Payment already verified.",
                            new VerifyPaymentResponse(payment.OrderId, "Successful"));

                    var verifyResult = await paystackService.VerifyTransactionAsync(request.Reference);

                    if (!verifyResult.Success)
                        return BaseResponse<VerifyPaymentResponse>.Failure(
                            $"Could not verify payment: {verifyResult.ErrorMessage}");

                    if (!verifyResult.IsPaid)
                    {
                        payment.Status = PaystackStatus.Failed;
                        payment.DateModified = DateTime.UtcNow;
                        paymentRepository.Update(payment);
                        await unitOfWork.SaveAsync();

                        return BaseResponse<VerifyPaymentResponse>.Failure("Payment was not successful.");
                    }

                    payment.Status = PaystackStatus.Successful;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.DateModified = DateTime.UtcNow;
                    paymentRepository.Update(payment);

                    var order = await orderRepository.GetOrderAsync(payment.OrderId);
                    if (order is not null)
                    {
                        order.Status = OrderStatus.Confirmed;
                        order.DateModified = DateTime.UtcNow;
                        orderRepository.Update(order);

                        var vendor = order.OrderListings.FirstOrDefault()?.Listing.Vendor;
                        if (vendor is not null)
                        {
                            await notificationService.SendNotificationAsync(
                                 vendor.UserId,
                                 "New Order Received",
                                 $"You have a new order ({order.OrderNo}) — payment confirmed.",
                                 NotificationType.NewOrder);

                            try
                            {
                                await emailService.SendOrderStatusEmailAsync(
                                    vendor.User.Email,
                                    order.OrderNo,
                                    "New Order Received",
                                    $"You have a new paid order ({order.OrderNo}). Payment has been confirmed.");
                            }
                            catch
                            {
                                // Email failed but verification succeeded
                            }

                            string fulfilmentText = order.FulfilmentType == FulfilmentType.Delivery
                                ? "arrange delivery" : "arrange pickup";

                            await notificationService.SendNotificationAsync(
                                order.Customer.UserId,
                                "Order Confirmed",
                                $"Your payment for order ({order.OrderNo}) was successful. {vendor.OrganizationName} will contact you shortly to {fulfilmentText}.",
                                NotificationType.OrderStatusChanged);

                            try
                            {
                                await emailService.SendOrderStatusEmailAsync(
                                    order.Customer.User.Email,
                                    order.OrderNo,
                                    "Order Confirmed",
                                    $"Your payment for order ({order.OrderNo}) was successful. {vendor.OrganizationName} will contact you shortly to {fulfilmentText}.");
                            }
                            catch
                            {
                                // Email failed but verification succeeded
                            }
                        }
                    }

                    await unitOfWork.SaveAsync();

                    return BaseResponse<VerifyPaymentResponse>.Success(
                        "Payment verified successfully. Order confirmed.",
                        new VerifyPaymentResponse(payment.OrderId, "Successful"));
                }
                catch (Exception ex)
                {
                    return BaseResponse<VerifyPaymentResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record VerifyPaymentResponse(Guid OrderId, string PaymentStatus);
    }
}