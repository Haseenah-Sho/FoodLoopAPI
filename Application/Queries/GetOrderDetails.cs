using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetOrderDetails
    {
        public record GetOrderDetailsQuery(Guid CustomerUserId, Guid OrderId) : IRequest<BaseResponse<GetOrderDetailsResponse>>;

        public record OrderItemResponse(string FoodName, int Quantity);

        public record GetOrderDetailsResponse(
            Guid OrderId,
            string OrderNo,
            decimal TotalAmount,
            string Status,
            string FulfilmentType,
            string? DeliveryAddress,
            DateTime OrderedOn,
            List<OrderItemResponse> Items);

        public class GetOrderDetailsHandler(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository)
            : IRequestHandler<GetOrderDetailsQuery, BaseResponse<GetOrderDetailsResponse>>
        {
            public async Task<BaseResponse<GetOrderDetailsResponse>> Handle(
                GetOrderDetailsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<GetOrderDetailsResponse>.Failure("Customer profile not found.");

                    var order = await orderRepository.GetOrderAsync(request.OrderId);
                    if (order is null)
                        return BaseResponse<GetOrderDetailsResponse>.Failure("Order not found.");

                    if (order.CustomerId != customer.Id)
                        return BaseResponse<GetOrderDetailsResponse>.Failure("You are not authorized to view this order.");

                    var response = new GetOrderDetailsResponse(
                        order.Id,
                        order.OrderNo,
                        order.TotalAmount,
                        order.Status.ToString(),
                        order.FulfilmentType.ToString(),
                        order.DeliveryAddress,
                        order.DateCreated,
                        order.OrderListings.Select(ol => new OrderItemResponse(
                            ol.Listing.FoodName, ol.Quantity)).ToList());

                    return BaseResponse<GetOrderDetailsResponse>.Success(
                        "Order details retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<GetOrderDetailsResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}