using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetListingDetails
    {
        public record GetListingDetailsQuery(Guid ListingId) : IRequest<BaseResponse<GetListingDetailsResponse>>;

        public record GetListingDetailsResponse(
            Guid ListingId,
            string FoodName,
            string FoodDescription,
            int Quantity,
            int QuantityPerUnit,
            int RemainingPortion,
            bool IsFree,
            decimal? Price,
            bool PickUpAvailable,
            bool DeliveryAvailable,
            DateTime PickUpStart,
            DateTime PickUpEnd,
            decimal DeliveryFee,
            string Status,
            string VendorName,
            string Address,
            decimal Latitude,
            decimal Longitude,
            double AverageRating,
            int RatingCount,
            List<string> ImageUrls);

        public class GetListingDetailsHandler(
            IListingRepository listingRepository)
            : IRequestHandler<GetListingDetailsQuery, BaseResponse<GetListingDetailsResponse>>
        {
            public async Task<BaseResponse<GetListingDetailsResponse>> Handle(
                GetListingDetailsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var listing = await listingRepository.GetListingAsync(request.ListingId);
                    if (listing is null)
                        return BaseResponse<GetListingDetailsResponse>.Failure("Listing not found.");

                    var ratings = listing.Ratings;
                    var averageRating = ratings.Count > 0 ? ratings.Average(r => r.Stars) : 0;

                    var response = new GetListingDetailsResponse(
                        listing.Id,
                        listing.FoodName,
                        listing.FoodDescription,
                        listing.Quantity,
                        listing.QuantityPerUnit,
                        listing.RemainingPortion,
                        listing.IsFree,
                        listing.Price,
                        listing.PickUpAvailable,
                        listing.DeliveryAvailable,
                        listing.PickUpStart,
                        listing.PickUpEnd,
                        listing.DeliveryFee,
                        listing.Status.ToString(),
                        listing.Vendor.OrganizationName,
                        listing.Address,
                        listing.Latitude,
                        listing.Longitude,
                        Math.Round(averageRating, 1),
                        ratings.Count,
                        listing.ListingImages
                            .OrderByDescending(li => li.IsPrimary)
                            .Select(li => li.ImageUrl)
                            .ToList());

                    return BaseResponse<GetListingDetailsResponse>.Success(
                        "Listing details retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<GetListingDetailsResponse>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}