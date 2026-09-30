using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetFlaggedOrders
    {
        public record GetFlaggedOrdersQuery() : IRequest<BaseResponse<List<FlaggedOrderResponse>>>;

        public record FlaggedOrderResponse(
            Guid OrderId,
            string OrderNo,
            string VendorName,
            string CustomerName,
            string Note,
            DateTime FlaggedAt,
            bool VendorNotified);

        public class GetFlaggedOrdersHandler(IOrderRepository orderRepository)
            : IRequestHandler<GetFlaggedOrdersQuery, BaseResponse<List<FlaggedOrderResponse>>>
        {
            public async Task<BaseResponse<List<FlaggedOrderResponse>>> Handle(
                GetFlaggedOrdersQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var orders = await orderRepository.GetFlaggedOrdersAsync();

                    var response = orders
                        .OrderBy(o => o.OrderListings.First().Listing.Vendor.OrganizationName)
                        .ThenByDescending(o => o.DescriptionMismatchFlaggedAt)
                        .Select(o => new FlaggedOrderResponse(
                            o.Id,
                            o.OrderNo,
                            o.OrderListings.First().Listing.Vendor.OrganizationName,
                            o.Customer.User.FullName ?? o.Customer.User.UserName,
                            o.DescriptionMismatchNote ?? "",
                            o.DescriptionMismatchFlaggedAt ?? o.DateModified,
                            o.DescriptionMismatchVendorNotifiedAt.HasValue))
                        .ToList();

                    return BaseResponse<List<FlaggedOrderResponse>>.Success(
                        "Flagged orders retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<FlaggedOrderResponse>>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}