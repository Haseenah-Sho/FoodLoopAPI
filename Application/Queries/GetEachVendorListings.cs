using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetEachVendorListings
    {
        public record GetEachVendorListingsQuery(Guid VendorUserId)
            : IRequest<BaseResponse<ICollection<GetEachVendorListingsResponse>>>;

        public record GetEachVendorListingsResponse(
            Guid ListingId,
            string FoodName,
            bool IsFree,
            decimal? Price,
            int Quantity,
            int RemainingPortion,
            bool PickUpAvailable,
            bool DeliveryAvailable,
            string Status,
            DateTime CreatedOn);

        public class GetEachVendorListingsHandler(
            IVendorRepository vendorRepository,
            IListingRepository listingRepository)
            : IRequestHandler<GetEachVendorListingsQuery, BaseResponse<ICollection<GetEachVendorListingsResponse>>>
        {
            public async Task<BaseResponse<ICollection<GetEachVendorListingsResponse>>> Handle(
                GetEachVendorListingsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendor = await vendorRepository.GetVendorByUserIdAsync(request.VendorUserId);
                    if (vendor is null)
                        return BaseResponse<ICollection<GetEachVendorListingsResponse>>.Failure("Vendor profile not found.");

                    var listings = await listingRepository.GetListingsByVendorAsync(vendor.Id);

                    var response = listings
                        .Where(l => !l.IsDeleted)
                        .OrderByDescending(l => l.DateCreated)
                        .Select(l => new GetEachVendorListingsResponse(
                            l.Id,
                            l.FoodName,
                            l.IsFree,
                            l.Price,
                            l.Quantity,
                            l.RemainingPortion,
                            l.PickUpAvailable,
                            l.DeliveryAvailable,
                            l.Status.ToString(),
                            l.DateCreated))
                        .ToList();

                    return BaseResponse<ICollection<GetEachVendorListingsResponse>>.Success(
                        "Listings retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<ICollection<GetEachVendorListingsResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }
    }
}