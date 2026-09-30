using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetOrders
    {
        public record GetOrdersQuery(Guid CustomerUserId) : IRequest<BaseResponse<ICollection<GetOrdersResponse>>>;

        public record GetOrdersResponse(
            Guid OrderId,
            string OrderNo,
            decimal TotalAmount,
            string Status,
            string? DeliveryStatus,
            string FulfilmentType,
            DateTime OrderedOn,
            List<string> ListingNames);

        public class GetOrdersHandler(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository)
            : IRequestHandler<GetOrdersQuery, BaseResponse<ICollection<GetOrdersResponse>>>
        {
            public async Task<BaseResponse<ICollection<GetOrdersResponse>>> Handle(
                GetOrdersQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<ICollection<GetOrdersResponse>>.Failure("Customer profile not found.");

                    var orders = await orderRepository.GetOrdersByCustomerAsync(customer.Id);

                    var response = orders.Select(o => new GetOrdersResponse(
                        o.Id,
                        o.OrderNo,
                        o.TotalAmount,
                        o.Status.ToString(),
                        o.Delivery?.Status.ToString(),
                        o.FulfilmentType.ToString(),
                        o.DateCreated,
                        o.OrderListings.Select(ol => ol.Listing.FoodName).ToList()
                    )).OrderByDescending(o => o.OrderedOn).ToList();

                    return BaseResponse<ICollection<GetOrdersResponse>>.Success(
                        "Orders retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<GetOrdersResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}