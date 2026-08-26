using Application.Common.Dtos;
using Application.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllVendors
    {
        public record GetAllVendorsQuery() : IRequest<BaseResponse<List<GetAllVendorsResponse>>>;

        public class GetAllVendorsHandler(
            IVendorRepository vendorRepository)
            : IRequestHandler<GetAllVendorsQuery, BaseResponse<List<GetAllVendorsResponse>>>
        {
            public async Task<BaseResponse<List<GetAllVendorsResponse>>> Handle(
                GetAllVendorsQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    var vendors = await vendorRepository.GetAllVendorsAsync();

                    var response = vendors
                        .Where(v => !v.IsDeleted)
                        .Select(v => new GetAllVendorsResponse(
                            v.Id,
                            v.OrganizationName,
                            v.User.Email,
                            v.PhoneNumber,
                            v.IsApproved,
                            v.Listings.Count,
                            v.DateCreated))
                        .ToList();

                    return BaseResponse<List<GetAllVendorsResponse>>.Success(
                        "Vendors retrieved successfully.", response);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<GetAllVendorsResponse>>.Failure(
                        $"An error occurred: {ex.Message}");
                }
            }
        }

        public record GetAllVendorsResponse(
            Guid VendorId,
            string OrganizationName,
            string Email,
            string PhoneNumber,
            bool IsApproved,
            int TotalListings,
            DateTime DateJoined);
    }
}