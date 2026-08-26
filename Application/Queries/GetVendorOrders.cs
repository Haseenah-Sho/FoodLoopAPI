using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetVendorOrders
    {
        public record GetVendorOrdersQuery(Guid VendorUserId) : IRequest<BaseResponse<ICollection<VendorOrderResponse>>>;

        public record VendorOrderResponse(
            Guid OrderId,
            string OrderNo,
            string CustomerName,
            string? CustomerPhoneNumber,
            decimal TotalAmount,
            string Status,
            string? DeliveryStatus,
            string FulfilmentType,
            DateTime OrderedOn);

        public class GetVendorOrdersHandler(
            IVendorRepository vendorRepository,
            IOrderRepository orderRepository)
            : IRequestHandler<GetVendorOrdersQuery, BaseResponse<ICollection<VendorOrderResponse>>>
        {
            public async Task<BaseResponse<ICollection<VendorOrderResponse>>> Handle(
                GetVendorOrdersQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<ICollection<VendorOrderResponse>>.Failure("Vendor profile not found.");

                    var orders = await orderRepository.GetOrdersByVendorAsync(vendor.Id);

                    var response = orders
                        .Where(o => !o.IsDeleted)
                        .OrderByDescending(o => o.DateCreated)
                        .Select(o => new VendorOrderResponse(
                            o.Id,
                            o.OrderNo,
                            o.Customer.User.FullName ?? o.Customer.User.UserName,
                            o.Customer.PhoneNumber,
                            o.TotalAmount,
                            o.Status.ToString(),
                            o.Delivery?.Status.ToString(),
                            o.FulfilmentType.ToString(),
                            o.DateCreated))
                        .ToList();

                    return BaseResponse<ICollection<VendorOrderResponse>>.Success(
                        "Orders retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<VendorOrderResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}