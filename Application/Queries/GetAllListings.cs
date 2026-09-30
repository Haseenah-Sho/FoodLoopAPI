using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllListings
    {
        public record GetAllListingsQuery() : IRequest<BaseResponse<List<GetAllListingsResponse>>>;

        public class GetAllListingsHandler(
            IListingRepository listingRepository)
            : IRequestHandler<GetAllListingsQuery, BaseResponse<List<GetAllListingsResponse>>>
        {
            public async Task<BaseResponse<List<GetAllListingsResponse>>> Handle(
                GetAllListingsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var listings = await listingRepository.GetAllListingsAsync();

                    var response = listings
                        .Where(l => !l.IsDeleted)
                        .Select(l => new GetAllListingsResponse(
                            l.Id,
                            l.FoodName,
                            l.Vendor.OrganizationName,
                            l.IsFree,
                            l.Price,
                            l.Quantity,
                            l.RemainingPortion,
                            l.Status.ToString(),
                            l.DateCreated))
                        .ToList();

                    return BaseResponse<List<GetAllListingsResponse>>.Success(
                        "Food items retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<GetAllListingsResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetAllListingsResponse(
            Guid ListingId,
            string FoodName,
            string VendorName,
            bool IsFree,
            decimal? Price,
            int Quantity,
            int RemainingPortion,
            string Status,
            DateTime CreatedOn);
    }
}