using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetVendorTransactionHistory
    {
        public record GetVendorTransactionHistoryQuery(Guid VendorUserId) : IRequest<BaseResponse<ICollection<GetVendorTransactionHistoryResponse>>>;

        public record GetVendorTransactionHistoryResponse(
            Guid OrderId,
            string OrderNo,
            string CustomerName,
            decimal Amount,
            string PaymentStatus,
            DateTime? PaidAt);

        public class GetVendorTransactionHistoryHandler(
            IVendorRepository vendorRepository,
            IOrderRepository orderRepository)
            : IRequestHandler<GetVendorTransactionHistoryQuery, BaseResponse<ICollection<GetVendorTransactionHistoryResponse>>>
        {
            public async Task<BaseResponse<ICollection<GetVendorTransactionHistoryResponse>>> Handle(
                GetVendorTransactionHistoryQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<ICollection<GetVendorTransactionHistoryResponse>>.Failure("Vendor profile not found.");

                    var orders = await orderRepository.GetOrdersByVendorAsync(vendor.Id);

                    var response = orders
                        .Where(o => o.Payment is not null)
                        .OrderByDescending(o => o.DateCreated)
                        .Select(o => new GetVendorTransactionHistoryResponse(
                            o.Id,
                            o.OrderNo,
                            o.Customer.User.FullName ?? o.Customer.User.UserName,
                            o.Payment!.Amount ?? 0,
                            o.Payment!.Status.ToString(),
                            o.Payment!.PaidAt))
                        .ToList();

                    return BaseResponse<ICollection<GetVendorTransactionHistoryResponse>>.Success(
                        "Transactions retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<GetVendorTransactionHistoryResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}