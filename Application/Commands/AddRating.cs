using Application.Common.Dtos;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Commands
{
    public class AddRating
    {
        public record AddRatingCommand(Guid CustomerUserId, Guid ListingId, int Stars, string? Comment) : IRequest<BaseResponse<AddRatingResponse>>;

        public class AddRatingValidator : AbstractValidator<AddRatingCommand>
        {
            public AddRatingValidator()
            {
                RuleFor(x => x.ListingId)
                    .NotEmpty().WithMessage("Listing ID is required.");

                RuleFor(x => x.Stars)
                    .InclusiveBetween(1, 5).WithMessage("Stars must be between 1 and 5.");

                RuleFor(x => x.Comment)
                    .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.")
                    .When(x => x.Comment is not null);
            }
        }

        public class AddRatingHandler(
            ICustomerRepository customerRepository,
            IListingRepository listingRepository,
            IOrderRepository orderRepository,
            IRatingRepository ratingRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<AddRatingCommand, BaseResponse<AddRatingResponse>>
        {
            public async Task<BaseResponse<AddRatingResponse>> Handle(
                AddRatingCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var customer = await customerRepository.GetCustomerByUserIdAsync(request.CustomerUserId);
                    if (customer is null)
                        return BaseResponse<AddRatingResponse>.Failure("Customer profile not found.");

                    var listing = await listingRepository.GetListingAsync(request.ListingId);
                    if (listing is null)
                        return BaseResponse<AddRatingResponse>.Failure("Listing not found.");

                    // I want to confirm that the customer has a completed order that contains this listing
                    var customerOrders = await orderRepository.GetOrdersByCustomerAsync(customer.Id);

                    var hasCompletedOrder = customerOrders.Any(o =>
                        o.Status == OrderStatus.Completed &&
                        o.OrderListings.Any(ol => ol.ListingId == request.ListingId));

                    if (!hasCompletedOrder)
                        return BaseResponse<AddRatingResponse>.Failure(
                            "You can only rate a listing after your order has been completed.");

                    var alreadyRated = await ratingRepository
                        .HasCustomerRatedListingAsync(customer.Id, request.ListingId);

                    if (alreadyRated)
                        return BaseResponse<AddRatingResponse>.Failure(
                            "You have already rated this listing.");

                    var rating = new Rating
                    {
                        CustomerId = customer.Id,
                        ListingId = request.ListingId,
                        Stars = request.Stars,
                        Comment = request.Comment,
                        CreatedBy = customer.UserId.ToString(),
                    };

                    await ratingRepository.AddAsync(rating);
                    await unitOfWork.SaveAsync();

                    return BaseResponse<AddRatingResponse>.Success(
                        "Rating submitted successfully.",
                        new AddRatingResponse(
                            rating.Id,
                            listing.FoodName,
                            rating.Stars,
                            rating.Comment));
                }
                catch (Exception ex)
                {
                    return BaseResponse<AddRatingResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }

        public record AddRatingResponse(
            Guid RatingId,
            string FoodName,
            int Stars,
            string? Comment);
    }
}