using Application.Common.Dtos;
using Application.Repositories;
using Application.Services;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class ResumePayment
    {
        public record ResumePaymentCommand(Guid CustomerUserId, Guid OrderId)
            : IRequest<BaseResponse<ResumePaymentResponse>>;

        public class ResumePaymentHandler(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            IPaystackService paystackService)
            : IRequestHandler<ResumePaymentCommand, BaseResponse<ResumePaymentResponse>>
        {
            public async Task<BaseResponse<ResumePaymentResponse>> Handle(
                ResumePaymentCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<ResumePaymentResponse>.Failure("Customer profile not found.");

                    var order = await orderRepository.GetOrderAsync(request.OrderId);
                    if (order is null)
                        return BaseResponse<ResumePaymentResponse>.Failure("Order not found.");

                    if (order.CustomerId != customer.Id)
                        return BaseResponse<ResumePaymentResponse>.Failure("You are not authorized to pay for this order.");

                    if (order.Status != OrderStatus.Pending)
                        return BaseResponse<ResumePaymentResponse>.Failure(
                            "This order is not awaiting payment.");

                    var paymentReference = $"PAY-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

                    var initResult = await paystackService.InitializeTransactionAsync(
                        customer.User.Email, order.TotalAmount, paymentReference);

                    if (!initResult.Success)
                        return BaseResponse<ResumePaymentResponse>.Failure(
                            $"Payment initialization failed: {initResult.ErrorMessage}");

                    if (order.Payment is not null)
                    {
                        order.Payment.PaystackReference = initResult.Reference;
                        order.Payment.Status = PaystackStatus.Pending;
                        order.Payment.DateModified = DateTime.UtcNow;
                        paymentRepository.Update(order.Payment);
                    }

                    await unitOfWork.SaveAsync();

                    return BaseResponse<ResumePaymentResponse>.Success(
                        "Payment initialized.",
                        new ResumePaymentResponse(initResult.AuthorizationUrl!, initResult.Reference!));
                }
                catch (Exception ex)
                {
                    return BaseResponse<ResumePaymentResponse>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }

        public record ResumePaymentResponse(string PaymentAuthorizationUrl, string PaymentReference);
    }
}