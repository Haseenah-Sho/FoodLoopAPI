using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetOrderByOrderNo
    {
        public record GetOrderByOrderNoQuery(Guid VendorUserId, string OrderNo) : IRequest<BaseResponse<GetOrderByOrderNoResponse>>;

        public record GetOrderByOrderNoResponse(
            Guid OrderId,
            string OrderNo,
            string Status,
            string FulfilmentType,
            string CustomerName,
            string CustomerPhoneNumber,
            List<OrderItemByOrderNoResponse> Items);

        public record OrderItemByOrderNoResponse(string FoodName, int Quantity);

        public class GetOrderByOrderNoHandler(
            IVendorRepository vendorRepository,
            IOrderRepository orderRepository)
            : IRequestHandler<GetOrderByOrderNoQuery, BaseResponse<GetOrderByOrderNoResponse>>
        {
            public async Task<BaseResponse<GetOrderByOrderNoResponse>> Handle(
                GetOrderByOrderNoQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<GetOrderByOrderNoResponse>.Failure("Vendor profile not found.");

                    var order = await orderRepository.GetOrderByOrderNoAsync(request.OrderNo);
                    if (order is null)
                        return BaseResponse<GetOrderByOrderNoResponse>.Failure("Order not found.");

                    var belongsToVendor = order.OrderListings
                        .Any(ol => ol.Listing.VendorId == vendor.Id);

                    if (!belongsToVendor)
                        return BaseResponse<GetOrderByOrderNoResponse>.Failure(
                            "This order does not belong to any of your food items.");

                    var response = new GetOrderByOrderNoResponse(
                        order.Id,
                        order.OrderNo,
                        order.Status.ToString(),
                        order.FulfilmentType.ToString(),
                        order.Customer.User.FullName ?? order.Customer.User.UserName,
                        order.Customer.PhoneNumber,
                        order.OrderListings
                            .Where(ol => ol.Listing.VendorId == vendor.Id)
                            .Select(ol => new OrderItemByOrderNoResponse(ol.Listing.FoodName, ol.Quantity))
                            .ToList());

                    return BaseResponse<GetOrderByOrderNoResponse>.Success(
                        "Order found.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<GetOrderByOrderNoResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}