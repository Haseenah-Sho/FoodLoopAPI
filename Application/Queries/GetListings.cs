using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetListings
    {
        public record GetListingsQuery(bool? IsFree) : IRequest<BaseResponse<ICollection<GetListingsResponse>>>;

        public record GetListingsResponse(
            Guid ListingId,
            string FoodName,
            bool IsFree,
            decimal? Price,
            int Quantity,
            int RemainingPortion,
            bool PickUpAvailable,
            bool DeliveryAvailable,
            DateTime PickUpStart,
            DateTime PickUpEnd,
            string VendorName,
            decimal Latitude,
            decimal Longitude,
            string? PrimaryImageUrl);

        public class GetListingsHandler(
            IListingRepository listingRepository)
            : IRequestHandler<GetListingsQuery, BaseResponse<ICollection<GetListingsResponse>>>
        {
            public async Task<BaseResponse<ICollection<GetListingsResponse>>> Handle(
                GetListingsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var listings = await listingRepository.GetAllActiveListingsAsync();

                    if (request.IsFree.HasValue)
                        listings = listings.Where(l => l.IsFree == request.IsFree.Value).ToList();

                    var response = listings.Select(l => new GetListingsResponse(
                        l.Id,
                        l.FoodName,
                        l.IsFree,
                        l.Price,
                        l.Quantity,
                        l.RemainingPortion,
                        l.PickUpAvailable,
                        l.DeliveryAvailable,
                        l.PickUpStart,
                        l.PickUpEnd,
                        l.Vendor.OrganizationName,
                        l.Latitude,
                        l.Longitude,
                        l.ListingImages.FirstOrDefault(li => li.IsPrimary)?.ImageUrl
                            ?? l.ListingImages.FirstOrDefault()?.ImageUrl
                    )).ToList();

                    return BaseResponse<ICollection<GetListingsResponse>>.Success(
                        "Listings retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<GetListingsResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}