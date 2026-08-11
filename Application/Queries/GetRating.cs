using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetRating
    {
        public record GetRatingQuery(Guid ListingId) : IRequest<BaseResponse<GetRatingResponse>>;

        public record RatingItemResponse(
            string CustomerName,
            int Stars,
            string? Comment,
            DateTime RatedOn);

        public record GetRatingResponse(
            Guid ListingId,
            string FoodName,
            double AverageRating,
            int TotalRatings,
            ICollection<RatingItemResponse> Ratings);

        public class GetRatingHandler(
            IListingRepository listingRepository,
            IRatingRepository ratingRepository)
            : IRequestHandler<GetRatingQuery, BaseResponse<GetRatingResponse>>
        {
            public async Task<BaseResponse<GetRatingResponse>> Handle(
                GetRatingQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var listing = await listingRepository.GetListingAsync(request.ListingId);
                    if (listing is null)
                        return BaseResponse<GetRatingResponse>.Failure("Food item not found.");

                    var ratings = await ratingRepository.GetByListingAsync(request.ListingId);

                    var averageRating = ratings.Count > 0
                        ? Math.Round(ratings.Average(r => r.Stars), 1)
                        : 0;

                    var response = new GetRatingResponse(
                        listing.Id,
                        listing.FoodName,
                        averageRating,
                        ratings.Count,
                        ratings.Select(r => new RatingItemResponse(
                            r.Customer.User.FullName ?? r.Customer.User.UserName,
                            r.Stars,
                            r.Comment,
                            r.DateCreated)).ToList());

                    return BaseResponse<GetRatingResponse>.Success(
                        "Ratings retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<GetRatingResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}