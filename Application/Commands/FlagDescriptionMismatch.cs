using Application.Common.Dtos;
using Application.Repositories;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class FlagDescriptionMismatch
    {
        public record FlagDescriptionMismatchCommand(
            Guid CustomerUserId,
            Guid OrderId,
            string Note) : IRequest<BaseResponse<string>>;

        public class FlagDescriptionMismatchValidator : AbstractValidator<FlagDescriptionMismatchCommand>
        {
            public FlagDescriptionMismatchValidator()
            {
                RuleFor(x => x.Note)
                    .NotEmpty().WithMessage("Please describe what didn't match.")
                    .MaximumLength(500).WithMessage("Note cannot exceed 500 characters.");
            }
        }

        public class FlagDescriptionMismatchHandler(
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<FlagDescriptionMismatchCommand, BaseResponse<string>>
        {
            public async Task<BaseResponse<string>> Handle(
                FlagDescriptionMismatchCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<string>.Failure("Customer profile not found.");

                    var order = await orderRepository.GetOrderAsync(request.OrderId);
                    if (order is null || order.CustomerId != customer.Id)
                        return BaseResponse<string>.Failure("Order not found.");

                    bool isAwaitingDeliveryConfirmation =
                        order.FulfilmentType == FulfilmentType.Delivery &&
                        order.Delivery is not null &&
                        order.Delivery.Status == DeliveryStatus.Delivered &&
                        order.Status != OrderStatus.Completed;

                    if (order.Status != OrderStatus.Completed && !isAwaitingDeliveryConfirmation)
                        return BaseResponse<string>.Failure("You can only report an issue once the order has been delivered or completed.");

                    if (order.DescriptionMismatchFlagged)
                        return BaseResponse<string>.Failure("You've already reported an issue on this order.");

                    order.DescriptionMismatchFlagged = true;
                    order.DescriptionMismatchNote = request.Note;
                    order.DescriptionMismatchFlaggedAt = DateTime.UtcNow;
                    order.DateModified = DateTime.UtcNow;

                    orderRepository.Update(order);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<string>.Success(
                        "Thanks for letting us know - this has been flagged for review.", "Flagged");
                }
                catch (Exception ex)
                {
                    return BaseResponse<string>.Failure($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}