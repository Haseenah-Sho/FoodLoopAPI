using Application.Common.Dtos;
using Application.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class CancelOrder
    {
        public record CancelOrderCommand(Guid CustomerUserId, Guid OrderId)
            : IRequest<BaseResponse<string>>;

        public class CancelOrderHandler(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IListingRepository listingRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<CancelOrderCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                CancelOrderCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<string>.Failure("Customer profile not found.");

                    var order = await orderRepository.GetOrderAsync(request.OrderId);
                    if (order is null)
                        return BaseResponse<string>.Failure("Order not found.");

                    if (order.CustomerId != customer.Id)
                        return BaseResponse<string>.Failure("You are not authorized to cancel this order.");

                    if (order.Status != OrderStatus.Pending)
                        return BaseResponse<string>.Failure(
                            "Only orders pending payment can be cancelled.");

                    order.Status = OrderStatus.Cancelled;
                    order.DateModified = DateTime.UtcNow;
                    orderRepository.Update(order);

                    foreach (var item in order.OrderListings)
                    {
                        await listingRepository.RestoreStockAsync(item.ListingId, item.Quantity);
                    }

                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success(
                        "Order cancelled successfully.",
                        order.OrderNo);
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}