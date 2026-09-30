using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetCustomerTransactionHistory
    {
        public record GetCustomerTransactionHistoryQuery(Guid CustomerUserId) : IRequest<BaseResponse<ICollection<GetCustomerTransactionHistoryResponse>>>;

        public record GetCustomerTransactionHistoryResponse(
            Guid PaymentId,
            Guid OrderId,
            string OrderNo,
            decimal Amount,
            string Status,
            string? PaystackReference,
            DateTime? PaidAt,
            DateTime CreatedOn);

        public class GetCustomerTransactionHistoryHandler(
            ICustomerRepository customerRepository,
            IPaymentRepository paymentRepository)
            : IRequestHandler<GetCustomerTransactionHistoryQuery, BaseResponse<ICollection<GetCustomerTransactionHistoryResponse>>>
        {
            public async Task<BaseResponse<ICollection<GetCustomerTransactionHistoryResponse>>> Handle(
                GetCustomerTransactionHistoryQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<ICollection<GetCustomerTransactionHistoryResponse>>.Failure("Customer profile not found.");

                    var payments = await paymentRepository.GetByUserAsync(customer.UserId);

                    var response = payments
                        .OrderByDescending(p => p.DateCreated)
                        .Select(p => new GetCustomerTransactionHistoryResponse(
                            p.Id,
                            p.OrderId,
                            p.Order.OrderNo,
                            p.Amount ?? 0,
                            p.Status.ToString(),
                            p.PaystackReference,
                            p.PaidAt,
                            p.DateCreated))
                        .ToList();

                    return BaseResponse<ICollection<GetCustomerTransactionHistoryResponse>>.Success(
                        "Payment history retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<GetCustomerTransactionHistoryResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}